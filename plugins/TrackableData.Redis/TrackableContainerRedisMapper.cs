using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.Linq;
using StackExchange.Redis;

namespace TrackableData.Redis
{
    public class TrackableContainerRedisMapper<T>
        where T : ITrackableContainer<T>
    {
        private readonly Type _trackableType;

        private class PropertyItem
        {
            public string Name;
            public RedisKey KeySuffix;
            public PropertyInfo PropertyInfo;
            public PropertyInfo TrackerPropertyInfo;
            public object Mapper;
            public Func<IDatabase, T, RedisKey, Task> CreateAsync;
            public Func<IDatabase, RedisKey, Task<int>> DeleteAsync;
            public Func<IDatabase, T, RedisKey, Task<bool>> LoadAsync;
            public Func<IDatabase, IContainerTracker<T>, RedisKey, Task> SaveAsync;
        }

        private readonly PropertyItem[] _items;

        public TrackableContainerRedisMapper(RedisTypeConverter typeConverter = null)
        {
            if (typeConverter == null)
                typeConverter = RedisTypeConverter.Instance;

            _trackableType = TrackableResolver.GetContainerTrackerType(typeof(T));
            if (_trackableType == null)
                throw new ArgumentException($"Cannot find tracker type of '{nameof(T)}'");

            _items = ConstructPropertyItems(typeConverter);
        }

        private static PropertyItem[] ConstructPropertyItems(RedisTypeConverter typeConverter)
        {
            var trackerType = TrackerResolver.GetDefaultTracker(typeof(T));

            var items = new List<PropertyItem>();
            foreach (var property in typeof(T).GetProperties())
            {
                var keySuffix = "." + property.Name;

                var attr = property.GetCustomAttribute<TrackablePropertyAttribute>();
                if (attr != null)
                {
                    if (attr["redis.ignore"] != null)
                        continue;
                    keySuffix = attr["redis.keysuffix:"] ?? keySuffix;
                }

                var item = new PropertyItem
                {
                    Name = property.Name,
                    KeySuffix = keySuffix,
                    PropertyInfo = property,
                    TrackerPropertyInfo = trackerType.GetProperty(property.Name + "Tracker")
                };

                if (item.TrackerPropertyInfo == null)
                    throw new ArgumentException($"Cannot find tracker type of '{property.Name}'");

                if (TrackableResolver.IsTrackablePoco(property.PropertyType))
                {
                    typeof(TrackableContainerRedisMapper<T>)
                        .GetMethod("BuildTrackablePocoProperty", BindingFlags.Static | BindingFlags.NonPublic)
                        .MakeGenericMethod(TrackableResolver.GetPocoType(property.PropertyType))
                        .Invoke(null, new object[] { item, typeConverter });
                }
                else if (TrackableResolver.IsTrackableDictionary(property.PropertyType))
                {
                    typeof(TrackableContainerRedisMapper<T>)
                        .GetMethod("BuildTrackableDictionaryProperty", BindingFlags.Static | BindingFlags.NonPublic)
                        .MakeGenericMethod(property.PropertyType.GetGenericArguments())
                        .Invoke(null, new object[] { item, typeConverter });
                }
                else if (TrackableResolver.IsTrackableSet(property.PropertyType))
                {
                    typeof(TrackableContainerRedisMapper<T>)
                        .GetMethod("BuildTrackableSetProperty", BindingFlags.Static | BindingFlags.NonPublic)
                        .MakeGenericMethod(property.PropertyType.GetGenericArguments())
                        .Invoke(null, new object[] { item, typeConverter });
                }
                else if (TrackableResolver.IsTrackableList(property.PropertyType))
                {
                    typeof(TrackableContainerRedisMapper<T>)
                        .GetMethod("BuildTrackableListProperty", BindingFlags.Static | BindingFlags.NonPublic)
                        .MakeGenericMethod(property.PropertyType.GetGenericArguments())
                        .Invoke(null, new object[] { item, typeConverter });
                }
                else
                {
                    throw new InvalidOperationException("Cannot resolve property: " + property.Name);
                }

                items.Add(item);
            }
            return items.ToArray();
        }

        private static void BuildTrackablePocoProperty<TPoco>(PropertyItem item,
                                                              RedisTypeConverter typeConverter)
            where TPoco : ITrackablePoco<TPoco>
        {
            var mapper = new TrackablePocoRedisMapper<TPoco>(typeConverter);
            item.Mapper = mapper;

            item.CreateAsync = (db, container, key) =>
            {
                var value = (TPoco)item.PropertyInfo.GetValue(container);
                return mapper.CreateAsync(db, value, key.Prepend(item.KeySuffix));
            };
            item.DeleteAsync = (db, key) =>
            {
                return mapper.DeleteAsync(db, key.Prepend(item.KeySuffix));
            };
            item.LoadAsync = (db, container, key) =>
            {
                var value = (TPoco)item.PropertyInfo.GetValue(container);
                // when there is no entry for poco, it is regarded as non-existent container.
                return mapper.LoadAsync(db, value, key.Prepend(item.KeySuffix));
            };
            item.SaveAsync = async (db, tracker, key) =>
            {
                var valueTracker = (TrackablePocoTracker<TPoco>)item.TrackerPropertyInfo.GetValue(tracker);
                if (valueTracker.HasChange)
                {
                    await mapper.SaveAsync(db, valueTracker, key.Prepend(item.KeySuffix));
                }
            };
        }

        private static void BuildTrackableDictionaryProperty<TKey, TValue>(PropertyItem item,
                                                                           RedisTypeConverter typeConverter)
        {
            var mapper = new TrackableDictionaryRedisMapper<TKey, TValue>(typeConverter);
            item.Mapper = mapper;

            item.CreateAsync = (db, container, key) =>
            {
                var dictionary = (IDictionary<TKey, TValue>)item.PropertyInfo.GetValue(container);
                return mapper.CreateAsync(db, dictionary, key.Prepend(item.KeySuffix));
            };
            item.DeleteAsync = (db, key) =>
            {
                return mapper.DeleteAsync(db, key.Prepend(item.KeySuffix));
            };
            item.LoadAsync = async (db, container, key) =>
            {
                var dictionary = (IDictionary<TKey, TValue>)item.PropertyInfo.GetValue(container);
                // when there is no entry for dictionary, it is regarded as an empty dictionary.
                await mapper.LoadAsync(db, dictionary, key.Prepend(item.KeySuffix));
                return true;
            };
            item.SaveAsync = async (db, tracker, key) =>
            {
                var valueTracker = (TrackableDictionaryTracker<TKey, TValue>)item.TrackerPropertyInfo.GetValue(tracker);
                if (valueTracker.HasChange)
                {
                    await mapper.SaveAsync(db, valueTracker, key.Prepend(item.KeySuffix));
                }
            };
        }

        private static void BuildTrackableSetProperty<TValue>(PropertyItem item,
                                                              RedisTypeConverter typeConverter)
        {
            var mapper = new TrackableSetRedisMapper<TValue>(typeConverter);
            item.Mapper = mapper;

            item.CreateAsync = (db, container, key) =>
            {
                var set = (ICollection<TValue>)item.PropertyInfo.GetValue(container);
                return mapper.CreateAsync(db, set, key.Prepend(item.KeySuffix));
            };
            item.DeleteAsync = (db, key) =>
            {
                return mapper.DeleteAsync(db, key.Prepend(item.KeySuffix));
            };
            item.LoadAsync = async (db, container, key) =>
            {
                var set = (ICollection<TValue>)item.PropertyInfo.GetValue(container);
                // when there is no entry for set, it is regarded as an empty set.
                await mapper.LoadAsync(db, set, key.Prepend(item.KeySuffix));
                return true;
            };
            item.SaveAsync = async (db, tracker, key) =>
            {
                var valueTracker = (TrackableSetTracker<TValue>)item.TrackerPropertyInfo.GetValue(tracker);
                if (valueTracker.HasChange)
                {
                    await mapper.SaveAsync(db, valueTracker, key.Prepend(item.KeySuffix));
                }
            };
        }

        private static void BuildTrackableListProperty<TValue>(PropertyItem item,
                                                               RedisTypeConverter typeConverter)
        {
            var mapper = new TrackableListRedisMapper<TValue>(typeConverter);
            item.Mapper = mapper;

            item.CreateAsync = (db, container, key) =>
            {
                var list = (IList<TValue>)item.PropertyInfo.GetValue(container);
                return mapper.CreateAsync(db, list, key.Prepend(item.KeySuffix));
            };
            item.DeleteAsync = (db, key) =>
            {
                return mapper.DeleteAsync(db, key.Prepend(item.KeySuffix));
            };
            item.LoadAsync = async (db, container, key) =>
            {
                var list = (IList<TValue>)item.PropertyInfo.GetValue(container);
                // when there is no entry for list, it is regarded as an empty list.
                await mapper.LoadAsync(db, list, key.Prepend(item.KeySuffix));
                return true;
            };
            item.SaveAsync = async (db, tracker, key) =>
            {
                var valueTracker = (TrackableListTracker<TValue>)item.TrackerPropertyInfo.GetValue(tracker);
                if (valueTracker.HasChange)
                {
                    await mapper.SaveAsync(db, valueTracker, key.Prepend(item.KeySuffix));
                }
            };
        }

        public IEnumerable<ITrackable> GetTrackables(T container)
        {
            return _items.Select(item => (ITrackable)item.PropertyInfo.GetValue(container));
        }

        public IEnumerable<ITracker> GetTrackers(T container)
        {
            return GetTrackers(container.Tracker);
        }

        public IEnumerable<ITracker> GetTrackers(IContainerTracker<T> tracker)
        {
            return _items.Select(item => (ITracker)item.TrackerPropertyInfo.GetValue(tracker));
        }

        public async Task CreateAsync(IDatabase db, T container, RedisKey key)
        {
            foreach (var pi in _items)
            {
                await pi.CreateAsync(db, container, key);
            }
        }

        public async Task<int> DeleteAsync(IDatabase db, RedisKey key)
        {
            var count = 0;
            foreach (var pi in _items)
            {
                count += await pi.DeleteAsync(db, key);
            }
            return count;
        }

        public async Task<T> LoadAsync(IDatabase db, RedisKey key)
        {
            var container = (T)Activator.CreateInstance(_trackableType);
            foreach (var pi in _items)
            {
                var ok = await pi.LoadAsync(db, container, key);
                if (ok == false)
                {
                    // empty poco can do it
                    return default(T);
                }
            }
            return container;
        }

        public Task SaveAsync(IDatabase db, ITracker tracker, RedisKey key)
        {
            return SaveAsync(db, (IContainerTracker<T>)tracker, key);
        }

        public async Task SaveAsync(IDatabase db, IContainerTracker<T> tracker, RedisKey key)
        {
            foreach (var pi in _items)
            {
                await pi.SaveAsync(db, tracker, key);
            }
        }
    }
}
