using System.Collections.Generic;
using System.Threading.Tasks;
using TrackableData.Sql;
using TrackableData.TestKits;

namespace TrackableData.SqlTestKits
{
    public abstract class TrackableDictionaryDataTest : StorageDictionaryDataTestKit<int>
    {
        private IDbConnectionProvider _db;
        private TrackableDictionarySqlMapper<int, ItemData> _mapper;

        public TrackableDictionaryDataTest(IDbConnectionProvider dbConnectionProvider, ISqlProvider sqlProvider)
            : base(useDuplicateCheck: true)
        {
            _db = dbConnectionProvider;
            _mapper = new TrackableDictionarySqlMapper<int, ItemData>(
                sqlProvider,
                nameof(TrackableDictionaryDataTest),
                new ColumnDefinition("Id"));
            _mapper.ResetTableAsync(_db.Connection).Wait();
        }

        protected override int CreateKey(int value)
        {
            return value;
        }

        protected override Task CreateAsync(IDictionary<int, ItemData> dictionary)
        {
            return _mapper.CreateAsync(_db.Connection, dictionary);
        }

        protected override Task<int> DeleteAsync()
        {
            return _mapper.DeleteAsync(_db.Connection);
        }

        protected override Task<TrackableDictionary<int, ItemData>> LoadAsync()
        {
            return _mapper.LoadAsync(_db.Connection);
        }

        protected override Task SaveAsync(TrackableDictionary<int, ItemData> dictionary)
        {
            return _mapper.SaveAsync(_db.Connection, dictionary.Tracker);
        }
    }
}
