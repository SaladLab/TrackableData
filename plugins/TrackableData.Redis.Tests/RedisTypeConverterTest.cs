using System;
using StackExchange.Redis;
using Xunit;

namespace TrackableData.Redis
{
    public class RedisTypeConverterTest
    {
        [Fact]
        public void RegisterTypeConverterAsExplicitType_WorkWell()
        {
            var converter = new RedisTypeConverter();
            converter.Register(
                v => ('`' + v + '`'),
                v => ((string)v).Trim('`'));

            AssertConversionEqual(converter, "Value", "`Value`");
        }

        [Fact]
        public void RegisterTypeConverterAsObjectType_WorkWell()
        {
            var converter = new RedisTypeConverter();
            converter.Register(
                typeof(string),
                v => ('`' + (string)v + '`'),
                v => ((string)v).Trim('`'));

            AssertConversionEqual(converter, "Value", "`Value`");
        }

        [Fact]
        public void DefaultConverter_WorkWell()
        {
            var converter = new RedisTypeConverter();

            AssertConversionEqual(converter, true);
            AssertConversionEqual(converter, (short)1);
            AssertConversionEqual(converter, (int)1);
            AssertConversionEqual(converter, (long)1);
            AssertConversionEqual(converter, 'c');
            AssertConversionEqual(converter, "string:\xAC00");
            AssertConversionEqual(converter, 3.141592f);
            AssertConversionEqual(converter, 3.1415927410125732);
            AssertConversionEqual(converter, new byte[] { 0, 1, 2 });
            AssertConversionEqual(converter, new DateTime(2001, 1, 1, 1, 1, 1));
            AssertConversionEqual(converter, new DateTimeOffset(2001, 1, 1, 1, 1, 1, TimeSpan.FromHours(2)));
            AssertConversionEqual(converter, new TimeSpan(1, 2, 3));
            AssertConversionEqual(converter, Guid.NewGuid());
        }

        [Fact]
        public void Nullable_WorkWell()
        {
            var converter = new RedisTypeConverter();

            AssertConversionEqual(converter, (bool?)null, RedisValue.Null);
            AssertConversionEqual(converter, (bool?)true, true);
            AssertConversionEqual(converter, (int?)null, RedisValue.Null);
            AssertConversionEqual(converter, (int?)100, 100);
        }

        [Fact]
        public void JsonFallback_WorkWell()
        {
            var converter = new RedisTypeConverter();

            AssertConversionEqual(converter, new { Value = "123" }, "{\"Value\":\"123\"}");
        }

        private void AssertConversionEqual<T>(RedisTypeConverter converter, T value,
                                              RedisValue? expectedRedisValue = null)
        {
            // check from(to(V)) == V (explicit type version)
            var rv1 = converter.GetToRedisValueFunc<T>()(value);
            var nv1 = converter.GetFromRedisValueFunc<T>()(rv1);
            Assert.Equal(value, nv1);

            // check from(to(V)) == V (object type version)
            var rv2 = converter.GetToRedisValueFunc(typeof(T))(value);
            var nv2 = converter.GetFromRedisValueFunc(typeof(T))(rv2);
            Assert.Equal(value, nv2);

            // check to1(V) == to2(V)
            Assert.Equal(rv1, rv2);

            if (expectedRedisValue != null)
                Assert.Equal(expectedRedisValue.Value, rv1);
        }
    }
}
