using StackExchange.Redis;
using System;
using System.Threading.Tasks;
using Xunit;

namespace TrackableData.Redis.Tests
{
    public interface ITestTypeNullablePoco : ITrackablePoco<ITestTypeNullablePoco>
    {
        int Id { get; set; }
        bool? vBool { get; set; }
        byte? vByte { get; set; }
        short? vShort { get; set; }
        char? vChar { get; set; }
        int? vInt { get; set; }
        long? vLong { get; set; }
        float? vFloat { get; set; }
        double? vDouble { get; set; }
        decimal? vDecimal { get; set; }
        DateTime? vDateTime { get; set; }
        DateTimeOffset? vDateTimeOffset { get; set; }
        TimeSpan? vTimeSpan { get; set; }
        Guid? vGuid { get; set; }
        Suit? vSuit { get; set; }
    }
    
    public class TrackablePocoTestWithNullableTypes : IClassFixture<Redis>
    {
        private static TrackablePocoRedisMapper<ITestTypeNullablePoco> _mapper =
            new TrackablePocoRedisMapper<ITestTypeNullablePoco>();

        private IDatabase _db;
        private string _testId = "TrackablePocoTestWithNullableTypes";

        public TrackablePocoTestWithNullableTypes(Redis redis)
        {
            _db = redis.Db;
        }

        [Fact]
        public async Task Test_CreateAsDefault_LoadAndCheckEqual()
        {
            var p0 = new TrackableTestTypeNullablePoco();
            await _mapper.CreateAsync(_db, p0, _testId);

            var p1 = await _mapper.LoadAsync(_db, _testId);
            AssertTestPocoEqual(p0, p1);
        }

        [Fact]
        public async Task Test_CreateAsIdentity_LoadAndCheckEqual()
        {
            var p0 = new TrackableTestTypeNullablePoco();
            p0.vBool = true;
            p0.vByte = 1;
            p0.vShort = 1;
            p0.vChar = '\x1';
            p0.vFloat = 1;
            p0.vDouble = 1;
            p0.vDecimal = 1;
            p0.vDateTime = new DateTime(2001, 1, 1, 1, 1, 1);
            p0.vDateTimeOffset = new DateTimeOffset(2001, 1, 1, 1, 1, 1, TimeSpan.Zero);
            p0.vTimeSpan = new TimeSpan(1, 1, 1);
            p0.vGuid = new Guid(1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1);
            p0.vSuit = Suit.Spade;
            await _mapper.CreateAsync(_db, p0, _testId);

            var p1 = await _mapper.LoadAsync(_db, _testId);
            AssertTestPocoEqual(p0, p1);
        }

        private void AssertTestPocoEqual(ITestTypeNullablePoco p0, ITestTypeNullablePoco p1)
        {
            Assert.Equal(p0.vBool, p1.vBool);
            Assert.Equal(p0.vByte, p1.vByte);
            Assert.Equal(p0.vShort, p1.vShort);
            Assert.Equal(p0.vChar, p1.vChar);
            Assert.Equal(p0.vInt, p1.vInt);
            Assert.Equal(p0.vLong, p1.vLong);
            Assert.Equal(p0.vFloat, p1.vFloat);
            Assert.Equal(p0.vDouble, p1.vDouble);
            Assert.Equal(p0.vDecimal, p1.vDecimal);
            Assert.Equal(p0.vDateTime, p1.vDateTime);
            Assert.Equal(p0.vDateTimeOffset, p1.vDateTimeOffset);
            Assert.Equal(p0.vTimeSpan, p1.vTimeSpan);
            Assert.Equal(p0.vGuid, p1.vGuid);
        }
    }
}
