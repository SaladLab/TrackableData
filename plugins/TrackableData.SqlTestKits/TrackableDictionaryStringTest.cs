using System.Collections.Generic;
using System.Threading.Tasks;
using TrackableData.Sql;
using TrackableData.TestKits;

namespace TrackableData.SqlTestKits
{
    public abstract class TrackableDictionaryStringTest : StorageDictionaryValueTestKit<int>
    {
        private IDbConnectionProvider _db;
        private TrackableDictionarySqlMapper<int, string> _mapper;

        public TrackableDictionaryStringTest(IDbConnectionProvider dbConnectionProvider, ISqlProvider sqlProvider)
            : base(useDuplicateCheck: true)
        {
            _db = dbConnectionProvider;
            _mapper = new TrackableDictionarySqlMapper<int, string>(
                sqlProvider,
                nameof(TrackableDictionaryStringTest),
                new ColumnDefinition("Id"), new ColumnDefinition("Value", typeof(string)),
                null);
            _mapper.ResetTableAsync(_db.Connection).Wait();
        }

        protected override int CreateKey(int value)
        {
            return value;
        }

        protected override Task CreateAsync(IDictionary<int, string> dictionary)
        {
            return _mapper.CreateAsync(_db.Connection, dictionary);
        }

        protected override Task<int> DeleteAsync()
        {
            return _mapper.DeleteAsync(_db.Connection);
        }

        protected override Task<TrackableDictionary<int, string>> LoadAsync()
        {
            return _mapper.LoadAsync(_db.Connection);
        }

        protected override Task SaveAsync(TrackableDictionary<int, string> dictionary)
        {
            return _mapper.SaveAsync(_db.Connection, dictionary.Tracker);
        }
    }
}
