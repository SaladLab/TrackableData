using System;
using System.Threading.Tasks;
using StackExchange.Redis;
using Xunit;

namespace TrackableData.Redis.Tests
{
    public interface ITestTypeNullablePoco : ITrackablePoco<ITestTypeNullablePoco>
    {
        int Id { get; set; }
        bool? ValBool { get; set; }
        byte? ValByte { get; set; }
        short? ValShort { get; set; }
        char? ValChar { get; set; }
        int? ValInt { get; set; }
        long? ValLong { get; set; }
        float? ValFloat { get; set; }
        double? ValDouble { get; set; }
        decimal? ValDecimal { get; set; }
        DateTime? ValDateTime { get; set; }
        DateTimeOffset? ValDateTimeOffset { get; set; }
        TimeSpan? ValTimeSpan { get; set; }
        Guid? ValGuid { get; set; }
        Suit? ValSuit { get; set; }
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
            p0.ValBool = true;
            p0.ValByte = 1;
            p0.ValShort = 1;
            p0.ValChar = '\x1';
            p0.ValFloat = 1;
            p0.ValDouble = 1;
            p0.ValDecimal = 1;
            p0.ValDateTime = new DateTime(2001, 1, 1, 1, 1, 1);
            p0.ValDateTimeOffset = new DateTimeOffset(2001, 1, 1, 1, 1, 1, TimeSpan.Zero);
            p0.ValTimeSpan = new TimeSpan(1, 1, 1);
            p0.ValGuid = new Guid(1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1);
            p0.ValSuit = Suit.Spade;
            await _mapper.CreateAsync(_db, p0, _testId);

            var p1 = await _mapper.LoadAsync(_db, _testId);
            AssertTestPocoEqual(p0, p1);
        }

        private void AssertTestPocoEqual(ITestTypeNullablePoco p0, ITestTypeNullablePoco p1)
        {
            Assert.Equal(p0.ValBool, p1.ValBool);
            Assert.Equal(p0.ValByte, p1.ValByte);
            Assert.Equal(p0.ValShort, p1.ValShort);
            Assert.Equal(p0.ValChar, p1.ValChar);
            Assert.Equal(p0.ValInt, p1.ValInt);
            Assert.Equal(p0.ValLong, p1.ValLong);
            Assert.Equal(p0.ValFloat, p1.ValFloat);
            Assert.Equal(p0.ValDouble, p1.ValDouble);
            Assert.Equal(p0.ValDecimal, p1.ValDecimal);
            Assert.Equal(p0.ValDateTime, p1.ValDateTime);
            Assert.Equal(p0.ValDateTimeOffset, p1.ValDateTimeOffset);
            Assert.Equal(p0.ValTimeSpan, p1.ValTimeSpan);
            Assert.Equal(p0.ValGuid, p1.ValGuid);
        }
    }
}
