using System;
using System.Threading.Tasks;
using TrackableData.Sql;
using Xunit;

namespace TrackableData.SqlTestKits
{
    public abstract class TrackableTestTypePocoTest
    {
        private IDbConnectionProvider _db;
        private TrackablePocoSqlMapper<ITestTypePoco> _mapper;

        protected TrackableTestTypePocoTest(IDbConnectionProvider dbConnectionProvider, ISqlProvider sqlProvider)
        {
            _db = dbConnectionProvider;
            _mapper = new TrackablePocoSqlMapper<ITestTypePoco>(sqlProvider, nameof(TrackableTestTypePoco));
            _mapper.ResetTableAsync(_db.Connection).Wait();
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
            await _mapper.CreateAsync(_db.Connection, p0);

            var p1 = await _mapper.LoadAsync(_db.Connection, p0.Id);
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
            p0.ValLong = -9223372036854775808L;

            // just arbitrary value for float, double, decimal
            p0.ValFloat = -9999.99f;
            p0.ValDouble = -99999.99;
            p0.ValDecimal = -999999.99m;

            // 1000-01-01 is minimum value of mysql datetime
            p0.ValDateTime = new DateTime(1000, 1, 1, 0, 0, 0);
            p0.ValDateTimeOffset = new DateTimeOffset(1000, 1, 1, 0, 0, 0, TimeSpan.Zero);

            // 00:00:00 is minimum value of mysql
            p0.ValTimeSpan = new TimeSpan(0, 0, 0);

            p0.ValString = "0";
            p0.ValBytes = new byte[] { 0 };
            p0.ValGuid = Guid.Empty;
            p0.ValSuit = (Suit)0;
            await _mapper.CreateAsync(_db.Connection, p0);

            var p1 = await _mapper.LoadAsync(_db.Connection, p0.Id);
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

            // 9999-12-31 is maxmimum value of all sql engines
            p0.ValDateTime = new DateTime(9999, 12, 31, 23, 59, 59, 999);

            // 9999-12-30 is used to make PostgreSql pass test
            // (PostgreSql translates utc date to local date while passing value to .NET,
            //  which may causes overflow.)
            p0.ValDateTimeOffset = new DateTimeOffset(9999, 12, 30, 23, 59, 59, 999, TimeSpan.Zero);

            // 23:59:59 is maximum value of mysql
            p0.ValTimeSpan = new TimeSpan(0, 23, 59, 59, 999);

            p0.ValString = "\xAC00\xD7A3";
            p0.ValBytes = new byte[] { 0, 127, 255 };
            p0.ValGuid = new Guid(0xFFFFFFFF, 0xFFFF, 0xFFFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF);
            p0.ValSuit = (Suit)0xFF;
            await _mapper.CreateAsync(_db.Connection, p0);

            var p1 = await _mapper.LoadAsync(_db.Connection, p0.Id);
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
