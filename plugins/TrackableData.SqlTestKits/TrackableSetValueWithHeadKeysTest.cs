using System.Collections.Generic;
using System.Threading.Tasks;
using TrackableData.Sql;
using TrackableData.TestKits;

namespace TrackableData.SqlTestKits
{
    public abstract class TrackableSetValueWithHeadKeysTest : StorageSetValueTestKit
    {
        private IDbConnectionProvider _db;
        private TrackableSetSqlMapper<int> _mapper;

        public TrackableSetValueWithHeadKeysTest(IDbConnectionProvider dbConnectionProvider, ISqlProvider sqlProvider)
        {
            _db = dbConnectionProvider;
            _mapper = new TrackableSetSqlMapper<int>(
                sqlProvider,
                nameof(TrackableSetValueWithHeadKeysTest),
                new ColumnDefinition("Value", typeof(int)),
                new[]
                {
                    new ColumnDefinition("Head1", typeof(int)),
                    new ColumnDefinition("Head2", typeof(string), 100)
                });
            _mapper.ResetTableAsync(_db.Connection).Wait();
        }

        protected override Task CreateAsync(ICollection<int> set)
        {
            return _mapper.CreateAsync(_db.Connection, set, 1, "One");
        }

        protected override Task<int> DeleteAsync()
        {
            return _mapper.DeleteAsync(_db.Connection, 1, "One");
        }

        protected override Task<TrackableSet<int>> LoadAsync()
        {
            return _mapper.LoadAsync(_db.Connection, 1, "One");
        }

        protected override Task SaveAsync(TrackableSet<int> set)
        {
            return _mapper.SaveAsync(_db.Connection, set.Tracker, 1, "One");
        }
    }
}
