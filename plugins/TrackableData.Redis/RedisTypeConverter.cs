using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace TrackableData.Redis
{
    public class RedisTypeConverter
    {
        private static RedisTypeConverter _instance = new RedisTypeConverter();

        public static RedisTypeConverter Instance => _instance;

        private static MethodInfo _methodInfoForRegisterNullableType;
        private static MethodInfo _methodInfoForRegisterWithJsonSerialization;

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

                var nullableUnderlyingType = Nullable.GetUnderlyingType(type);
                if (nullableUnderlyingType != null)
                    return RegisterNullableType(nullableUnderlyingType);

                return RegisterWithJsonSerialization(type);
            }
        }

        private ConverterSet RegisterNullableType<T>()
            where T : struct
        {
            var underlyingTypeConverter = GetConverterSet(typeof(T));
            if (underlyingTypeConverter == null)
                throw new ArgumentException("Cannot find underlying type converter. Type=" + typeof(T).Name);

            Func<T?, RedisValue> toFunc =
                o => o == null
                         ? RedisValue.Null
                         : ((Func<T, RedisValue>)underlyingTypeConverter.ToRedisValueFunc)(o.Value);

            Func<RedisValue, T?> fromFunc =
                o => o.IsNull
                         ? null
                         : (T?)((Func<RedisValue, T>)underlyingTypeConverter.FromRedisValueFunc)(o);

            var cs = new ConverterSet
            {
                ToRedisValueFunc = toFunc,
                FromRedisValueFunc = fromFunc,
                ObjectToRedisValueFunc = RedisTypeConverterHelper.ConvertToObjectToFunc(toFunc),
                ObjectFromRedisValueFunc = RedisTypeConverterHelper.ConvertToObjectFromFunc(fromFunc),
            };

            lock (_converterMap)
            {
                _converterMap[typeof(T?)] = cs;
                return cs;
            }
        }

        private ConverterSet RegisterNullableType(Type underlyingType)
        {
            if (_methodInfoForRegisterNullableType == null)
            {
                _methodInfoForRegisterNullableType =
                    typeof(RedisTypeConverter).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                                              .First(m => m.Name == "RegisterNullableType" &&
                                                          m.GetParameters().Length == 0);
            }

            return (ConverterSet)_methodInfoForRegisterNullableType.MakeGenericMethod(underlyingType)
                                                                   .Invoke(this, new object[0]);
        }

        private ConverterSet RegisterWithJsonSerialization<T>()
        {
            Func<T, RedisValue> toFunc = v => JsonConvert.SerializeObject(v, _jsonSerializerSettings);
            Func<RedisValue, T> fromFunc = o => JsonConvert.DeserializeObject<T>(o, _jsonSerializerSettings);

            var cs = new ConverterSet
            {
                ToRedisValueFunc = toFunc,
                FromRedisValueFunc = fromFunc,
                ObjectToRedisValueFunc = RedisTypeConverterHelper.ConvertToObjectToFunc(toFunc),
                ObjectFromRedisValueFunc = RedisTypeConverterHelper.ConvertToObjectFromFunc(fromFunc),
            };

            lock (_converterMap)
            {
                _converterMap[typeof(T)] = cs;
                return cs;
            }
        }

        private ConverterSet RegisterWithJsonSerialization(Type type)
        {
            if (_methodInfoForRegisterWithJsonSerialization == null)
            {
                _methodInfoForRegisterWithJsonSerialization =
                    typeof(RedisTypeConverter).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                                              .First(m => m.Name == "RegisterWithJsonSerialization" &&
                                                          m.GetParameters().Length == 0);
            }

            return (ConverterSet)_methodInfoForRegisterWithJsonSerialization.MakeGenericMethod(type)
                                                                            .Invoke(this, new object[0]);
        }
    }
}
