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
            await _mapper.CreateAsync(_db.Connection, p0);

            var p1 = await _mapper.LoadAsync(_db.Connection, p0.Id);
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
