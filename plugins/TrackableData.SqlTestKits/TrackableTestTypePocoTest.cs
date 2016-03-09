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
            await _mapper.CreateAsync(_db.Connection, p0);

            var p1 = await _mapper.LoadAsync(_db.Connection, p0.Id);
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
            p0.vLong = -9223372036854775808L;

            // just arbitrary value for float, double, decimal
            p0.vFloat = -9999.99f;
            p0.vDouble = -99999.99;
            p0.vDecimal = -999999.99m;

            // 1000-01-01 is minimum value of mysql datetime
            p0.vDateTime = new DateTime(1000, 1, 1, 0, 0, 0);
            p0.vDateTimeOffset = new DateTimeOffset(1000, 1, 1, 0, 0, 0, TimeSpan.Zero);

            // 00:00:00 is minimum value of mysql
            p0.vTimeSpan = new TimeSpan(0, 0, 0);

            p0.vString = "0";
            p0.vBytes = new byte[] { 0 };
            p0.vGuid = Guid.Empty;
            await _mapper.CreateAsync(_db.Connection, p0);

            var p1 = await _mapper.LoadAsync(_db.Connection, p0.Id);
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

            // 9999-12-31 is maxmimum value of all sql engines
            p0.vDateTime = new DateTime(9999, 12, 31, 23, 59, 59, 999);

            // 9999-12-30 is used to make PostgreSql pass test
            // (PostgreSql translates utc date to local date while passing value to .NET, 
            //  which may causes overflow.)
            p0.vDateTimeOffset = new DateTimeOffset(9999, 12, 30, 23, 59, 59, 999, TimeSpan.Zero);

            // 23:59:59 is maximum value of mysql
            p0.vTimeSpan = new TimeSpan(0, 23, 59, 59, 999);

            p0.vString = "\xAC00\xD7A3";
            p0.vBytes = new byte[] { 0, 127, 255 };
            p0.vGuid = new Guid(0xFFFFFFFF, 0xFFFF, 0xFFFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF);
            await _mapper.CreateAsync(_db.Connection, p0);

            var p1 = await _mapper.LoadAsync(_db.Connection, p0.Id);
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
