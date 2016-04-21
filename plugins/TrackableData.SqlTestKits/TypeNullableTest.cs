using System;
using System.Threading.Tasks;
using TrackableData.Sql;
using TrackableData.TestKits;

namespace TrackableData.SqlTestKits
{
    public abstract class TypeNullableTest : StorageTypeNullableTestKit
    {
        private IDbConnectionProvider _db;
        private TrackablePocoSqlMapper<ITypeNullableTestPoco> _mapper;

        protected TypeNullableTest(IDbConnectionProvider dbConnectionProvider, ISqlProvider sqlProvider)
        {
            _db = dbConnectionProvider;
            _mapper = new TrackablePocoSqlMapper<ITypeNullableTestPoco>(sqlProvider, nameof(TypeNullableTest));
            _mapper.ResetTableAsync(_db.Connection).Wait();
        }

        protected override Task CreateAsync(TrackableTypeNullableTestPoco data)
        {
            return _mapper.CreateAsync(_db.Connection, data);
        }

        protected override async Task<TrackableTypeNullableTestPoco> LoadAsync(int id)
        {
            return (TrackableTypeNullableTestPoco)(await _mapper.LoadAsync(_db.Connection, id));
        }

        protected override Task SaveAsync(TrackableTypeNullableTestPoco data)
        {
            return _mapper.SaveAsync(_db.Connection, data.Tracker);
        }

        protected override void OnDataInitialized(TrackableTypeNullableTestPoco data)
        {
            base.OnDataInitialized(data);

            if (data.ValInt == 1)
            {
                data.ValDateTime = new DateTime(2001, 1, 1, 1, 1, 1);
                data.ValDateTimeOffset = new DateTimeOffset(2001, 1, 1, 1, 1, 1, TimeSpan.Zero);
            }
        }
    }
}
