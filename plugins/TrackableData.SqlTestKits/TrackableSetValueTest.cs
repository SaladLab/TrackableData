using System.Collections.Generic;
using System.Threading.Tasks;
using TrackableData.Sql;
using TrackableData.TestKits;

namespace TrackableData.SqlTestKits
{
    public abstract class TrackableSetValueTest : StorageSetValueTestKit
    {
        private IDbConnectionProvider _db;
        private TrackableSetSqlMapper<int> _mapper;

        public TrackableSetValueTest(IDbConnectionProvider dbConnectionProvider, ISqlProvider sqlProvider)
        {
            _db = dbConnectionProvider;
            _mapper = new TrackableSetSqlMapper<int>(
                sqlProvider,
                nameof(TrackableSetValueTest),
                new ColumnDefinition("Value", typeof(int)),
                null);
            _mapper.ResetTableAsync(_db.Connection).Wait();
        }

        protected override Task CreateAsync(ICollection<int> set)
        {
            return _mapper.CreateAsync(_db.Connection, set);
        }

        protected override Task<int> DeleteAsync()
        {
            return _mapper.DeleteAsync(_db.Connection);
        }

        protected override Task<TrackableSet<int>> LoadAsync()
        {
            return _mapper.LoadAsync(_db.Connection);
        }

        protected override Task SaveAsync(TrackableSet<int> set)
        {
            return _mapper.SaveAsync(_db.Connection, set.Tracker);
        }
    }
}
