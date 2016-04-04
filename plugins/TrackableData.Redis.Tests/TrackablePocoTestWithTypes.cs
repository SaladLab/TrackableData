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
        bool ValBool { get; set; }
        byte ValByte { get; set; }
        short ValShort { get; set; }
        char ValChar { get; set; }
        int ValInt { get; set; }
        long ValLong { get; set; }
        float ValFloat { get; set; }
        double ValDouble { get; set; }
        decimal ValDecimal { get; set; }
        DateTime ValDateTime { get; set; }
        DateTimeOffset ValDateTimeOffset { get; set; }
        TimeSpan ValTimeSpan { get; set; }
        string ValString { get; set; }
        byte[] ValBytes { get; set; }
        Guid ValGuid { get; set; }
        Suit ValSuit { get; set; }
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
            p0.ValBool = true;
            p0.ValByte = 1;
            p0.ValShort = 1;
            p0.ValChar = '\x1';
            p0.ValInt = 1;
            p0.ValLong = 1;
            p0.ValFloat = 1;
            p0.ValDouble = 1;
            p0.ValDecimal = 1;
            p0.ValDateTime = new DateTime(2001, 1, 1, 1, 1, 1);
            p0.ValDateTimeOffset = new DateTimeOffset(2001, 1, 1, 1, 1, 1, TimeSpan.Zero);
            p0.ValTimeSpan = new TimeSpan(1, 1, 1);
            p0.ValString = "1";
            p0.ValBytes = new byte[] { 1 };
            p0.ValGuid = new Guid(1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1);
            p0.ValSuit = Suit.Spade;

            await _mapper.CreateAsync(_db, p0, _testId);

            var p1 = await _mapper.LoadAsync(_db, _testId);
            AssertTestPocoEqual(p0, p1);
        }

        [Fact]
        public async Task Test_CreateAsMinimum_LoadAndCheckEqual()
        {
            var p0 = new TrackableTestTypePoco();
            p0.ValBool = false;
            p0.ValByte = 0;
            p0.ValShort = -32768;
            p0.ValChar = '\x0';
            p0.ValInt = -2147483648;
            p0.ValLong = -9223372036854775807L;

            // just arbitrary value for float, double, decimal
            p0.ValFloat = -9999.99f;
            p0.ValDouble = -99999.99;
            p0.ValDecimal = -999999.99m;

            p0.ValDateTime = DateTime.MinValue;
            p0.ValDateTimeOffset = DateTimeOffset.MinValue;
            p0.ValTimeSpan = TimeSpan.MinValue;

            p0.ValString = "0";
            p0.ValBytes = new byte[] { 0 };
            p0.ValGuid = Guid.Empty;
            p0.ValSuit = (Suit)0;
            await _mapper.CreateAsync(_db, p0, _testId);

            var p1 = await _mapper.LoadAsync(_db, _testId);
            AssertTestPocoEqual(p0, p1);
        }

        [Fact]
        public async Task Test_CreateAsMaximum_LoadAndCheckEqual()
        {
            var p0 = new TrackableTestTypePoco();
            p0.ValBool = true;
            p0.ValByte = 255;
            p0.ValShort = 32767;
            p0.ValChar = '\xFFFF';
            p0.ValInt = 2147483647;
            p0.ValLong = 9223372036854775807L;

            // just arbitrary value for float, double, decimal
            p0.ValFloat = 9999.99f;
            p0.ValDouble = 99999.99;
            p0.ValDecimal = 999999.99m;

            p0.ValDateTime = DateTime.MaxValue;
            p0.ValDateTimeOffset = DateTimeOffset.MaxValue;
            p0.ValTimeSpan = TimeSpan.MaxValue;

            p0.ValString = "\xAC00\xD7A3";
            p0.ValBytes = new byte[] { 0, 127, 255 };
            p0.ValGuid = new Guid(0xFFFFFFFF, 0xFFFF, 0xFFFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF);
            p0.ValSuit = (Suit)0xFF;
            await _mapper.CreateAsync(_db, p0, _testId);

            var p1 = await _mapper.LoadAsync(_db, _testId);
            AssertTestPocoEqual(p0, p1);
        }

        private void AssertTestPocoEqual(ITestTypePoco p0, ITestTypePoco p1)
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
            Assert.Equal(p0.ValString, p1.ValString);
            Assert.Equal(p0.ValBytes, p1.ValBytes);
            Assert.Equal(p0.ValGuid, p1.ValGuid);
        }
    }
}
