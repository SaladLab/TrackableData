using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TrackableData.MsSql;

namespace MigrationTest
{
    internal class SqlBenchmark
    {
        public static async Task ReadAsync(bool parallel)
        {
            var sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlDb"].ConnectionString);
            sqlConnection.Open();

            var sqlDriver = new SqlDriver(MsSqlProvider.Instance, sqlConnection);

            var uids = new List<int>();
            using (var command = new SqlCommand("SELECT [Uid] FROM tblUser", sqlConnection))
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

            Console.WriteLine(uids.Count);

            var repeatCount = 1000;
            var timer = new Stopwatch();
            timer.Start();

            for (int i = 0; i < repeatCount; i++)
            {
                var tasks = uids.Select(uid => sqlDriver.LoadUserAsync(uid));
                await tasks.WaitForComplete(true);
            }

            var elapsed = timer.Elapsed.TotalSeconds;
            var rowPerSec = repeatCount * uids.Count / timer.Elapsed.TotalSeconds;
            Console.WriteLine($"Elapsed: {(int)elapsed}s RowPerSec: {(int)rowPerSec}");
        }
    }
}
