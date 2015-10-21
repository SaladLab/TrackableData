using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Model;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using Newtonsoft.Json;
using TrackableData.MongoDB;

namespace MigrationTest
{
    class Program
    {
        private static MsSqlDriver _sqlDriver;
        private static MongoDbDriver _mongoDriver;

        static void Main(string[] args)
        {
            // TypeMapper.RegisterClassMap(typeof(UserItem));
            // var ui = new UserItem();

            _sqlDriver = new MsSqlDriver(ConfigurationManager.ConnectionStrings["SourceDb"].ConnectionString);
            _mongoDriver = new MongoDbDriver(ConfigurationManager.ConnectionStrings["TargetDb"].ConnectionString);
            _mongoDriver.Database.DropCollectionAsync("User").Wait();
            // DumpUser(2).Wait();
            LoadAndSave(2).Wait();
        }

        static async Task DumpUser(int uid)
        {
            var user = await _sqlDriver.LoadUserAsync(uid);
            Console.WriteLine(JsonConvert.SerializeObject(user, Formatting.Indented));
        }

        static async Task LoadAndSave(int uid)
        {
            var user = await _sqlDriver.LoadUserAsync(uid);
            await _mongoDriver.CreateUserAsync(1, user);

            var user2 = await _mongoDriver.LoadUserAsync(1);
            await _mongoDriver.CreateUserAsync(2, user2);

            user2.Tracker = new TrackableUserContextTracker();
            user2.Data.Gold += 10;
            user2.Items.Remove(user2.Items.Keys.First());
            user2.Items.Add(2, new UserItem { CharacterId = 1010, Level = 10 });
            user2.Cards.Remove(user2.Cards.Keys.First());

            await _mongoDriver.SaveUserAsync(2, user2.Tracker);
            user2.Tracker.Clear();
        }
    }
}
