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
        private readonly string _collectionName;

        public MongoDbDriver(string connectionString, string collectionName)
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            _client = new MongoClient(builder.DataSource);
            _database = _client.GetDatabase(builder.InitialCatalog);
            _collectionName = collectionName;
        }

        public IMongoDatabase Database => _database;

        public IMongoCollection<BsonDocument> Collection => this[_collectionName];

        public IMongoCollection<BsonDocument> this[string name]
        {
            get { return _database.GetCollection<BsonDocument>(name); }
        }

        public Task CreateUserAsync(int uid, TrackableUserContext user)
        {
            return _userMapper.CreateAsync(this[_collectionName], user, uid);
        }

        public async Task ReplaceUserAsync(int uid, TrackableUserContext user)
        {
            var bson = _userMapper.ConvertToBsonDocument(user);
            await Collection.ReplaceOneAsync(
                Builders<BsonDocument>.Filter.Eq("_id", uid),
                bson);
        }

        public Task DeleteUserAsync(int uid)
        {
            return _userMapper.DeleteAsync(this[_collectionName], uid);
        }

        public async Task<TrackableUserContext> LoadUserAsync(int uid)
        {
            return (TrackableUserContext)(await _userMapper.LoadAsync(this[_collectionName], uid));
        }

        public Task SaveUserAsync(int uid, TrackableUserContextTracker tracker)
        {
            return _userMapper.SaveAsync(this[_collectionName], tracker, uid);
        }
    }
}
