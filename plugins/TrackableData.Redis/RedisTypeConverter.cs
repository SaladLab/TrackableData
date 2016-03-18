using StackExchange.Redis;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace TrackableData.Redis
{
    public class RedisTypeConverter
    {
        private static RedisTypeConverter _instance = new RedisTypeConverter();

        public static RedisTypeConverter Instance => _instance;

        internal class ConverterSet
        {
            public Delegate ToRedisValueFunc;
            public Delegate FromRedisValueFunc;
            public Func<object, RedisValue> ObjectToRedisValueFunc;
            public Func<RedisValue, object> ObjectFromRedisValueFunc;
        }

        private Dictionary<Type, ConverterSet> _converterMap;
        private JsonSerializerSettings _jsonSerializerSettings;

        public RedisTypeConverter(JsonSerializerSettings jsonSerializerSettings = null)
        {
            _converterMap = new Dictionary<Type, ConverterSet>(RedisTypeDefaultConverter.ConvertMap);
            _jsonSerializerSettings = jsonSerializerSettings ?? new JsonSerializerSettings();
        }

        public void Register<T>(Func<T, RedisValue> toRedisValueFunc,
                                Func<RedisValue, T> fromRedisValueFunc)
        {
            var cs = new ConverterSet
            {
                ToRedisValueFunc = toRedisValueFunc,
                FromRedisValueFunc = fromRedisValueFunc,
                ObjectToRedisValueFunc = RedisTypeConverterHelper.ConvertToObjectToFunc(toRedisValueFunc),
                ObjectFromRedisValueFunc = RedisTypeConverterHelper.ConvertToObjectFromFunc(fromRedisValueFunc),
            };

            lock (_converterMap)
            {
                _converterMap[typeof(T)] = cs;
            }
        }

        public void Register(Type type,
                             Func<object, RedisValue> toRedisValueFunc,
                             Func<RedisValue, object> fromRedisValueFunc)
        {
            var cs = new ConverterSet
            {
                ToRedisValueFunc = RedisTypeConverterHelper.ConvertToToFunc(type, toRedisValueFunc),
                FromRedisValueFunc = RedisTypeConverterHelper.ConvertToFromFunc(type, fromRedisValueFunc),
                ObjectToRedisValueFunc = toRedisValueFunc,
                ObjectFromRedisValueFunc = fromRedisValueFunc,
            };

            lock (_converterMap)
            {
                _converterMap[type] = cs;
            }
        }

        public void RegisterWithJsonSerialization(Type type)
        {
            Func<object, RedisValue> toFunc = v => JsonConvert.SerializeObject(v, _jsonSerializerSettings);
            Func<RedisValue, object> fromFunc = o => JsonConvert.DeserializeObject(o, type, _jsonSerializerSettings);

            var cs = new ConverterSet
            {
                ToRedisValueFunc = RedisTypeConverterHelper.ConvertToToFunc(type, toFunc),
                FromRedisValueFunc = RedisTypeConverterHelper.ConvertToFromFunc(type, fromFunc),
                ObjectToRedisValueFunc = toFunc,
                ObjectFromRedisValueFunc = fromFunc,
            };

            lock (_converterMap)
            {
                _converterMap[type] = cs;
            }
        }

        public Func<T, RedisValue> GetToRedisValueFunc<T>()
        {
            return (Func<T, RedisValue>)(GetConverterSet(typeof(T)).ToRedisValueFunc);
        }

        public Func<RedisValue, T> GetFromRedisValueFunc<T>()
        {
            return (Func<RedisValue, T>)(GetConverterSet(typeof(T)).FromRedisValueFunc);
        }

        public Func<object, RedisValue> GetToRedisValueFunc(Type type)
        {
            return GetConverterSet(type).ObjectToRedisValueFunc;
        }

        public Func<RedisValue, object> GetFromRedisValueFunc(Type type)
        {
            return GetConverterSet(type).ObjectFromRedisValueFunc;
        }

        private ConverterSet GetConverterSet(Type type)
        {
            lock (_converterMap)
            {
                ConverterSet cs;
                if (_converterMap.TryGetValue(type, out cs))
                    return cs;

                RegisterWithJsonSerialization(type);
                return _converterMap[type];
            }
        }
    }
}
