using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using TrackableData.MsSql;
using TrackableData.TestKits;
using Xunit;

namespace TrackableData.MsSql.Tests
{
    public class TrackableDictionaryStringTest : StorageDictionaryValueTestKit<int>, IClassFixture<Database>, IDisposable
    {
        private static readonly ColumnDefinition SingleValueColumnDef = new ColumnDefinition("Value", typeof(string));

        private static TrackableDictionaryMsSqlMapper<int, string> _mapper =
            new TrackableDictionaryMsSqlMapper<int, string>("String", new ColumnDefinition("Id"), SingleValueColumnDef, null);

        private Database _db;
        private SqlConnection _connection;

        public TrackableDictionaryStringTest(Database db)
        {
            _db = db;
            _connection = db.Connection;
            _mapper.ResetTableAsync(_connection).Wait();
        }

        public void Dispose()
        {
            _connection.Dispose();
        }

        protected override int CreateKey(int value)
        {
            return value;
        }

        protected override Task<TrackableDictionary<int, string>> LoadAsync()
        {
            return _mapper.LoadAsync(_connection);
        }

        protected override Task SaveAsync(ITracker tracker)
        {
            return _mapper.SaveAsync(_connection, (TrackableDictionaryTracker<int, string>)tracker);
        }
    }

    public class TrackableDictionaryDataTest : StorageDictionaryDataTestKit<int>, IClassFixture<Database>, IDisposable
    {
        private static TrackableDictionaryMsSqlMapper<int, ItemData> _mapper =
            new TrackableDictionaryMsSqlMapper<int, ItemData>(nameof(ItemData), new ColumnDefinition("Id"));

        private Database _db;
        private SqlConnection _connection;

        public TrackableDictionaryDataTest(Database db)
        {
            _db = db;
            _connection = db.Connection;
            _mapper.ResetTableAsync(_connection).Wait();
        }

        public void Dispose()
        {
            _connection.Dispose();
        }

        protected override int CreateKey(int value)
        {
            return value;
        }

        protected override Task<TrackableDictionary<int, ItemData>> LoadAsync()
        {
            return _mapper.LoadAsync(_connection);
        }

        protected override Task SaveAsync(ITracker tracker)
        {
            return _mapper.SaveAsync(_connection, (TrackableDictionaryTracker<int, ItemData>)tracker);
        }
    }

    public class TrackableDictionaryDataWithHeadKeysTest : StorageDictionaryDataTestKit<int>, IClassFixture<Database>, IDisposable
    {
        private static readonly ColumnDefinition[] HeadKeyColumnDefs =
        {
            new ColumnDefinition("Head1", typeof(int)),
            new ColumnDefinition("Head2", typeof(string), 100)
        };
        private static TrackableDictionaryMsSqlMapper<int, ItemData> _mapper =
            new TrackableDictionaryMsSqlMapper<int, ItemData>(nameof(ItemData), new ColumnDefinition("Id"), HeadKeyColumnDefs);

        private Database _db;
        private SqlConnection _connection;

        public TrackableDictionaryDataWithHeadKeysTest(Database db)
        {
            _db = db;
            _connection = db.Connection;
            _mapper.ResetTableAsync(_connection).Wait();
        }

        public void Dispose()
        {
            _connection.Dispose();
        }

        protected override int CreateKey(int value)
        {
            return value;
        }

        protected override Task<TrackableDictionary<int, ItemData>> LoadAsync()
        {
            return _mapper.LoadAsync(_connection, 1, "One");
        }

        protected override Task SaveAsync(ITracker tracker)
        {
            return _mapper.SaveAsync(_connection, (TrackableDictionaryTracker<int, ItemData>)tracker, 1, "One");
        }
    }
}
