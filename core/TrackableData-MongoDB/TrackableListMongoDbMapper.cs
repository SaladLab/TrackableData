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
        private string CreatePath(IEnumerable<object> keys)
        {
            return string.Join(".", keys.Select(x => x.ToString()));
        }

        public Tuple<FilterDefinition<BsonDocument>, UpdateDefinition<BsonDocument>, int>
            GenerateUpdateBson(TrackableListTracker<T> tracker, int cursor, params object[] keyValues)
        {
            if (keyValues.Length < 2)
                throw new ArgumentException("At least 2 keyValues required.");

            var filter = Builders<BsonDocument>.Filter.Eq("_id", keyValues[0]);
            var keyNamespace = CreatePath(keyValues.Skip(1));
            UpdateDefinition<BsonDocument> update = null;

            // Multiple push-back batching optimization
            if (cursor == 0 && tracker.ChangeList.Count > 1 && tracker.ChangeList.All(c => c.Operation == TrackableListOperation.PushBack))
            {
                return Tuple.Create(
                    filter,
                    Builders<BsonDocument>.Update.PushEach(keyNamespace, tracker.ChangeList.Select(c => c.NewValue)),
                    tracker.ChangeList.Count);
            }

            // Multiple push-front batching optimization
            if (cursor == 0 && tracker.ChangeList.Count > 1 && tracker.ChangeList.All(c => c.Operation == TrackableListOperation.PushFront))
            {
                return Tuple.Create(
                    filter,
                    Builders<BsonDocument>.Update.PushEach(keyNamespace, tracker.ChangeList.Select(c => c.NewValue).Reverse()),
                    tracker.ChangeList.Count);
            }

            // List update can process only one change each time
            var change = tracker.ChangeList[cursor];
            switch (change.Operation)
            {
                case TrackableListOperation.Insert:
                    update = Builders<BsonDocument>.Update.PushEach(keyNamespace, new[] { change.NewValue }, position: change.Index);
                    break;

                case TrackableListOperation.Remove:
                    throw new Exception("Remove operation is not supported!");

                case TrackableListOperation.Modify:
                    update = Builders<BsonDocument>.Update.Set(keyNamespace + "." + change.Index, change.NewValue);
                    break;

                case TrackableListOperation.PushFront:
                    update = Builders<BsonDocument>.Update.PushEach(keyNamespace, new[] { change.NewValue }, position: 0);
                    break;

                case TrackableListOperation.PushBack:
                    update = Builders<BsonDocument>.Update.Push(keyNamespace, change.NewValue);
                    break;

                case TrackableListOperation.PopFront:
                    update = Builders<BsonDocument>.Update.PopFirst(keyNamespace);
                    break;

                case TrackableListOperation.PopBack:
                    update = Builders<BsonDocument>.Update.PopLast(keyNamespace);
                    break;
            }
            return Tuple.Create(filter, update, 1);
        }

        public async Task<TrackableList<T>> LoadAsync(IMongoCollection<BsonDocument> collection, params object[] keyValues)
        {
            if (keyValues.Length < 2)
                throw new ArgumentException("At least 2 keyValue required.");

            // partial query

            var keyPath = keyValues.Length > 1 ? CreatePath(keyValues.Skip(1)) : "";
            var doc = await collection.Find(Builders<BsonDocument>.Filter.Eq("_id", keyValues[0]))
                                                .Project(Builders<BsonDocument>.Projection.Include(keyPath))
                                                .FirstOrDefaultAsync();
            if (doc == null)
                return null;

            BsonValue partialDoc = doc;
            for (int i = 1; i < keyValues.Length; i++)
            {
                if (partialDoc.IsBsonDocument == false)
                    return null;

                BsonValue partialValue;
                if (partialDoc.AsBsonDocument.TryGetValue(keyValues[i].ToString(), out partialValue) == false)
                    return null;

                partialDoc = partialValue;
            }

            if (partialDoc.IsBsonArray == false)
                throw new Exception($"Data should be an array. ({doc.BsonType})");

            var list = new TrackableList<T>();
            foreach (var arrayValue in partialDoc.AsBsonArray)
            {
                var value = (T)(arrayValue.IsBsonDocument
                                    ? BsonSerializer.Deserialize(arrayValue.AsBsonDocument, typeof(T))
                                    : Convert.ChangeType(arrayValue, typeof(T)));
                list.Add(value);
            }
            return list;
        }

        public async Task<UpdateResult> SaveAsync(IMongoCollection<BsonDocument> collection,
                                                  TrackableList<T> trackable,
                                                  params object[] keyValues)
        {
            return null;
        }

        public Task SaveAsync(IMongoCollection<BsonDocument> collection,
                                            IListTracker<T> tracker,
                                            params object[] keyValues)
        {
            return SaveAsync(collection, (TrackableListTracker<T>)tracker, keyValues);
        }

        public async Task SaveAsync(IMongoCollection<BsonDocument> collection,
                                            TrackableListTracker<T>tracker,
                                            params object[] keyValues)
        {
            if (tracker.HasChange == false)
                return;

            var cursor = 0;
            while (cursor < tracker.ChangeList.Count)
            {
                var ret = GenerateUpdateBson(tracker, cursor, keyValues);
                await collection.UpdateOneAsync(ret.Item1, ret.Item2, new UpdateOptions { IsUpsert = true });
                cursor += ret.Item3;
            }
        }
    }
}
