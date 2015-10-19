using System;
using System.Configuration;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MigrationTest
{
    class Program
    {
        private static MsSqlDriver _sqlDriver;
        private static MongoDbDriver _mongoDriver;

        static void Main(string[] args)
        {
            _sqlDriver = new MsSqlDriver(ConfigurationManager.ConnectionStrings["SourceDb"].ConnectionString);
            _mongoDriver = new MongoDbDriver(ConfigurationManager.ConnectionStrings["TargetDb"].ConnectionString);
            DumpUser(1).Wait();
        }

        static async Task DumpUser(int uid)
        {
            var user = await _sqlDriver.LoadUser(uid);
            Console.WriteLine(JsonConvert.SerializeObject(user, Formatting.Indented));
        }

        //static async Task LoadAndDump()
        //{
        //    var user = await _userSqlMapper.LoadAsync(_sqlConnection, 1);
        //    Console.WriteLine(JsonConvert.SerializeObject(user, Formatting.Indented));
        //}

        //static async Task LoadAndSave()
        //{
        //    var user = await _userSqlMapper.LoadAsync(_sqlConnection, 1);
        //    await _mongoCollection.InsertOneAsync(new BsonDocument().Add("_id", 1));
        //    await _userMongoMapper.CreateAsync(_mongoCollection, user, 1, "Data");
        //}
    }
}
