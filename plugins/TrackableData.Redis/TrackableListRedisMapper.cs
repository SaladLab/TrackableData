using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace TrackableData.Redis
{
    public class TrackableListRedisMapper<T>
    {
        private Func<T, RedisValue> _valueToRedisValue;
        private Func<RedisValue, T> _redisValueToValue;

        public TrackableListRedisMapper(RedisTypeConverter typeConverter = null)
        {
            if (typeConverter == null)
                typeConverter = RedisTypeConverter.Instance;

            _valueToRedisValue = typeConverter.GetToRedisValueFunc<T>();
            _redisValueToValue = typeConverter.GetFromRedisValueFunc<T>();

            if (_valueToRedisValue == null || _redisValueToValue == null)
                throw new ArgumentException("Cannot find type converter. Type=" + typeof(T).Name);
        }

        public async Task CreateAsync(IDatabase db, IList<T> list, RedisKey key)
        {
            await db.KeyDeleteAsync(key);
            await db.ListRightPushAsync(key, list.Select(_valueToRedisValue).ToArray());
        }

        public async Task<int> DeleteAsync(IDatabase db, RedisKey key)
        {
            return await db.KeyDeleteAsync(key).ConfigureAwait(false) ? 1 : 0;
        }

        public async Task<TrackableList<T>> LoadAsync(IDatabase db, RedisKey key)
        {
            var list = new TrackableList<T>();
            return await LoadAsync(db, list, key).ConfigureAwait(false) ? list : null;
        }

        public async Task<bool> LoadAsync(IDatabase db, IList<T> list, RedisKey key)
        {
            var values = await db.ListRangeAsync(key);
            if (values.Length == 0)
            {
                // Redis doesn't distinguish empty list with non-existent list
                return false;
            }

            foreach (var value in values)
                list.Add(_redisValueToValue(value));
            return true;
        }

        public Task SaveAsync(IDatabase db, IListTracker<T> tracker, RedisKey key)
        {
            return SaveAsync(db, (TrackableListTracker<T>)tracker, key);
        }

        public async Task SaveAsync(IDatabase db, TrackableListTracker<T> tracker, RedisKey key)
        {
            foreach (var change in tracker.ChangeList)
            {
                switch (change.Operation)
                {
                    case TrackableListOperation.Insert:
                        throw new InvalidOperationException("Redis doesn't support insert value by index.");

                    case TrackableListOperation.Remove:
                        throw new InvalidOperationException("Redis doesn't support remove value by index.");

                    case TrackableListOperation.Modify:
                        await db.ListSetByIndexAsync(key, change.Index, _valueToRedisValue(change.NewValue));
                        break;

                    case TrackableListOperation.PushFront:
                        await db.ListLeftPushAsync(key, _valueToRedisValue(change.NewValue));
                        break;

                    case TrackableListOperation.PushBack:
                        await db.ListRightPushAsync(key, _valueToRedisValue(change.NewValue));
                        break;

                    case TrackableListOperation.PopFront:
                        await db.ListLeftPopAsync(key);
                        break;

                    case TrackableListOperation.PopBack:
                        await db.ListRightPopAsync(key);
                        break;
                }
            }
        }
    }
}
