using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace TrackableData.MongoDB
{
    public class TrackablePocoMongoDbMapper<T>
        where T : ITrackablePoco<T>
    {
        private readonly Type _trackableType;
        private readonly PropertyInfo _idProperty;

        public TrackablePocoMongoDbMapper()
        {
            _trackableType = TrackableResolver.GetPocoTrackerType(typeof(T));
            TypeMapper.RegisterTrackablePocoMap(_trackableType);

            var type = typeof(T);
            foreach (var property in type.GetProperties())
            {
                if (property.Name.ToLower() == "id")
                    _idProperty = property;
            }
        }

        #region Conversion between T and Bson

        public BsonDocument ConvertToBsonDocument(T poco)
        {
            return BsonDocumentWrapper.Create(_trackableType, poco);
        }

        public T ConvertToTrackablePoco(BsonDocument doc)
        {
            return (T)BsonSerializer.Deserialize(doc, _trackableType);
        }

        public T ConvertToTrackablePoco(BsonDocument doc, params object[] partialKeys)
        {
            var partialDoc = DocumentHelper.QueryValue(doc, partialKeys);
            if (partialDoc == null)
                return default(T);

            return ConvertToTrackablePoco(partialDoc.AsBsonDocument);
        }

        #endregion

        #region MongoDB Command Builder

        public UpdateDefinition<BsonDocument> BuildUpdatesForCreate(
            UpdateDefinition<BsonDocument> update, T poco, params object[] keyValues)
        {
            var keyPath = DocumentHelper.ToDotPath(keyValues);
            var bson = ConvertToBsonDocument(poco);
            return (update == null)
                    ? Builders<BsonDocument>.Update.Set(keyPath, bson)
                    : update.Set(keyPath, bson);
        }

        public UpdateDefinition<BsonDocument> BuildUpdatesForSave(
            UpdateDefinition<BsonDocument> update, TrackablePocoTracker<T> tracker, params object[] keyValues)
        {
            var keyNamespace = DocumentHelper.ToDotPathWithTrailer(keyValues);
            foreach (var change in tracker.ChangeMap)
            {
                update = (update == null)
                    ? Builders<BsonDocument>.Update.Set(keyNamespace + change.Key.Name, change.Value.NewValue)
                    : update.Set(keyNamespace + change.Key.Name, change.Value.NewValue);
            }
            return update;
        }

        #endregion

        #region Helpers

        public async Task CreateAsync(IMongoCollection<BsonDocument> collection, T value, params object[] keyValues)
        {
            var bson = ConvertToBsonDocument(value);

            if (_idProperty != null)
            {
                if (keyValues.Length == 0)
                {
                    // ConvertToBsonDocument uses BsonDocumentWrapper to serialize T
                    // but it cannot handle returned _id property.
                    // To workaround this limitation, force value to be serailized directly.
                    bson = value.ToBsonDocument(_trackableType);

                    await collection.InsertOneAsync(bson);

                    var idValue = Convert.ChangeType(bson["_id"], _idProperty.PropertyType);
                    _idProperty.SetValue(value, idValue);
                }
                else
                {
                    // TODO: throw exception if already item exists ?

                    var keyPath = keyValues.Length > 1 ? DocumentHelper.ToDotPath(keyValues.Skip(1)) + "." : "";
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

                    var setPath = DocumentHelper.ToDotPath(keyValues.Skip(1));
                    await collection.UpdateOneAsync(
                        Builders<BsonDocument>.Filter.Eq("_id", keyValues[0]),
                        Builders<BsonDocument>.Update.Set(setPath, bson));
                }
            }
        }

        public Task<int> DeleteAsync(IMongoCollection<BsonDocument> collection, params object[] keyValues)
        {
            return DocumentHelper.DeleteAsync(collection, keyValues);
        }

        public async Task<T> LoadAsync(IMongoCollection<BsonDocument> collection, params object[] keyValues)
        {
            if (keyValues.Length == 0)
                throw new ArgumentException("At least 1 keyValue required.");

            BsonDocument doc;

            if (keyValues.Length == 1)
            {
                doc = await collection.Find(Builders<BsonDocument>.Filter.Eq("_id", keyValues[0]))
                                      .FirstOrDefaultAsync();
            }
            else
            {
                // partial query

                var partialKeys = keyValues.Skip(1);
                var partialPath = DocumentHelper.ToDotPath(partialKeys);
                var partialDoc = await collection.Find(Builders<BsonDocument>.Filter.Eq("_id", keyValues[0]))
                                                 .Project(Builders<BsonDocument>.Projection.Include(partialPath))
                                                 .FirstOrDefaultAsync();
                doc = DocumentHelper.QueryValue(partialDoc, keyValues.Skip(1)) as BsonDocument;
            }

            if (doc == null)
                return default(T);

            return (T)BsonSerializer.Deserialize(doc, _trackableType);
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
            if (keyValues.Length == 0)
                throw new ArgumentException("At least 1 keyValue required.");

            if (tracker.HasChange == false)
                return Task.FromResult((UpdateResult)null);

            return collection.UpdateOneAsync(
                Builders<BsonDocument>.Filter.Eq("_id", keyValues[0]), 
                BuildUpdatesForSave(null, tracker, keyValues.Skip(1).ToArray()));
        }

        #endregion
    }
}
