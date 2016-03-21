using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace TrackableData.Redis
{
    public class TrackableContainerHashesRedisMapper<T>
        where T : ITrackableContainer<T>
    {
        private readonly Type _trackableType;

        private class PropertyItem
        {
            public string Name;
            public RedisValue FieldName;
            public PropertyInfo PropertyInfo;
            public PropertyInfo TrackerPropertyInfo;
            public Func<object, RedisValue> ConvertToRedisValue;
            public Func<RedisValue, object> ConvertFromRedisValue;
        }

        private readonly PropertyItem[] _items;
        private readonly Dictionary<RedisValue, PropertyItem> _fieldNameToItemMap;

        public TrackableContainerHashesRedisMapper(RedisTypeConverter typeConverter = null)
        {
            if (typeConverter == null)
                typeConverter = RedisTypeConverter.Instance;

            _trackableType = TrackableResolver.GetContainerTrackerType(typeof(T));
            if (_trackableType == null)
                throw new ArgumentException($"Cannot find tracker type of '{nameof(T)}'");

            _items = ConstructPropertyItems(typeConverter);
            _fieldNameToItemMap = _items.ToDictionary(x => x.FieldName, y => y);
        }

        private static PropertyItem[] ConstructPropertyItems(RedisTypeConverter typeConverter)
        {
            var trackerType = TrackerResolver.GetDefaultTracker(typeof(T));

            var items = new List<PropertyItem>();
            foreach (var property in typeof(T).GetProperties())
            {
                var fieldName = property.Name;

                var attr = property.GetCustomAttribute<TrackablePropertyAttribute>();
                if (attr != null)
                {
                    if (attr["redis.ignore"] != null)
                        continue;
                    fieldName = attr["redis.field:"] ?? fieldName;
                }

                var item = new PropertyItem
                {
                    Name = property.Name,
                    FieldName = fieldName,
                    PropertyInfo = property,
                    TrackerPropertyInfo = trackerType.GetProperty(property.Name + "Tracker"),
                    ConvertToRedisValue = typeConverter.GetToRedisValueFunc(property.PropertyType),
                    ConvertFromRedisValue = typeConverter.GetFromRedisValueFunc(property.PropertyType),
                };

                if (item.TrackerPropertyInfo == null)
                    throw new ArgumentException($"Cannot find tracker type of '{property.Name}'");

                if (item.ConvertToRedisValue == null || item.ConvertFromRedisValue == null)
                    throw new ArgumentException("Cannot find type converter. Property=" + property.Name);

                items.Add(item);
            }
            return items.ToArray();
        }

        public IEnumerable<ITrackable> GetTrackables(T container)
        {
            return _items.Select(item => (ITrackable)item.PropertyInfo.GetValue(container));
        }

        public IEnumerable<ITrackable> GetTrackers(T container)
        {
            var tracker = container.Tracker;
            return _items.Select(item => (ITrackable)item.TrackerPropertyInfo.GetValue(tracker));
        }

        public async Task CreateAsync(IDatabase db, T container, RedisKey key)
        {
            await db.KeyDeleteAsync(key);

            var entries = _items.Select(f => new HashEntry(f.FieldName,
                                                           f.ConvertToRedisValue(f.PropertyInfo.GetValue(container))))
                                .Where(e => e.Value.IsNull == false).ToArray();
            await db.HashSetAsync(key, entries);
        }

        public async Task<int> DeleteAsync(IDatabase db, RedisKey key)
        {
            return await db.KeyDeleteAsync(key).ConfigureAwait(false) ? 1 : 0;
        }

        public async Task<T> LoadAsync(IDatabase db, RedisKey key)
        {
            var container = (T)Activator.CreateInstance(_trackableType);
            return await LoadAsync(db, container, key).ConfigureAwait(false) ? container : default(T);
        }

        public async Task<bool> LoadAsync(IDatabase db, T container, RedisKey key)
        {
            var entries = await db.HashGetAllAsync(key);
            if (entries.Length == 0)
            {
                // Redis doesn't distinguish empty hashes with non-existent hashes
                return false;
            }

            foreach (var entry in entries)
            {
                PropertyItem item;
                if (_fieldNameToItemMap.TryGetValue(entry.Name, out item) == false)
                    continue;

                var propertyValue = item.ConvertFromRedisValue(entry.Value);
                item.PropertyInfo.SetValue(container, propertyValue);
            }
            return true;
        }

        public Task SaveAsync(IDatabase db, T container, RedisKey key)
        {
            return SaveAsync(db, (ITrackableContainer<T>)container, key);
        }

        public async Task SaveAsync(IDatabase db, ITrackableContainer<T> container, RedisKey key)
        {
            var tracker = container.Tracker;
            if (tracker.HasChange == false)
                return;

            var updates = new List<HashEntry>();
            var removes = new List<RedisValue>();
            foreach (var item in _items)
            {
                var propertyTracker = (ITracker)item.TrackerPropertyInfo.GetValue(tracker);
                if (propertyTracker.HasChange)
                {
                    var propertyValue = item.ConvertToRedisValue(item.PropertyInfo.GetValue(container));
                    if (propertyValue.IsNull)
                        removes.Add(item.FieldName);
                    else
                        updates.Add(new HashEntry(item.FieldName, propertyValue));
                }
            }

            if (updates.Any())
                await db.HashSetAsync(key, updates.ToArray());
            if (removes.Any())
                await db.HashDeleteAsync(key, removes.ToArray());
        }
    }
}
