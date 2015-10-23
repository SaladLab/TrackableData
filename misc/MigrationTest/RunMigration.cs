using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace MigrationTest
{
    class RunMigration
    {
        public static async Task MigrateAsync()
        {
            var sqlDriver = new MsSqlDriver(
                ConfigurationManager.ConnectionStrings["SqlDb"].ConnectionString);

            var mongoDriver = new MongoDbDriver(
                ConfigurationManager.ConnectionStrings["MongoDb"].ConnectionString, "User");

            mongoDriver.Database.DropCollectionAsync("User").Wait();

            for (int i = 0; i < 128; i++)
            {
                var uid0 = i * 0x1000000;
                var uid1 = uid0 + 0xFFFFFF;

                var uids = new List<int>();

                mongoDriver.Database.GetCollection<BsonDocument>("User");

                var sql = $"SELECT [Uid] FROM tblUser WHERE [Uid] BETWEEN ${uid0} AND ${uid1}";
                using (var command = new SqlCommand(sql, sqlDriver.Connection))
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

                var timer = new Stopwatch();
                timer.Start();
                Console.Write($"[Step ({i + 1}/128)] Count:{uids.Count} ");
                foreach (var uid in uids)
                {
                    var user = await sqlDriver.LoadUserAsync(uid);
                    await mongoDriver.CreateUserAsync(uid, user);
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
