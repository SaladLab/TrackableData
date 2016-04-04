using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using StackExchange.Redis;

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
                            ObjectToRedisValueFunc = (Func<object, RedisValue>)convertToObjectToFunc.Invoke(null, new object[] { toFunc }),
                            ObjectFromRedisValueFunc = (Func<RedisValue, object>)convertToObjectFromFunc.Invoke(null, new object[] { fromFunc })
                        };
                    });
            });

        public static Dictionary<Type, RedisTypeConverter.ConverterSet> ConvertMap => _converterMap.Value;

        // default converters

        private static RedisValue ToRedisValue(bool v) => v;
        private static bool ToBool(RedisValue v) => (bool)v;

        private static RedisValue ToRedisValue(short v) => v;
        private static short ToShort(RedisValue v) => (short)v;

        private static RedisValue ToRedisValue(int v) => v;
        private static int ToInt(RedisValue v) => (int)v;

        private static RedisValue ToRedisValue(long v) => v;
        private static long ToLong(RedisValue v) => (long)v;

        private static RedisValue ToRedisValue(string v) => v;
        private static string ToString(RedisValue v) => v;

        private static RedisValue ToRedisValue(char v) => new string(v, 1);
        private static char ToChar(RedisValue v) => ((string)v)[0];

        private static RedisValue ToRedisValue(float v)
        {
            if (float.IsInfinity(v))
            {
                if (double.IsPositiveInfinity(v))
                    return "+inf";
                if (double.IsNegativeInfinity(v))
                    return "-inf";
            }
            return v.ToString("G", NumberFormatInfo.InvariantInfo);
        }

        private static float ToFloat(RedisValue v) => (float)(double)v;

        private static RedisValue ToRedisValue(double v) => v;
        private static double ToDouble(RedisValue v) => (double)v;

        private static RedisValue ToRedisValue(byte[] v) => v;
        private static byte[] ToBytes(RedisValue v) => (byte[])v;

        private static RedisValue ToRedisValue(DateTime v) => v.ToString("o");

        private static DateTime ToDateTime(RedisValue v) =>
            DateTime.ParseExact(v, "o", CultureInfo.InvariantCulture);

        private static RedisValue ToRedisValue(DateTimeOffset v) => v.ToString("o");

        private static DateTimeOffset ToDateTimeOffset(RedisValue v) =>
            DateTimeOffset.ParseExact(v, "o", CultureInfo.InvariantCulture);

        private static RedisValue ToRedisValue(TimeSpan v) => v.ToString("c");

        private static TimeSpan ToTimeSpan(RedisValue v) =>
            TimeSpan.ParseExact(v, "c", CultureInfo.InvariantCulture);

        private static RedisValue ToRedisValue(Guid v) => v.ToString("D");
        private static Guid ToGuid(RedisValue v) => Guid.ParseExact(v, "D");
    }
}
