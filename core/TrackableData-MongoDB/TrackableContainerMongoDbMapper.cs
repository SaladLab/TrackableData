using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Reflection;

namespace TrackableData.MongoDB
{
    public class TrackableContainerMongoDbMapper<T>
        where T : ITrackableContainer<T>
    {
        private readonly Type _trackableType;

        private class PropertyItem
        {
            public string Name;
            public PropertyInfo Property;
            public PropertyInfo TrackerProperty;
            public object Mapper;
            public Action<T, BsonDocument> ExportToBson;
            public Action<BsonDocument, T> ImportFromBson;
            public Func<UpdateDefinition<BsonDocument>, IContainerTracker<T>, IEnumerable<object>, UpdateDefinition<BsonDocument>> SaveChanges;
        }

        private readonly PropertyItem[] PropertyItems;

        public TrackableContainerMongoDbMapper()
        {
            _trackableType = TrackableResolver.GetContainerTrackerType(typeof(T));

            PropertyItems = ConstructPropertyItems();
        }

        #region Property Accessor

        private static PropertyItem[] ConstructPropertyItems()
        {
            var trackerType = TrackerResolver.GetDefaultTracker(typeof(T));

            var propertyItems = new List<PropertyItem>();
            foreach (var property in typeof(T).GetProperties())
            {
                var item = new PropertyItem
                {
                    Name = property.Name,
                    Property = property,
                    TrackerProperty = trackerType.GetProperty(property.Name + "Tracker")
                };

                if (TrackableResolver.IsTrackablePoco(property.PropertyType))
                {
                    typeof(TrackableContainerMongoDbMapper<T>)
                        .GetMethod("BuildTrackablePocoProperty", BindingFlags.Static | BindingFlags.NonPublic)
                        .MakeGenericMethod(TrackableResolver.GetPocoType(property.PropertyType))
                        .Invoke(null, new object[] { item });
                }
                else if (TrackableResolver.IsTrackableDictionary(property.PropertyType))
                {
                    typeof(TrackableContainerMongoDbMapper<T>)
                        .GetMethod("BuildTrackableDictionaryProperty", BindingFlags.Static | BindingFlags.NonPublic)
                        .MakeGenericMethod(property.PropertyType.GetGenericArguments())
                        .Invoke(null, new object[] { item });
                }
                else if (TrackableResolver.IsTrackableList(property.PropertyType))
                {
                    typeof(TrackableContainerMongoDbMapper<T>)
                        .GetMethod("BuildTrackableListProperty", BindingFlags.Static | BindingFlags.NonPublic)
                        .MakeGenericMethod(property.PropertyType.GetGenericArguments())
                        .Invoke(null, new object[] { item });
                }
                else
                {
                    throw new InvalidOperationException("Cannot resolve property: " + property.Name);
                }

                propertyItems.Add(item);
            }
            return propertyItems.ToArray();
        }

        private static void BuildTrackablePocoProperty<TPoco>(PropertyItem item)
              where TPoco : ITrackablePoco<TPoco>
        {
            var mapper = new TrackablePocoMongoDbMapper<TPoco>();
            item.Mapper = mapper;

            item.ExportToBson = (container, doc) =>
            {
                var value = (TPoco)item.Property.GetValue(container);
                if (value != null)
                    doc.Add(item.Property.Name, mapper.ConvertToBsonDocument(value));
            };
            item.ImportFromBson = (doc, container) =>
            {
                var value = mapper.ConvertToTrackablePoco(doc, item.Property.Name);
                item.Property.SetValue(container, value);
            };
            item.SaveChanges = (update, tracker, keyValues) =>
            {
                var valueTracker = (TrackablePocoTracker<TPoco>)item.TrackerProperty.GetValue(tracker);
                if (valueTracker.HasChange)
                {
                    update = mapper.BuildUpdatesForSave(
                        update, valueTracker,
                        keyValues.Concat(new object[] { item.Property.Name }).ToArray());
                }
                return update;
            };
        }

        private static void BuildTrackableDictionaryProperty<TKey, TValue>(PropertyItem item)
        {
            var mapper = new TrackableDictionaryMongoDbMapper<TKey, TValue>();
            item.Mapper = mapper;

            item.ExportToBson = (container, doc) =>
            {
                var value = (IDictionary<TKey, TValue>)item.Property.GetValue(container);
                if (value != null)
                    doc.Add(item.Property.Name, mapper.ConvertToBsonDocument(value));
            };
            item.ImportFromBson = (doc, container) =>
            {
                var value = mapper.ConvertToTrackableDictionary(doc, item.Property.Name);
                item.Property.SetValue(container, value);
            };
            item.SaveChanges = (update, tracker, keyValues) =>
            {
                var valueTracker = (TrackableDictionaryTracker<TKey, TValue>)item.TrackerProperty.GetValue(tracker);
                if (valueTracker.HasChange)
                {
                    update = mapper.BuildUpdatesForSave(
                        update, valueTracker,
                        keyValues.Concat(new object[] { item.Property.Name }).ToArray());
                }
                return update;
            };
        }

        private static void BuildTrackableListProperty<TValue>(PropertyItem item)
        {
            var mapper = new TrackableListMongoDbMapper<TValue>();
            item.Mapper = mapper;

            item.ExportToBson = (container, doc) =>
            {
                var value = (IList<TValue>)item.Property.GetValue(container);
                if (value != null)
                    doc.Add(item.Property.Name, mapper.ConvertToBsonArray(value));
            };
            item.ImportFromBson = (doc, container) =>
            {
                var value = mapper.ConvertToTrackableList(doc, item.Property.Name);
                item.Property.SetValue(container, value);
            };
            item.SaveChanges = (update, tracker, keyValues) =>
            {
                var valueTracker = (TrackableListTracker<TValue>)item.TrackerProperty.GetValue(tracker);
                if (valueTracker.HasChange)
                {
                    var listUpdates = mapper.BuildUpdatesForSave(
                        valueTracker,
                        keyValues.Concat(new object[] { item.Property.Name }).ToArray()).ToList();
                    if (listUpdates.Count > 1)
                        throw new InvalidOperationException("Container cannot save multiple changes from list.");
                    update = update == null
                                 ? listUpdates[0]
                                 : Builders<BsonDocument>.Update.Combine(update, listUpdates[0]);
                }
                return update;
            };
        }

        #endregion

        #region Helpers

        public async Task CreateAsync(IMongoCollection<BsonDocument> collection, T value, params object[] keyValues)
        {
            if (keyValues.Length == 0)
                throw new ArgumentException("At least 1 keyValue required.");

            var bson = new BsonDocument();
            foreach (var pi in PropertyItems)
            {
                pi.ExportToBson(value, bson);
            }
            if (keyValues.Length == 1)
            {
                bson.InsertAt(0, new BsonElement("_id", BsonValue.Create(keyValues[0])));
                await collection.InsertOneAsync(bson);
            }
            else
            {
                var setPath = DocumentHelper.ToDotPath(keyValues.Skip(1));
                await collection.UpdateOneAsync(
                    Builders<BsonDocument>.Filter.Eq("_id", keyValues[0]),
                    Builders<BsonDocument>.Update.Set(setPath, bson));
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

            var container = (T)Activator.CreateInstance(_trackableType);
            foreach (var pi in PropertyItems)
            {
                pi.ImportFromBson(doc, container);
            }
            return container;
        }

        public Task<UpdateResult> SaveAsync(IMongoCollection<BsonDocument> collection,
                                            ITracker tracker,
                                            params object[] keyValues)
        {
            return SaveAsync(collection, (IContainerTracker<T>)tracker, keyValues);
        }

        public Task<UpdateResult> SaveAsync(IMongoCollection<BsonDocument> collection,
                                            IContainerTracker<T> tracker,
                                            params object[] keyValues)
        {
            if (keyValues.Length == 0)
                throw new ArgumentException("At least 1 keyValue required.");

            var partialKeys = keyValues.Skip(1).ToArray();
            UpdateDefinition<BsonDocument> update = null;
            foreach (var pi in PropertyItems)
            {
                update = pi.SaveChanges(update, tracker, partialKeys);
            }
            return collection.UpdateOneAsync(
                Builders<BsonDocument>.Filter.Eq("_id", keyValues[0]),
                update,
                new UpdateOptions { IsUpsert = true });
        }

        #endregion
    }
}
