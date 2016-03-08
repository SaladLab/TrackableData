using System.Threading.Tasks;
using TrackableData.Sql;
using TrackableData.TestKits;

namespace TrackableData.SqlTestKits
{
    public class TrackablePocoWithHeadKeysTest : StoragePocoTestKit<TrackableTestPoco, int>
    {
        private IDbConnectionProvider _db;
        private TrackablePocoSqlMapper<ITestPoco> _mapper;

        public TrackablePocoWithHeadKeysTest(IDbConnectionProvider dbConnectionProvider, ISqlProvider sqlProvider)
        {
            _db = dbConnectionProvider;
            _mapper = new TrackablePocoSqlMapper<ITestPoco>(
                sqlProvider,
                nameof(TrackablePocoWithHeadKeysTest),
                new[]
                {
                    new ColumnDefinition("Head1", typeof(int)),
                    new ColumnDefinition("Head2", typeof(string), 100)
                });
            _mapper.ResetTableAsync(_db.Connection).Wait();
        }

        protected override Task CreateAsync(TrackableTestPoco person)
        {
            return _mapper.CreateAsync(_db.Connection, person, 1, "One");
        }

        protected override async Task<TrackableTestPoco> LoadAsync(int id)
        {
            return (TrackableTestPoco)(await _mapper.LoadAsync(_db.Connection, 1, "One", id));
        }

        protected override Task<int> DeleteAsync(int id)
        {
            return _mapper.DeleteAsync(_db.Connection, 1, "One", id);
        }

        protected override Task SaveAsync(ITracker tracker, int id)
        {
            return _mapper.SaveAsync(_db.Connection, (TrackablePocoTracker<ITestPoco>)tracker, 1, "One", id);
        }
    }
}
