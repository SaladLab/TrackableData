using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace TrackableData.Redis
{
    public class TrackablePocoRedisMapper<T>
        where T : ITrackablePoco<T>
    {
        private readonly Type _trackableType;

        public class FieldProperty
        {
            public string Name;
            public RedisValue FieldName;
            public PropertyInfo PropertyInfo;
            public Func<object, RedisValue> ConvertToRedisValue;
            public Func<RedisValue, object> ConvertFromRedisValue;
        }

        private readonly FieldProperty[] _fields;
        private readonly Dictionary<RedisValue, FieldProperty> _nameFieldMap;
        private readonly Dictionary<PropertyInfo, FieldProperty> _valueFieldMap;

        public TrackablePocoRedisMapper(RedisTypeConverter typeConverter = null)
        {
            if (typeConverter == null)
                typeConverter = RedisTypeConverter.Instance;

            _trackableType = TrackableResolver.GetPocoTrackerType(typeof(T));
            if (_trackableType == null)
                throw new ArgumentException($"Cannot find type '{typeof(T).Name}'");

            var fields = new List<FieldProperty>();
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

                var field = new FieldProperty
                {
                    Name = property.Name,
                    FieldName = fieldName,
                    PropertyInfo = property,
                    ConvertToRedisValue = typeConverter.GetToRedisValueFunc(property.PropertyType),
                    ConvertFromRedisValue = typeConverter.GetFromRedisValueFunc(property.PropertyType),
                };

                if (field.ConvertToRedisValue == null || field.ConvertFromRedisValue == null)
                    throw new ArgumentException("Cannot find type converter. Property=" + property.Name);

                fields.Add(field);
            }

            _fields = fields.ToArray();
            _nameFieldMap = _fields.ToDictionary(x => x.FieldName, y => y);
            _valueFieldMap = _fields.ToDictionary(x => x.PropertyInfo, y => y);
        }

        public async Task CreateAsync(IDatabase db, T value, RedisKey key)
        {
            await db.KeyDeleteAsync(key);

            var entries = _fields.Select(f => new HashEntry(f.FieldName,
                                                            f.ConvertToRedisValue(f.PropertyInfo.GetValue(value))))
                                 .Where(e => e.Value.IsNull == false).ToArray();
            await db.HashSetAsync(key, entries);
        }

        public async Task<int> DeleteAsync(IDatabase db, RedisKey key)
        {
            return await db.KeyDeleteAsync(key).ConfigureAwait(false) ? 1 : 0;
        }

        public async Task<T> LoadAsync(IDatabase db, RedisKey key)
        {
            var value = (T)Activator.CreateInstance(_trackableType);
            return await LoadAsync(db, value, key).ConfigureAwait(false) ? value : default(T);
        }

        public async Task<bool> LoadAsync(IDatabase db, T value, RedisKey key)
        {
            var entries = await db.HashGetAllAsync(key);
            if (entries.Length == 0)
            {
                // Redis doesn't distinguish empty hashes with non-existent hashes
                return false;
            }

            foreach (var entry in entries)
            {
                FieldProperty field;
                if (_nameFieldMap.TryGetValue(entry.Name, out field) == false)
                    continue;

                var fieldValue = field.ConvertFromRedisValue(entry.Value);
                field.PropertyInfo.SetValue(value, fieldValue);
            }
            return true;
        }

        public Task SaveAsync(IDatabase db, IPocoTracker<T> tracker, RedisKey key)
        {
            return SaveAsync(db, (TrackablePocoTracker<T>)tracker, key);
        }

        public async Task SaveAsync(IDatabase db, TrackablePocoTracker<T> tracker, RedisKey key)
        {
            if (tracker.HasChange == false)
                return;

            var updates = new List<HashEntry>();
            var removes = new List<RedisValue>();
            foreach (var change in tracker.ChangeMap)
            {
                FieldProperty field;
                if (_valueFieldMap.TryGetValue(change.Key, out field) == false)
                    continue;

                var redisValue = field.ConvertToRedisValue(change.Value.NewValue);
                if (redisValue.IsNull)
                    removes.Add(field.FieldName);
                else
                    updates.Add(new HashEntry(field.FieldName, redisValue));
            }

            if (updates.Any())
                await db.HashSetAsync(key, updates.ToArray());
            if (removes.Any())
                await db.HashDeleteAsync(key, removes.ToArray());
        }
    }
}
