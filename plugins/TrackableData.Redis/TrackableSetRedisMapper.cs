using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace TrackableData.Redis
{
    public class TrackableSetRedisMapper<T>
    {
        private Func<T, RedisValue> _valueToRedisValue;
        private Func<RedisValue, T> _redisValueToValue;

        public TrackableSetRedisMapper(RedisTypeConverter typeConverter = null)
        {
            if (typeConverter == null)
                typeConverter = RedisTypeConverter.Instance;

            _valueToRedisValue = typeConverter.GetToRedisValueFunc<T>();
            _redisValueToValue = typeConverter.GetFromRedisValueFunc<T>();

            if (_valueToRedisValue == null || _redisValueToValue == null)
                throw new ArgumentException("Cannot find type converter. Type=" + typeof(T).Name);
        }

        public async Task CreateAsync(IDatabase db, ICollection<T> set, RedisKey key)
        {
            await db.KeyDeleteAsync(key);
            await db.SetAddAsync(key, set.Select(_valueToRedisValue).ToArray());
        }

        public async Task<int> DeleteAsync(IDatabase db, RedisKey key)
        {
            return await db.KeyDeleteAsync(key).ConfigureAwait(false) ? 1 : 0;
        }

        public async Task<TrackableSet<T>> LoadAsync(IDatabase db, RedisKey key)
        {
            var set = new TrackableSet<T>();
            return await LoadAsync(db, set, key).ConfigureAwait(false) ? set : null;
        }

        public async Task<bool> LoadAsync(IDatabase db, ICollection<T> set, RedisKey key)
        {
            var values = await db.SetMembersAsync(key);
            if (values.Length == 0)
            {
                // Redis doesn't distinguish empty set with non-existent set
                return false;
            }

            foreach (var value in values)
                set.Add(_redisValueToValue(value));
            return true;
        }

        public Task SaveAsync(IDatabase db, ISetTracker<T> tracker, RedisKey key)
        {
            return SaveAsync(db, (TrackableSetTracker<T>)tracker, key);
        }

        public async Task SaveAsync(IDatabase db, TrackableSetTracker<T> tracker, RedisKey key)
        {
            var addValues = tracker.AddValues.Select(_valueToRedisValue).ToArray();
            var removeValues = tracker.RemoveValues.Select(_valueToRedisValue).ToArray();

            if (addValues.Length > 0)
                await db.SetAddAsync(key, addValues);

            if (removeValues.Length > 0)
                await db.SetRemoveAsync(key, removeValues);
        }
    }
}
