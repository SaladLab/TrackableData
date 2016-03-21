using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace TrackableData.Redis
{
    public class TrackableDictionaryRedisMapper<TKey, TValue>
    {
        private Func<TKey, RedisValue> _keyToRedisValue;
        private Func<RedisValue, TKey> _redisValueToKey;
        private Func<TValue, RedisValue> _valueToRedisValue;
        private Func<RedisValue, TValue> _redisValueToValue;

        public TrackableDictionaryRedisMapper(RedisTypeConverter typeConverter = null)
        {
            if (typeConverter == null)
                typeConverter = RedisTypeConverter.Instance;

            _keyToRedisValue = typeConverter.GetToRedisValueFunc<TKey>();
            _redisValueToKey = typeConverter.GetFromRedisValueFunc<TKey>();

            if (_keyToRedisValue == null || _redisValueToKey == null)
                throw new ArgumentException("Cannot find type converter. Type=" + typeof(TKey).Name);

            _valueToRedisValue = typeConverter.GetToRedisValueFunc<TValue>();
            _redisValueToValue = typeConverter.GetFromRedisValueFunc<TValue>();

            if (_valueToRedisValue == null || _redisValueToValue == null)
                throw new ArgumentException("Cannot find type converter. Type=" + typeof(TValue).Name);
        }

        public async Task CreateAsync(IDatabase db, IDictionary<TKey, TValue> dictionary, RedisKey key)
        {
            await db.KeyDeleteAsync(key);

            var entries = dictionary.Select(i => new HashEntry(_keyToRedisValue(i.Key),
                                                               _valueToRedisValue(i.Value))).ToArray();
            await db.HashSetAsync(key, entries);
        }

        public async Task<int> DeleteAsync(IDatabase db, RedisKey key)
        {
            return await db.KeyDeleteAsync(key).ConfigureAwait(false) ? 1 : 0;
        }

        public async Task<TrackableDictionary<TKey, TValue>> LoadAsync(IDatabase db, RedisKey key)
        {
            var dictionary = new TrackableDictionary<TKey, TValue>();
            return await LoadAsync(db, dictionary, key).ConfigureAwait(false) ? dictionary : null;
        }

        public async Task<bool> LoadAsync(IDatabase db, IDictionary<TKey, TValue> dictionary, RedisKey key)
        {
            var entries = await db.HashGetAllAsync(key);
            if (entries.Length == 0)
            {
                // Redis doesn't distinguish empty hashes with non-existent hashes
                return false;
            }

            foreach (var entry in entries)
            {
                dictionary.Add(_redisValueToKey(entry.Name), _redisValueToValue(entry.Value));
            }
            return true;
        }

        public Task SaveAsync(IDatabase db, IDictionaryTracker<TKey, TValue> tracker, RedisKey key)
        {
            return SaveAsync(db, (TrackableDictionaryTracker<TKey, TValue>)tracker, key);
        }

        public async Task SaveAsync(IDatabase db, TrackableDictionaryTracker<TKey, TValue> tracker, RedisKey key)
        {
            var updates = tracker.ChangeMap.Where(i => i.Value.Operation == TrackableDictionaryOperation.Add ||
                                                       i.Value.Operation == TrackableDictionaryOperation.Modify)
                                 .Select(i => new HashEntry(_keyToRedisValue(i.Key),
                                                            _valueToRedisValue(i.Value.NewValue))).ToArray();

            var removeKeys = tracker.RemoveKeys.Select(_keyToRedisValue).ToArray();

            if (updates.Length > 0)
                await db.HashSetAsync(key, updates);

            if (removeKeys.Length > 0)
                await db.HashDeleteAsync(key, removeKeys);
        }
    }
}
