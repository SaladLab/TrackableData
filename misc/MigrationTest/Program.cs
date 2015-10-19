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
            _mongoDriver.Database.DropCollectionAsync("User").Wait();
            // DumpUser(2).Wait();
            LoadAndSave(2).Wait();
        }

        static async Task DumpUser(int uid)
        {
            var user = await _sqlDriver.LoadUser(uid);
            Console.WriteLine(JsonConvert.SerializeObject(user, Formatting.Indented));
        }

        static async Task LoadAndSave(int uid)
        {
            var user = await _sqlDriver.LoadUser(uid);
            await _mongoDriver.CreateUser(1, user);
        }
    }
}
