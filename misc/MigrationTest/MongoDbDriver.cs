using System.Data.SqlClient;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
using Model;
using MongoDB.Bson;
using MongoDB.Driver;
using TrackableData;
using TrackableData.MongoDB;

namespace MigrationTest
{
    class MongoDbDriver
    {
        private static TrackableContainerMongoDbMapper<IUserContext> _userMapper =
            new TrackableContainerMongoDbMapper<IUserContext>();

        private readonly MongoClient _client;
        private readonly IMongoDatabase _database;

        public MongoDbDriver(string connectionString)
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            _client = new MongoClient(builder.DataSource);
            _database = _client.GetDatabase(builder.InitialCatalog);
        }

        public IMongoDatabase Database => _database;

        public IMongoCollection<BsonDocument> this[string name]
        {
            get { return _database.GetCollection<BsonDocument>(name); }
        }

        public Task CreateUserAsync(int uid, TrackableUserContext user)
        {
            return _userMapper.CreateAsync(this["User"], user, uid);
        }

        public Task DeleteUserAsync(int uid)
        {
            return _userMapper.DeleteAsync(this["User"], uid);
        }

        public async Task<TrackableUserContext> LoadUserAsync(int uid)
        {
            return (TrackableUserContext)(await _userMapper.LoadAsync(this["User"], uid));
        }

        public Task SaveUserAsync(int uid, TrackableUserContextTracker tracker)
        {
            return _userMapper.SaveAsync(this["User"], tracker, uid);
        }
    }
}
