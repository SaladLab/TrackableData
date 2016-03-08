using System.Collections.Generic;
using System.Threading.Tasks;
using TrackableData.Sql;
using TrackableData.TestKits;

namespace TrackableData.SqlTestKits
{
    public abstract class TrackableDictionaryDataWithHeadKeysTest : StorageDictionaryDataTestKit<int>
    {
        private IDbConnectionProvider _db;
        private TrackableDictionarySqlMapper<int, ItemData> _mapper;

        public TrackableDictionaryDataWithHeadKeysTest(IDbConnectionProvider dbConnectionProvider,
                                                       ISqlProvider sqlProvider)
        {
            _db = dbConnectionProvider;
            _mapper = new TrackableDictionarySqlMapper<int, ItemData>(
                sqlProvider,
                nameof(TrackableDictionaryDataWithHeadKeysTest),
                new ColumnDefinition("Id"),
                new[]
                {
                    new ColumnDefinition("Head1", typeof(int)),
                    new ColumnDefinition("Head2", typeof(string), 100)
                });

            _mapper.ResetTableAsync(_db.Connection).Wait();
        }

        protected override int CreateKey(int value)
        {
            return value;
        }

        protected override Task CreateAsync(IDictionary<int, ItemData> dictionary)
        {
            return _mapper.CreateAsync(_db.Connection, dictionary, 1, "One");
        }

        protected override Task<int> DeleteAsync()
        {
            return _mapper.DeleteAsync(_db.Connection, 1, "One");
        }

        protected override Task<TrackableDictionary<int, ItemData>> LoadAsync()
        {
            return _mapper.LoadAsync(_db.Connection, 1, "One");
        }

        protected override Task SaveAsync(ITracker tracker)
        {
            return _mapper.SaveAsync(_db.Connection, (TrackableDictionaryTracker<int, ItemData>)tracker, 1, "One");
        }
    }
}
