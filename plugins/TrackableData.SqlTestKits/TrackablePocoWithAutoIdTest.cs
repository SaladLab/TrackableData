using System;
using System.Threading.Tasks;
using TrackableData.Sql;
using TrackableData.TestKits;

namespace TrackableData.SqlTestKits
{
    public abstract class TrackablePocoWithAutoIdTest : StoragePocoWithAutoIdTestKit<TrackableTestPocoWithIdentity, int>
    {
        private IDbConnectionProvider _db;
        private TrackablePocoSqlMapper<ITestPocoWithIdentity> _mapper;

        public TrackablePocoWithAutoIdTest(IDbConnectionProvider dbConnectionProvider, ISqlProvider sqlProvider)
        {
            _db = dbConnectionProvider;
            _mapper = new TrackablePocoSqlMapper<ITestPocoWithIdentity>(sqlProvider, nameof(TrackablePocoWithAutoIdTest));
            _mapper.ResetTableAsync(_db.Connection).Wait();
        }

        protected override Task CreateAsync(TrackableTestPocoWithIdentity person)
        {
            return _mapper.CreateAsync(_db.Connection, person);
        }

        protected override async Task<TrackableTestPocoWithIdentity> LoadAsync(int id)
        {
            return (TrackableTestPocoWithIdentity)(await _mapper.LoadAsync(_db.Connection, id));
        }

        protected override Task<int> DeleteAsync(int id)
        {
            return _mapper.DeleteAsync(_db.Connection, id);
        }

        protected override Task SaveAsync(ITracker tracker, int id)
        {
            return _mapper.SaveAsync(_db.Connection, (TrackablePocoTracker<ITestPocoWithIdentity>)tracker, id);
        }
    }
}
