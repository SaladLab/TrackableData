using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace TrackableData.MongoDB
{
    public class TrackableDictionaryMongoDbMapper<TKey, TValue>
    {
        private readonly Func<UpdateDefinition<BsonDocument>, TValue, object[], UpdateDefinition<BsonDocument>> _updateGenerator;

        public TrackableDictionaryMongoDbMapper()
        {
            _updateGenerator = TrackableMongoDbMapper.CreatePocoUpdateFunc<TValue>();
        }

        private string CreatePath(IEnumerable<object> keys)
        {
            return string.Join(".", keys.Select(x => x.ToString()));
        }

        public Tuple<FilterDefinition<BsonDocument>, UpdateDefinition<BsonDocument>>
            GenerateUpdateBson(TrackableDictionary<TKey, TValue> trackable,
                               TrackableDictionaryTracker<TKey, TValue> tracker,
                               params object[] keyValues)
        {
            if (keyValues.Length == 0)
                throw new ArgumentException("At least 1 keyValue required.");

            // generate update command for each changes

            var filter = Builders<BsonDocument>.Filter.Eq("_id", keyValues[0]);
            var keyNamespace = keyValues.Length > 1 ? CreatePath(keyValues.Skip(1)) + "." : "";
            UpdateDefinition<BsonDocument> update = null;
            foreach (var change in tracker.ChangeMap)
            {
                switch (change.Value.Operation)
                {
                    case TrackableDictionaryOperation.Add:
                    case TrackableDictionaryOperation.Modify:
                        update = update == null
                                     ? Builders<BsonDocument>.Update.Set(keyNamespace + change.Key,
                                                                         change.Value.NewValue)
                                     : update.Set(keyNamespace + change.Key, change.Value.NewValue);
                        break;

                    case TrackableDictionaryOperation.Remove:
                        update = update == null
                                     ? Builders<BsonDocument>.Update.Unset(keyNamespace + change.Key)
                                     : update.Unset(keyNamespace + change.Key);
                        break;
                }
            }

            // check each value for TrackableValue

            if (trackable != null && _updateGenerator != null)
            {
                foreach (var item in trackable)
                {
                    var trackableValue = (ITrackable)item.Value;
                    if (trackableValue.Changed == false || tracker.ChangeMap.ContainsKey(item.Key))
                        continue;

                    update = _updateGenerator(update, item.Value, keyValues.Skip(1).Concat(new object[] { item.Key}).ToArray());
                }
            }

            return Tuple.Create(filter, update);
        }

        public async Task<TrackableDictionary<TKey, TValue>> LoadAsync(IMongoCollection<BsonDocument> collection, params object[] keyValues)
        {
            if (keyValues.Length == 0)
                throw new ArgumentException("At least 1 keyValue required.");

            BsonDocument doc = null;

            if (keyValues.Length == 1)
            {
                doc = await collection.Find(Builders<BsonDocument>.Filter.Eq("_id", keyValues[0])).FirstOrDefaultAsync();
            }
            else
            {
                // partial query

                var keyPath = keyValues.Length > 1 ? CreatePath(keyValues.Skip(1)) : "";
                var partialDoc = await collection.Find(Builders<BsonDocument>.Filter.Eq("_id", keyValues[0]))
                                                 .Project(Builders<BsonDocument>.Projection.Include(keyPath))
                                                 .FirstOrDefaultAsync();
                if (partialDoc != null)
                {
                    for (int i = 1; i < keyValues.Length; i++)
                    {
                        BsonValue partialValue;
                        if (partialDoc.TryGetValue(keyValues[i].ToString(), out partialValue) == false)
                        {
                            partialDoc = null;
                            break;
                        }
                        partialDoc = (BsonDocument)partialValue;
                    }
                    doc = partialDoc;
                }
            }

            if (doc == null)
                return null;

            var dictionary = new TrackableDictionary<TKey, TValue>();
            foreach (var element in doc.Elements)
            {
                if (element.Name == "_id")
                    continue;
                var key = (TKey)Convert.ChangeType(element.Name, typeof(TKey));
                var value = (TValue)(element.Value.IsBsonDocument
                                         ? BsonSerializer.Deserialize(element.Value.AsBsonDocument, typeof(TValue))
                                         : Convert.ChangeType(element.Value, typeof(TValue)));
                dictionary.Add(key, value);
            }
            return dictionary;
        }

        public Task<UpdateResult> SaveAsync(IMongoCollection<BsonDocument> collection,
                                            TrackableDictionary<TKey, TValue> trackable,
                                            params object[] keyValues)
        {
            var ret = GenerateUpdateBson(
                trackable, (TrackableDictionaryTracker<TKey, TValue>)trackable.Tracker, keyValues);
            return collection.UpdateOneAsync(ret.Item1, ret.Item2, new UpdateOptions { IsUpsert = true });
        }

        public Task<UpdateResult> SaveAsync(IMongoCollection<BsonDocument> collection,
                                            IDictionaryTracker<TKey, TValue> tracker,
                                            params object[] keyValues)
        {
            return SaveAsync(collection, (TrackableDictionaryTracker<TKey, TValue>)tracker, keyValues);
        }

        public Task<UpdateResult> SaveAsync(IMongoCollection<BsonDocument> collection,
                                            TrackableDictionaryTracker<TKey, TValue> tracker,
                                            params object[] keyValues)
        {
            if (tracker.HasChange == false)
                return null;

            var ret = GenerateUpdateBson(null, tracker, keyValues);
            return collection.UpdateOneAsync(ret.Item1, ret.Item2, new UpdateOptions { IsUpsert = true });
        }
    }
}
