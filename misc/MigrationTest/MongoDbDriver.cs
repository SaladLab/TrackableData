using System.Data.SqlClient;
using System.Threading.Tasks;
using Model;
using MongoDB.Bson;
using MongoDB.Driver;
using TrackableData.MongoDB;

namespace MigrationTest
{
    class MongoDbDriver
    {
        private static TrackablePocoMongoDbMapper<IUserData> _userDataMapper =
            new TrackablePocoMongoDbMapper<IUserData>();

        private static TrackableDictionaryMongoDbMapper<int, UserItem> _userItemMapper =
            new TrackableDictionaryMongoDbMapper<int, UserItem>();

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

        public async Task CreateUser(int uid, TrackableUserContext user)
        {
            var userCollection = this["User"];
            await userCollection.InsertOneAsync(new BsonDocument().Add("_id", 1));
            await _userDataMapper.CreateAsync(userCollection, user.Data, uid, "Data");
            await _userItemMapper.SaveAsync(userCollection, user.Items, uid, "Items");
        }
    }
}
