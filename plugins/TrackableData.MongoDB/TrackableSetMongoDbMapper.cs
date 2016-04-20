using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace TrackableData.MongoDB
{
    public class TrackableSetMongoDbMapper<T>
    {
        public TrackableSetMongoDbMapper()
        {
            TypeMapper.RegisterMap(typeof(T));
        }

        public BsonArray ConvertToBsonArray(ICollection<T> set)
        {
            var bson = new BsonArray();
            foreach (var item in set)
                bson.Add(BsonDocumentWrapper.Create(typeof(T), item));
            return bson;
        }

        public void ConvertToSet(BsonArray bson, ICollection<T> set)
        {
            set.Clear();
            foreach (var item in bson)
            {
                var value = (T)(item.IsBsonDocument
                                    ? BsonSerializer.Deserialize(item.AsBsonDocument, typeof(T))
                                    : Convert.ChangeType(item, typeof(T)));
                set.Add(value);
            }
        }

        public TrackableSet<T> ConvertToTrackableSet(BsonArray bson)
        {
            var set = new TrackableSet<T>();
            ConvertToSet(bson, set);
            return set;
        }

        public TrackableSet<T> ConvertToTrackableSet(BsonDocument doc, params object[] partialKeys)
        {
            var partialDoc = DocumentHelper.QueryValue(doc, partialKeys);
            if (partialDoc == null)
                return null;

            var set = new TrackableSet<T>();
            ConvertToSet(partialDoc.AsBsonArray, set);
            return set;
        }

        public UpdateDefinition<BsonDocument> BuildUpdatesForCreate(
            UpdateDefinition<BsonDocument> update, ICollection<T> set, params object[] keyValues)
        {
            var valuePath = DocumentHelper.ToDotPath(keyValues);
            var bson = ConvertToBsonArray(set);
            return update == null
                       ? Builders<BsonDocument>.Update.Set(valuePath, bson)
                       : update.Set(valuePath, bson);
        }

        public List<UpdateDefinition<BsonDocument>> BuildUpdatesForSave(
            UpdateDefinition<BsonDocument> update, TrackableSetTracker<T> tracker, params object[] keyValues)
        {
            var updates = new List<UpdateDefinition<BsonDocument>>();
            var keyNamespace = DocumentHelper.ToDotPath(keyValues);

            if (tracker.AddValues.Any())
            {
                updates.Add(update == null
                    ? Builders<BsonDocument>.Update.AddToSetEach(keyNamespace, tracker.AddValues)
                    : update.AddToSetEach(keyNamespace, tracker.AddValues));
                update = null;
            }

            if (tracker.RemoveValues.Any())
            {
                updates.Add(update == null
                    ? Builders<BsonDocument>.Update.PullAll(keyNamespace, tracker.RemoveValues)
                    : update.PullAll(keyNamespace, tracker.RemoveValues));
                update = null;
            }

            if (update != null)
                updates.Add(update);

            return updates;
        }

        public Task CreateAsync(IMongoCollection<BsonDocument> collection, ICollection<T> set, params object[] keyValues)
        {
            if (keyValues.Length < 2)
                throw new ArgumentException("At least 2 keyValue required.");

            var update = BuildUpdatesForCreate(null, set, keyValues.Skip(1).ToArray());
            return collection.UpdateOneAsync(
                Builders<BsonDocument>.Filter.Eq("_id", keyValues[0]),
                update,
                new UpdateOptions { IsUpsert = true });
        }

        public Task<int> DeleteAsync(IMongoCollection<BsonDocument> collection, params object[] keyValues)
        {
            return DocumentHelper.DeleteAsync(collection, keyValues);
        }

        public async Task<TrackableSet<T>> LoadAsync(IMongoCollection<BsonDocument> collection,
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

            return ConvertToTrackableSet(doc.AsBsonArray);
        }

        public Task SaveAsync(IMongoCollection<BsonDocument> collection,
                              ISetTracker<T> tracker,
                              params object[] keyValues)
        {
            return SaveAsync(collection, (TrackableSetTracker<T>)tracker, keyValues);
        }

        public async Task SaveAsync(IMongoCollection<BsonDocument> collection,
                                    TrackableSetTracker<T> tracker,
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
