using System;
using System.Threading.Tasks;
using TrackableData.Sql;
using Xunit;

namespace TrackableData.SqlTestKits
{
    public abstract class TrackableTestTypeNullablePocoTest
    {
        private IDbConnectionProvider _db;
        private TrackablePocoSqlMapper<ITestTypeNullablePoco> _mapper;

        protected TrackableTestTypeNullablePocoTest(IDbConnectionProvider dbConnectionProvider, ISqlProvider sqlProvider)
        {
            _db = dbConnectionProvider;
            _mapper = new TrackablePocoSqlMapper<ITestTypeNullablePoco>(sqlProvider,
                                                                        nameof(TrackableTestTypeNullablePoco));
            _mapper.ResetTableAsync(_db.Connection).Wait();
        }

        [Fact]
        public async Task Test_CreateAsDefault_LoadAndCheckEqual()
        {
            var p0 = new TrackableTestTypeNullablePoco();
            await _mapper.CreateAsync(_db.Connection, p0);

            var p1 = await _mapper.LoadAsync(_db.Connection, p0.Id);
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
            await _mapper.CreateAsync(_db.Connection, p0);

            var p1 = await _mapper.LoadAsync(_db.Connection, p0.Id);
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
