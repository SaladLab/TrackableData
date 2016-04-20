using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace TrackableData.MongoDB
{
    public class TrackableListMongoDbMapper<T>
    {
        public TrackableListMongoDbMapper()
        {
            TypeMapper.RegisterMap(typeof(T));
        }

        public BsonArray ConvertToBsonArray(IList<T> list)
        {
            var bson = new BsonArray();
            foreach (var item in list)
                bson.Add(BsonDocumentWrapper.Create(typeof(T), item));
            return bson;
        }

        public void ConvertToList(BsonArray bson, IList<T> list)
        {
            list.Clear();
            foreach (var item in bson)
            {
                var value = (T)(item.IsBsonDocument
                                    ? BsonSerializer.Deserialize(item.AsBsonDocument, typeof(T))
                                    : Convert.ChangeType(item, typeof(T)));
                list.Add(value);
            }
        }

        public TrackableList<T> ConvertToTrackableList(BsonArray bson)
        {
            var list = new TrackableList<T>();
            ConvertToList(bson, list);
            return list;
        }

        public TrackableList<T> ConvertToTrackableList(BsonDocument doc, params object[] partialKeys)
        {
            var partialDoc = DocumentHelper.QueryValue(doc, partialKeys);
            if (partialDoc == null)
                return null;

            var list = new TrackableList<T>();
            ConvertToList(partialDoc.AsBsonArray, list);
            return list;
        }

        public UpdateDefinition<BsonDocument> BuildUpdatesForCreate(
            UpdateDefinition<BsonDocument> update, IList<T> list, params object[] keyValues)
        {
            var valuePath = DocumentHelper.ToDotPath(keyValues);
            var bson = ConvertToBsonArray(list);
            return update == null
                       ? Builders<BsonDocument>.Update.Set(valuePath, bson)
                       : update.Set(valuePath, bson);
        }

        public List<UpdateDefinition<BsonDocument>> BuildUpdatesForSave(
            UpdateDefinition<BsonDocument> update, TrackableListTracker<T> tracker, params object[] keyValues)
        {
            var keyNamespace = DocumentHelper.ToDotPath(keyValues);

            // Multiple push-back batching optimization
            if (tracker.ChangeList.Count > 1 &&
                tracker.ChangeList.All(c => c.Operation == TrackableListOperation.PushBack))
            {
                var newValues = tracker.ChangeList.Select(c => c.NewValue);
                return new List<UpdateDefinition<BsonDocument>>
                {
                    update == null
                        ? Builders<BsonDocument>.Update.PushEach(keyNamespace, newValues)
                        : update.PushEach(keyNamespace, newValues)
                };
            }

            // Multiple push-front batching optimization
            if (tracker.ChangeList.Count > 1 &&
                tracker.ChangeList.All(c => c.Operation == TrackableListOperation.PushFront))
            {
                var newValues = tracker.ChangeList.Select(c => c.NewValue).Reverse();
                return new List<UpdateDefinition<BsonDocument>>
                {
                    update == null
                        ? Builders<BsonDocument>.Update.PushEach(keyNamespace, newValues, position: 0)
                        : update.PushEach(keyNamespace, newValues, position: 0)
                };
            }

            // List update can process only one change each time
            var updates = new List<UpdateDefinition<BsonDocument>>();
            foreach (var change in tracker.ChangeList)
            {
                switch (change.Operation)
                {
                    case TrackableListOperation.Insert:
                        updates.Add(update == null
                            ? Builders<BsonDocument>.Update.PushEach(keyNamespace, new[] { change.NewValue }, position: change.Index)
                            : update.PushEach(keyNamespace, new[] { change.NewValue }, position: change.Index));
                        update = null;
                        break;

                    case TrackableListOperation.Remove:
                        throw new Exception("Remove operation is not supported!");

                    case TrackableListOperation.Modify:
                        updates.Add(update == null
                            ? Builders<BsonDocument>.Update.Set(keyNamespace + "." + change.Index, change.NewValue)
                            : update.Set(keyNamespace + "." + change.Index, change.NewValue));
                        break;

                    case TrackableListOperation.PushFront:
                        updates.Add(update == null
                            ? Builders<BsonDocument>.Update.PushEach(keyNamespace, new[] { change.NewValue }, position: 0)
                            : update.PushEach(keyNamespace, new[] { change.NewValue }, position: 0));
                        break;

                    case TrackableListOperation.PushBack:
                        updates.Add(update == null
                            ? Builders<BsonDocument>.Update.Push(keyNamespace, change.NewValue)
                            : update.Push(keyNamespace, change.NewValue));
                        break;

                    case TrackableListOperation.PopFront:
                        updates.Add(update == null
                            ? Builders<BsonDocument>.Update.PopFirst(keyNamespace)
                            : update.PopFirst(keyNamespace));
                        break;

                    case TrackableListOperation.PopBack:
                        updates.Add(update == null
                            ? Builders<BsonDocument>.Update.PopLast(keyNamespace)
                            : update.PopLast(keyNamespace));
                        break;
                }
            }

            if (update != null)
                updates.Add(update);

            return updates;
        }

        public Task CreateAsync(IMongoCollection<BsonDocument> collection, IList<T> list, params object[] keyValues)
        {
            if (keyValues.Length < 2)
                throw new ArgumentException("At least 2 keyValue required.");

            var update = BuildUpdatesForCreate(null, list, keyValues.Skip(1).ToArray());
            return collection.UpdateOneAsync(
                Builders<BsonDocument>.Filter.Eq("_id", keyValues[0]),
                update,
                new UpdateOptions { IsUpsert = true });
        }

        public Task<int> DeleteAsync(IMongoCollection<BsonDocument> collection, params object[] keyValues)
        {
            return DocumentHelper.DeleteAsync(collection, keyValues);
        }

        public async Task<TrackableList<T>> LoadAsync(IMongoCollection<BsonDocument> collection,
                                                      params object[] keyValues)
        {
            if (keyValues.Length < 2)
                throw new ArgumentException("At least 2 keyValue required.");

            // partial query

            var keyPath = keyValues.Length > 1 ? DocumentHelper.ToDotPath(keyValues.Skip(1)) : "";
            var partialDoc = await collection.Find(Builders<BsonDocument>.Filter.Eq("_id", keyValues[0]))
                                             .Project(Builders<BsonDocument>.Projection.Include(keyPath))
                                             .FirstOrDefaultAsync();
            if (partialDoc == null)
                return null;

            var doc = DocumentHelper.QueryValue(partialDoc, keyValues.Skip(1));
            if (doc == null)
                return null;

            if (doc.IsBsonArray == false)
                throw new Exception($"Data should be an array. ({doc.BsonType})");

            return ConvertToTrackableList(doc.AsBsonArray);
        }

        public Task SaveAsync(IMongoCollection<BsonDocument> collection,
                              IListTracker<T> tracker,
                              params object[] keyValues)
        {
            return SaveAsync(collection, (TrackableListTracker<T>)tracker, keyValues);
        }

        public async Task SaveAsync(IMongoCollection<BsonDocument> collection,
                                    TrackableListTracker<T> tracker,
                                    params object[] keyValues)
        {
            if (keyValues.Length < 2)
                throw new ArgumentException("At least 2 keyValue required.");

            if (tracker.HasChange == false)
                return;

            var filter = Builders<BsonDocument>.Filter.Eq("_id", keyValues[0]);
            foreach (var update in BuildUpdatesForSave(null, tracker, keyValues.Skip(1).ToArray()))
            {
                await collection.UpdateOneAsync(
                    filter, update, new UpdateOptions { IsUpsert = true });
            }
        }
    }
}
