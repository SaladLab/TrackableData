using System.Threading.Tasks;
using TrackableData.Sql;
using TrackableData.TestKits;

namespace TrackableData.SqlTestKits
{
    public abstract class TrackablePocoTest : StoragePocoTestKit<TrackableTestPoco, int>
    {
        private IDbConnectionProvider _db;
        private TrackablePocoSqlMapper<ITestPoco> _mapper;

        protected TrackablePocoTest(IDbConnectionProvider dbConnectionProvider, ISqlProvider sqlProvider)
            : base(useDuplicateCheck: true)
        {
            _db = dbConnectionProvider;
            _mapper = new TrackablePocoSqlMapper<ITestPoco>(sqlProvider, nameof(TrackablePocoTest));
            _mapper.ResetTableAsync(_db.Connection).Wait();
        }

        protected override Task CreateAsync(TrackableTestPoco person)
        {
            return _mapper.CreateAsync(_db.Connection, person);
        }

        protected override async Task<TrackableTestPoco> LoadAsync(int id)
        {
            return (TrackableTestPoco)(await _mapper.LoadAsync(_db.Connection, id));
        }

        protected override Task<int> DeleteAsync(int id)
        {
            return _mapper.DeleteAsync(_db.Connection, id);
        }

        protected override Task SaveAsync(TrackableTestPoco person, int id)
        {
            return _mapper.SaveAsync(_db.Connection, person.Tracker, id);
        }
    }
}
