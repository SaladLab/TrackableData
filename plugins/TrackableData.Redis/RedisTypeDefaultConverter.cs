using StackExchange.Redis;
using System;
using System.Collections.Generic;
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

                return toMethods.ToDictionary(
                    i => i.Key,
                    i =>
                    {
                        var toMethod = i.Value;
                        var fromMethod = fromMethods[i.Key];

                        var toFunc = toMethod.CreateDelegate(
                            typeof(Func<,>).MakeGenericType(i.Key, typeof(RedisValue)));
                        var fromFunc = toMethod.CreateDelegate(
                            typeof(Func<,>).MakeGenericType(typeof(RedisValue), i.Key));

                        return new RedisTypeConverter.ConverterSet
                        {
                            ToRedisValueFunc = toFunc,
                            FromRedisValueFunc = fromFunc,
                            ObjectToRedisValueFunc = null,
                            ObjectFromRedisValueFunc = null
                        };
                    });
            });
    
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
