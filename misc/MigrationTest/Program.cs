using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
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
            _sqlDriver = new MsSqlDriver(ConfigurationManager.ConnectionStrings["SourceDb"].ConnectionString);
            _mongoDriver = new MongoDbDriver(ConfigurationManager.ConnectionStrings["TargetDb"].ConnectionString);
            MigrateAsync().Wait();
        }

        static async Task TestAsync()
        {
            _mongoDriver.Database.DropCollectionAsync("User").Wait();

            var uid = 2;

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

        static async Task MigrateAsync()
        {
            _mongoDriver.Database.DropCollectionAsync("User").Wait();

            for (int i = 0; i < 128; i++)
            {
                var uid0 = i * 0x1000000;
                var uid1 = uid0 + 0xFFFFFF;

                var uids = new List<int>();
                var sql = $"SELECT [Uid] FROM tblUser WHERE [Uid] BETWEEN ${uid0} AND ${uid1}";
                using (var command = new SqlCommand(sql, _sqlDriver.Connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (await reader.ReadAsync())
                        {
                            var uid = (int)reader.GetValue(0);
                            uids.Add(uid);
                        }
                    }
                }
                //if (uids.Count > 100)
                //    uids.RemoveRange(100, uids.Count - 100);

                var timer = new Stopwatch();
                timer.Start();
                Console.Write($"[Step ({i+1}/128)] Count:{uids.Count} ");
                foreach (var uid in uids)
                {
                    var user = await _sqlDriver.LoadUserAsync(uid);
                    await _mongoDriver.CreateUserAsync(uid, user);
                }
                timer.Stop();

                var elapsed = timer.Elapsed.TotalSeconds;
                if (uids.Count > 0 && elapsed > 0)
                {
                    var rowPerSec = uids.Count / timer.Elapsed.TotalSeconds;
                    Console.WriteLine($"Elapsed: {(int)elapsed}s RowPerSec: {(int)rowPerSec}");
                }
                else
                {
                    Console.WriteLine();
                }
            }
        }
    }
}
