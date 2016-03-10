using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using TrackableData;

namespace MigrationTest
{
    internal class RunBenchmark
    {
        private static async Task<List<int>> LoadUids(IMongoCollection<BsonDocument> collection, int uidKey)
        {
            var uid0 = uidKey * 0x1000000;
            var uid1 = uid0 + 0xFFFFFF;

            var uids = new List<int>();
            await collection
                .Find(Builders<BsonDocument>.Filter.Gte("_id", uid0) &
                      Builders<BsonDocument>.Filter.Lte("_id", uid1))
                .Project(Builders<BsonDocument>.Projection.Include("_id"))
                .ForEachAsync(x => uids.Add((int)x["_id"]));
            return uids;
        }

        private static async Task<int> WaitForComplete(IEnumerable<Task<int>> tasks, bool parallel)
        {
            var totalCount = 0;
            if (parallel)
            {
                var counts = await Task.WhenAll(tasks);
                return counts.Sum();
            }
            else
            {
                int i = 0;
                foreach (var task in tasks)
                {
                    Console.WriteLine($"#{i++}");
                    totalCount += await task;
                }
                return totalCount;
            }
        }

        public static async Task ReadAsync(bool parallel)
        {
            var mongoDriver = new MongoDbDriver(
                ConfigurationManager.ConnectionStrings["MongoDb"].ConnectionString, "User");

            var timer = new Stopwatch();
            timer.Start();

            var tasks = Enumerable.Range(0, 128).Select(async i =>
            {
                var uids = await LoadUids(mongoDriver.Collection, i);
                foreach (var uid in uids)
                    await mongoDriver.LoadUserAsync(uid);

                return uids.Count;
            });

            var totalCount = await WaitForComplete(tasks, parallel);

            var elapsed = timer.Elapsed.TotalSeconds;
            var rowPerSec = totalCount / timer.Elapsed.TotalSeconds;
            Console.WriteLine($"Elapsed: {(int)elapsed}s RowPerSec: {(int)rowPerSec}");
        }

        public static async Task DuplicateAsync(bool parallel)
        {
            var mongoDriver = new MongoDbDriver(
                ConfigurationManager.ConnectionStrings["MongoDb"].ConnectionString, "User");

            var mongoDriver2 = new MongoDbDriver(
                ConfigurationManager.ConnectionStrings["MongoDb"].ConnectionString, "User2");

            await mongoDriver2.Database.DropCollectionAsync("User2");

            var timer = new Stopwatch();
            timer.Start();

            var tasks = Enumerable.Range(0, 128).Select(async i =>
            {
                var uids = await LoadUids(mongoDriver.Collection, i);
                foreach (var uid in uids)
                {
                    var user = await mongoDriver.LoadUserAsync(uid);
                    await mongoDriver2.CreateUserAsync(uid, user);
                }
                return uids.Count;
            });

            var totalCount = await WaitForComplete(tasks, parallel);

            var elapsed = timer.Elapsed.TotalSeconds;
            var rowPerSec = totalCount / timer.Elapsed.TotalSeconds;
            Console.WriteLine($"Elapsed: {(int)elapsed}s RowPerSec: {(int)rowPerSec}");
        }

        public static async Task ReplaceAsync(bool parallel)
        {
            var mongoDriver2 = new MongoDbDriver(
                ConfigurationManager.ConnectionStrings["MongoDb"].ConnectionString, "User2");

            var timer = new Stopwatch();
            timer.Start();

            var tasks = Enumerable.Range(0, 128).Select(async i =>
            {
                var uids = await LoadUids(mongoDriver2.Collection, i);
                foreach (var uid in uids)
                {
                    var user = await mongoDriver2.LoadUserAsync(uid);
                    await mongoDriver2.ReplaceUserAsync(uid, user);
                }
                return uids.Count;
            });

            var totalCount = await WaitForComplete(tasks, parallel);

            var elapsed = timer.Elapsed.TotalSeconds;
            var rowPerSec = totalCount / timer.Elapsed.TotalSeconds;
            Console.WriteLine($"Elapsed: {(int)elapsed}s RowPerSec: {(int)rowPerSec}");
        }

        public static async Task SaveSimpleAsync(bool parallel)
        {
            var mongoDriver2 = new MongoDbDriver(
                ConfigurationManager.ConnectionStrings["MongoDb"].ConnectionString, "User2");

            var timer = new Stopwatch();
            timer.Start();

            var tasks = Enumerable.Range(0, 128).Select(async i =>
            {
                var uids = await LoadUids(mongoDriver2.Collection, i);
                foreach (var uid in uids)
                {
                    var user = await mongoDriver2.LoadUserAsync(uid);
                    user.SetDefaultTracker();
                    user.Data.Gold += 1;
                    user.Data.Gold -= 1;
                    await mongoDriver2.SaveUserAsync(uid, user.Tracker);
                }
                return uids.Count;
            });

            var totalCount = await WaitForComplete(tasks, parallel);

            var elapsed = timer.Elapsed.TotalSeconds;
            var rowPerSec = totalCount / timer.Elapsed.TotalSeconds;
            Console.WriteLine($"Elapsed: {(int)elapsed}s RowPerSec: {(int)rowPerSec}");
        }

        public static async Task SaveComplexAsync(bool parallel, bool full)
        {
            var mongoDriver2 = new MongoDbDriver(
                ConfigurationManager.ConnectionStrings["MongoDb"].ConnectionString, "User2");

            var timer = new Stopwatch();
            timer.Start();

            var tasks = Enumerable.Range(0, 128).Select(async i =>
            {
                var uids = await LoadUids(mongoDriver2.Collection, i);
                foreach (var uid in uids)
                {
                    var user = await mongoDriver2.LoadUserAsync(uid);
                    user.SetDefaultTracker();
                    user.Data.Gold += 1;
                    user.Data.Gold -= 1;

                    foreach (var key in user.Items.Keys.ToList())
                    {
                        var item = user.Items[key];
                        user.Items[key] = item;
                        if (full == false)
                            break;
                    }

                    foreach (var key in user.Teams.Keys.ToList())
                    {
                        var team = user.Teams[key];
                        user.Teams[key] = team;
                        if (full == false)
                            break;
                    }

                    await mongoDriver2.SaveUserAsync(uid, user.Tracker);
                }
                return uids.Count;
            });

            var totalCount = await WaitForComplete(tasks, parallel);

            var elapsed = timer.Elapsed.TotalSeconds;
            var rowPerSec = totalCount / timer.Elapsed.TotalSeconds;
            Console.WriteLine($"Elapsed: {(int)elapsed}s RowPerSec: {(int)rowPerSec}");
        }
    }
}
