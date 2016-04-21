using System;
using System.Threading.Tasks;
using TrackableData.Sql;
using TrackableData.TestKits;

namespace TrackableData.SqlTestKits
{
    public abstract class TypeTest : StorageTypeTestKit
    {
        private IDbConnectionProvider _db;
        private TrackablePocoSqlMapper<ITypeTestPoco> _mapper;

        protected TypeTest(IDbConnectionProvider dbConnectionProvider, ISqlProvider sqlProvider)
        {
            _db = dbConnectionProvider;
            _mapper = new TrackablePocoSqlMapper<ITypeTestPoco>(sqlProvider, nameof(TypeTest));
            _mapper.ResetTableAsync(_db.Connection).Wait();
        }

        protected override Task CreateAsync(TrackableTypeTestPoco data)
        {
            return _mapper.CreateAsync(_db.Connection, data);
        }

        protected override async Task<TrackableTypeTestPoco> LoadAsync(int id)
        {
            return (TrackableTypeTestPoco)(await _mapper.LoadAsync(_db.Connection, id));
        }

        protected override Task SaveAsync(TrackableTypeTestPoco data)
        {
            return _mapper.SaveAsync(_db.Connection, data.Tracker);
        }

        protected override void OnDataInitialized(TrackableTypeTestPoco data)
        {
            base.OnDataInitialized(data);

            if (data.ValInt == int.MinValue)
            {
                // just arbitrary value for float, double, decimal
                data.ValFloat = -9999.99f;
                data.ValDouble = -99999.99;
                data.ValDecimal = -999999.99m;

                // 1000-01-01 is minimum value of mysql datetime
                data.ValDateTime = new DateTime(1000, 1, 1, 0, 0, 0);
                data.ValDateTimeOffset = new DateTimeOffset(1000, 1, 1, 0, 0, 0, TimeSpan.Zero);

                // 00:00:00 is minimum value of mysql
                data.ValTimeSpan = new TimeSpan(0, 0, 0);
            }
            else if (data.ValInt == int.MaxValue)
            {
                // just arbitrary value for float, double, decimal
                data.ValFloat = 9999.99f;
                data.ValDouble = 99999.99;
                data.ValDecimal = 999999.99m;

                // 9999-12-31 is maxmimum value of all sql engines
                data.ValDateTime = new DateTime(9999, 12, 31, 23, 59, 59, 999);

                // 9999-12-30 is used to make PostgreSql pass test
                // (PostgreSql translates utc date to local date while passing value to .NET,
                //  which may causes overflow.)
                data.ValDateTimeOffset = new DateTimeOffset(9999, 12, 30, 23, 59, 59, 999, TimeSpan.Zero);

                // 23:59:59 is maximum value of mysql
                data.ValTimeSpan = new TimeSpan(0, 23, 59, 59, 999);
            }
            else if (data.ValInt == 1)
            {
                data.ValDateTime = new DateTime(2001, 1, 1, 1, 1, 1);
                data.ValDateTimeOffset = new DateTimeOffset(2001, 1, 1, 1, 1, 1, TimeSpan.Zero);
            }
        }
    }
}
