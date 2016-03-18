using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace TrackableData.Redis
{
    internal static class RedisTypeDefaultConverter
    {
        private static Lazy<Dictionary<Type, RedisTypeConverter.ConverterSet>> _converterMap =
            new Lazy<Dictionary<Type, RedisTypeConverter.ConverterSet>>(() =>
            {
                var toMethods = typeof(RedisTypeDefaultConverter)
                    .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
                    .Where(m => m.Name == "ToRedisValue")
                    .ToDictionary(i => i.GetParameters()[0].ParameterType, i => i);

                var fromMethods = typeof(RedisTypeDefaultConverter)
                    .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
                    .Where(m => m.Name.StartsWith("To") &&
                                m.GetParameters().Length == 1 &&
                                m.GetParameters()[0].ParameterType == typeof(RedisValue))
                    .ToDictionary(i => i.ReturnParameter.ParameterType, i => i);

                var convertToObjectToFuncMethod = typeof(RedisTypeConverterHelper)
                    .GetMethods(BindingFlags.Static | BindingFlags.Public)
                    .FirstOrDefault(m => m.Name.StartsWith("ConvertToObjectToFunc"));

                var convertToObjectFromFuncMethod = typeof(RedisTypeConverterHelper)
                    .GetMethods(BindingFlags.Static | BindingFlags.Public)
                    .FirstOrDefault(m => m.Name.StartsWith("ConvertToObjectFromFunc"));

                return toMethods.ToDictionary(
                    i => i.Key,
                    i =>
                    {
                        var toMethod = i.Value;
                        var fromMethod = fromMethods[i.Key];

                        var toFunc = toMethod.CreateDelegate(
                            typeof(Func<,>).MakeGenericType(i.Key, typeof(RedisValue)));
                        var fromFunc = fromMethod.CreateDelegate(
                            typeof(Func<,>).MakeGenericType(typeof(RedisValue), i.Key));
                        var convertToObjectToFunc = convertToObjectToFuncMethod.MakeGenericMethod(i.Key);
                        var convertToObjectFromFunc = convertToObjectFromFuncMethod.MakeGenericMethod(i.Key);

                        return new RedisTypeConverter.ConverterSet
                        {
                            ToRedisValueFunc = toFunc,
                            FromRedisValueFunc = fromFunc,
                            ObjectToRedisValueFunc = (Func<object, RedisValue>)
                                                     convertToObjectToFunc.Invoke(null, new object[] { toFunc }),
                            ObjectFromRedisValueFunc = (Func<RedisValue, object>)
                                                       convertToObjectFromFunc.Invoke(null, new object[] { fromFunc })
                        };
                    });
            });

        public static Dictionary<Type, RedisTypeConverter.ConverterSet> ConvertMap => _converterMap.Value;

        private static RedisValue ToRedisValue(bool v) => v;
        private static RedisValue ToRedisValue(short v) => v;
        private static RedisValue ToRedisValue(int v) => v;
        private static RedisValue ToRedisValue(long v) => v;
        private static RedisValue ToRedisValue(string v) => v;
        private static RedisValue ToRedisValue(float v) => v;
        private static RedisValue ToRedisValue(double v) => v;
        private static RedisValue ToRedisValue(byte[] v) => v;

        private static bool ToBool(RedisValue v) => (bool)v;
        private static short ToShort(RedisValue v) => (short)v;
        private static int ToInt(RedisValue v) => (int)v;
        private static long ToLong(RedisValue v) => (long)v;
        private static string ToString(RedisValue v) => v;
        private static float ToFloat(RedisValue v) => (float)v;
        private static double ToDouble(RedisValue v) => (double)v;
        private static byte[] ToBytes(RedisValue v) => (byte[])v;
    }
}
