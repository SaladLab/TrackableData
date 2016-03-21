using System;
using System.Threading.Tasks;
using StackExchange.Redis;
using Xunit;

namespace TrackableData.Redis.Tests
{
    public enum Suit : byte
    {
        Spade = 1,
        Heart,
        Diamond,
        Club
    }

    public interface ITestTypePoco : ITrackablePoco<ITestTypePoco>
    {
        bool vBool { get; set; }
        byte vByte { get; set; }
        short vShort { get; set; }
        char vChar { get; set; }
        int vInt { get; set; }
        long vLong { get; set; }
        float vFloat { get; set; }
        double vDouble { get; set; }
        decimal vDecimal { get; set; }
        DateTime vDateTime { get; set; }
        DateTimeOffset vDateTimeOffset { get; set; }
        TimeSpan vTimeSpan { get; set; }
        string vString { get; set; }
        byte[] vBytes { get; set; }
        Guid vGuid { get; set; }
        Suit vSuit { get; set; }
    }

    public class TrackablePocoTestWithTypes : IClassFixture<Redis>
    {
        private static TrackablePocoRedisMapper<ITestTypePoco> _mapper =
            new TrackablePocoRedisMapper<ITestTypePoco>();

        private IDatabase _db;
        private string _testId = "TrackablePocoTestWithTypes";

        public TrackablePocoTestWithTypes(Redis redis)
        {
            _db = redis.Db;
        }

        [Fact]
        public async Task Test_CreateAsIdentity_LoadAndCheckEqual()
        {
            var p0 = new TrackableTestTypePoco();
            p0.vBool = true;
            p0.vByte = 1;
            p0.vShort = 1;
            p0.vChar = '\x1';
            p0.vInt = 1;
            p0.vLong = 1;
            p0.vFloat = 1;
            p0.vDouble = 1;
            p0.vDecimal = 1;
            p0.vDateTime = new DateTime(2001, 1, 1, 1, 1, 1);
            p0.vDateTimeOffset = new DateTimeOffset(2001, 1, 1, 1, 1, 1, TimeSpan.Zero);
            p0.vTimeSpan = new TimeSpan(1, 1, 1);
            p0.vString = "1";
            p0.vBytes = new byte[] { 1 };
            p0.vGuid = new Guid(1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1);
            p0.vSuit = Suit.Spade;

            await _mapper.CreateAsync(_db, p0, _testId);

            var p1 = await _mapper.LoadAsync(_db, _testId);
            AssertTestPocoEqual(p0, p1);
        }

        [Fact]
        public async Task Test_CreateAsMinimum_LoadAndCheckEqual()
        {
            var p0 = new TrackableTestTypePoco();
            p0.vBool = false;
            p0.vByte = 0;
            p0.vShort = -32768;
            p0.vChar = '\x0';
            p0.vInt = -2147483648;
            p0.vLong = -9223372036854775807L;

            // just arbitrary value for float, double, decimal
            p0.vFloat = -9999.99f;
            p0.vDouble = -99999.99;
            p0.vDecimal = -999999.99m;

            p0.vDateTime = DateTime.MinValue;
            p0.vDateTimeOffset = DateTimeOffset.MinValue;
            p0.vTimeSpan = TimeSpan.MinValue;

            p0.vString = "0";
            p0.vBytes = new byte[] { 0 };
            p0.vGuid = Guid.Empty;
            p0.vSuit = (Suit)0;
            await _mapper.CreateAsync(_db, p0, _testId);

            var p1 = await _mapper.LoadAsync(_db, _testId);
            AssertTestPocoEqual(p0, p1);
        }

        [Fact]
        public async Task Test_CreateAsMaximum_LoadAndCheckEqual()
        {
            var p0 = new TrackableTestTypePoco();
            p0.vBool = true;
            p0.vByte = 255;
            p0.vShort = 32767;
            p0.vChar = '\xFFFF';
            p0.vInt = 2147483647;
            p0.vLong = 9223372036854775807L;

            // just arbitrary value for float, double, decimal
            p0.vFloat = 9999.99f;
            p0.vDouble = 99999.99;
            p0.vDecimal = 999999.99m;

            p0.vDateTime = DateTime.MaxValue;
            p0.vDateTimeOffset = DateTimeOffset.MaxValue;
            p0.vTimeSpan = TimeSpan.MaxValue;

            p0.vString = "\xAC00\xD7A3";
            p0.vBytes = new byte[] { 0, 127, 255 };
            p0.vGuid = new Guid(0xFFFFFFFF, 0xFFFF, 0xFFFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF);
            p0.vSuit = (Suit)0xFF;
            await _mapper.CreateAsync(_db, p0, _testId);

            var p1 = await _mapper.LoadAsync(_db, _testId);
            AssertTestPocoEqual(p0, p1);
        }

        private void AssertTestPocoEqual(ITestTypePoco p0, ITestTypePoco p1)
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
            Assert.Equal(p0.vString, p1.vString);
            Assert.Equal(p0.vBytes, p1.vBytes);
            Assert.Equal(p0.vGuid, p1.vGuid);
        }
    }
}
