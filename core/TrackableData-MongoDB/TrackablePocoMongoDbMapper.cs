using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace TrackableData.MongoDB
{
    public class TrackablePocoMongoDbMapper<T>
        where T : ITrackablePoco
    {
        private readonly Type _trackableType;
        private readonly PropertyInfo _idProperty;

        public TrackablePocoMongoDbMapper()
        {
            var trackableTypeName = typeof(T).Namespace + "." + ("Trackable" + typeof(T).Name.Substring(1));
            _trackableType = typeof(T).Assembly.GetType(trackableTypeName);

            RegisterMapper();

            var type = typeof(T);
            foreach (var property in type.GetProperties())
            {
                if (property.Name.ToLower() == "id")
                    _idProperty = property;
            }
        }

        public bool RegisterMapper()
        {
            if (BsonClassMap.IsClassMapRegistered(_trackableType))
                return false;

            var classMap = new BsonClassMap(_trackableType);
            classMap.AutoMap();
            classMap.UnmapMember(_trackableType.GetProperty("Tracker"));
            BsonClassMap.RegisterClassMap(classMap);
            return true;
        }

        // When T has id
        //
        //   O: T.id -> { T }
        //      k[0] -> { k[1]: { T.id: { ... } } } with keyValues
        //
        //   X: objectid -> { T }
        //      k[0] -> { k[1]: { ... } } with keyValues

        private string CreatePath(IEnumerable<object> keys)
        {
            return string.Join(".", keys.Select(x => x.ToString()));
        }

        public Tuple<FilterDefinition<BsonDocument>, UpdateDefinition<BsonDocument>>
            GenerateUpdateBson(TrackablePocoTracker<T> tracker, params object[] keyValues)
        {
            if (keyValues.Length == 0)
                throw new ArgumentException("At least 1 keyValue required.");

            var filter = Builders<BsonDocument>.Filter.Eq("_id", keyValues[0]);
            var keyNamespace = keyValues.Length > 1 ? CreatePath(keyValues.Skip(1)) + "." : "";
            UpdateDefinition<BsonDocument> update = null;
            foreach (var change in tracker.ChangeMap)
            {
                if (update == null)
                    update = Builders<BsonDocument>.Update.Set(keyNamespace + change.Key.Name, change.Value.NewValue);
                else
                    update = update.Set(keyNamespace + change.Key.Name, change.Value.NewValue);
            }

            return Tuple.Create(filter, update);
        }

        public async Task CreateAsync(IMongoCollection<BsonDocument> collection, T value, params object[] keyValues)
        {
            var bson = value.ToBsonDocument(_trackableType);

            if (_idProperty != null)
            {
                if (keyValues.Length == 0)
                {
                    await collection.InsertOneAsync(bson);

                    // TODO: ObjectID 타입에 따라 분기
                    _idProperty.SetValue(value, bson["_id"].AsObjectId);
                }
                else
                {
                    // TODO: throw exception if already item exists ?

                    var keyPath = keyValues.Length > 1 ? CreatePath(keyValues.Skip(1)) + "." : "";
                    var setPath = keyPath + _idProperty.GetValue(value);
                    await collection.UpdateOneAsync(
                        Builders<BsonDocument>.Filter.Eq("_id", keyValues[0]),
                        Builders<BsonDocument>.Update.Set(setPath, bson),
                        new UpdateOptions { IsUpsert = true });
                }
            }
            else
            {
                if (keyValues.Length == 0)
                {
                    await collection.InsertOneAsync(bson);
                }
                else if (keyValues.Length == 1)
                {
                    bson["_id"] = BsonValue.Create(keyValues[0]);
                    await collection.InsertOneAsync(bson);
                }
                else
                {
                    // TODO: throw exception if already item exists ?

                    var setPath = CreatePath(keyValues.Skip(1));
                    await collection.UpdateOneAsync(
                        Builders<BsonDocument>.Filter.Eq("_id", keyValues[0]),
                        Builders<BsonDocument>.Update.Set(setPath, bson));
                }
            }
        }

        public async Task<int> RemoveAsync(IMongoCollection<BsonDocument> collection, params object[] keyValues)
        {
            if (keyValues.Length == 0)
                throw new ArgumentException("At least 1 keyValue required.");

            if (keyValues.Length == 1)
            {
                var ret = await collection.DeleteOneAsync(Builders<BsonDocument>.Filter.Eq("_id", keyValues[0]));
                return ret != null ? (int)ret.DeletedCount : 0;
            }
            else
            {
                var keyPath = CreatePath(keyValues.Skip(1));
                var ret = await collection.UpdateOneAsync(Builders<BsonDocument>.Filter.Eq("_id", keyValues[0]),
                                                          Builders<BsonDocument>.Update.Unset(keyPath));
                return ret != null ? (int)ret.ModifiedCount : 0;
            }
        }

        public async Task<T> LoadAsync(IMongoCollection<BsonDocument> collection, params object[] keyValues)
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
                return default(T);

            var value = (T)BsonSerializer.Deserialize(doc, _trackableType);
            return value;
        }

        public Task<UpdateResult> SaveAsync(IMongoCollection<BsonDocument> collection,
                                            IPocoTracker<T> tracker,
                                            params object[] keyValues)
        {
            return SaveAsync(collection, (TrackablePocoTracker<T>)tracker, keyValues);
        }

        public Task<UpdateResult> SaveAsync(IMongoCollection<BsonDocument> collection,
                                            TrackablePocoTracker<T> tracker,
                                            params object[] keyValues)
        {
            if (tracker.HasChange == false)
                return null;

            var ret = GenerateUpdateBson(tracker, keyValues);
            return collection.UpdateOneAsync(ret.Item1, ret.Item2);
        }
    }
}
