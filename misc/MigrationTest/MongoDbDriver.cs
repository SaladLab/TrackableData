using System.Configuration;
using System.Data.SqlClient;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MigrationTest
{
    class MongoDbDriver
    {
        private MongoClient _client;
        private IMongoDatabase _database;

        public MongoDbDriver(string connectionString)
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            _client = new MongoClient(builder.DataSource);
            _database = _client.GetDatabase(builder.InitialCatalog);
        }

        public IMongoCollection<BsonDocument> this[string name]
        {
            get { return _database.GetCollection<BsonDocument>("User"); }
        }
    }
}
