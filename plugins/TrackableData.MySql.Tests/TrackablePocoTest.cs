using MySql.Data.MySqlClient;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using TrackableData.MySql;
using TrackableData.Sql;
using Xunit;

namespace TrackableData.MySql.Tests
{
    public interface ITestPoco : ITrackablePoco<ITestPoco>
    {
        [TrackableProperty("sql.primary-key")]
        int Id { get; set; }
        string Name { get; set; }
        int Age { get; set; }
    }

    public interface ITestPocoWithIdentity : ITrackablePoco<ITestPocoWithIdentity>
    {
        [TrackableProperty("sql.primary-key", "sql.identity")]
        int Id { get; set; }
        string Name { get; set; }
        int Age { get; set; }
    }

    public class TrackablePocoTest : TestKits.StoragePocoTestKit<TrackableTestPoco, int>, IClassFixture<Database>, IDisposable
    {
        private static TrackablePocoSqlMapper<ITestPoco> _mapper =
            new TrackablePocoSqlMapper<ITestPoco>(MySqlProvider.Instance, nameof(TrackablePocoTest));

        private Database _db;
        private MySqlConnection _connection;

        public TrackablePocoTest(Database db)
        {
            _db = db;
            _connection = db.Connection;
            _mapper.ResetTableAsync(_connection).Wait();
        }

        public void Dispose()
        {
            _connection.Dispose();
        }

        protected override Task CreateAsync(TrackableTestPoco person)
        {
            return _mapper.CreateAsync(_connection, person);
        }

        protected override async Task<TrackableTestPoco> LoadAsync(int id)
        {
            return (TrackableTestPoco)(await _mapper.LoadAsync(_connection, id));
        }

        protected override Task<int> DeleteAsync(int id)
        {
            return _mapper.DeleteAsync(_connection, id);
        }

        protected override Task SaveAsync(ITracker tracker, int id)
        {
            return _mapper.SaveAsync(_connection, (TrackablePocoTracker<ITestPoco>)tracker, id);
        }
    }

    public class TrackablePocoWithHeadKeysTest : TestKits.StoragePocoTestKit<TrackableTestPoco, int>, IClassFixture<Database>, IDisposable
    {
        private static readonly ColumnDefinition[] HeadKeyColumnDefs =
        {
            new ColumnDefinition("Head1", typeof(int)),
            new ColumnDefinition("Head2", typeof(string), 100)
        };
        private static TrackablePocoSqlMapper<ITestPoco> _mapper =
            new TrackablePocoSqlMapper<ITestPoco>(MySqlProvider.Instance, nameof(TrackablePocoWithHeadKeysTest), HeadKeyColumnDefs);

        private Database _db;
        private MySqlConnection _connection;

        public TrackablePocoWithHeadKeysTest(Database db)
        {
            _db = db;
            _connection = db.Connection;
            _mapper.ResetTableAsync(_connection).Wait();
        }

        public void Dispose()
        {
            _connection.Dispose();
        }

        protected override Task CreateAsync(TrackableTestPoco person)
        {
            return _mapper.CreateAsync(_connection, person, 1, "One");
        }

        protected override async Task<TrackableTestPoco> LoadAsync(int id)
        {
            return (TrackableTestPoco)(await _mapper.LoadAsync(_connection, 1, "One", id));
        }

        protected override Task<int> DeleteAsync(int id)
        {
            return _mapper.DeleteAsync(_connection, 1, "One", id);
        }

        protected override Task SaveAsync(ITracker tracker, int id)
        {
            return _mapper.SaveAsync(_connection, (TrackablePocoTracker<ITestPoco>)tracker, 1, "One", id);
        }
    }

    public class TrackablePocoWithAutoIdTest : TestKits.StoragePocoWithAutoIdTestKit<TrackableTestPocoWithIdentity, int>, IClassFixture<Database>, IDisposable
    {
        private static TrackablePocoSqlMapper<ITestPocoWithIdentity> _mapper =
            new TrackablePocoSqlMapper<ITestPocoWithIdentity>(MySqlProvider.Instance, nameof(TrackablePocoWithAutoIdTest));

        private Database _db;
        private MySqlConnection _connection;

        public TrackablePocoWithAutoIdTest(Database db)
        {
            _db = db;
            _connection = db.Connection;
            _mapper.ResetTableAsync(_connection).Wait();
        }

        public void Dispose()
        {
            _connection.Dispose();
        }

        protected override Task CreateAsync(TrackableTestPocoWithIdentity person)
        {
            return _mapper.CreateAsync(_connection, person);
        }

        protected override async Task<TrackableTestPocoWithIdentity> LoadAsync(int id)
        {
            return (TrackableTestPocoWithIdentity)(await _mapper.LoadAsync(_connection, id));
        }

        protected override Task<int> DeleteAsync(int id)
        {
            return _mapper.DeleteAsync(_connection, id);
        }

        protected override Task SaveAsync(ITracker tracker, int id)
        {
            return _mapper.SaveAsync(_connection, (TrackablePocoTracker<ITestPocoWithIdentity>)tracker, id);
        }
    }
}
