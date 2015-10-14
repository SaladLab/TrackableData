using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace TrackableData.Sql.Tests
{
    public class TrackablePocoTest : IClassFixture<Database>
    {
        private Database _db;

        public TrackablePocoTest(Database db)
        {
            _db = db;
        }

        [Fact]
        public async Task Test_SqlMapper_LoadPoco()
        {
            var a = new TrackablePocoSqlMapper<IPerson>("tblPerson");

            // CREATE

            //var sql = a.GenerateCreateTableSql();
            //using (var connection = _db.Connection)
            //using (var command = new SqlCommand(sql, connection))
            //{
            //    command.ExecuteNonQuery();
            //}

            // SELECT

            // var sql = a.GenerateSelectSql();
            //using (var connection = _db.Connection)
            //using (var command = new SqlCommand(sql, connection))
            //{
            //    using (var reader = command.ExecuteReader())
            //    {
            //        while (reader.Read())
            //        {
            //            var person = a.Fetch(reader);
            //            Console.WriteLine(person);
            //        }
            //    }
            //}

            // UPDATE

            using (var connection = _db.Connection)
            {
                var poco = (TrackablePerson)await a.LoadAsync(connection, 1);
                poco.SetDefaultTracker();
                poco.Age += 1;

                var tracker = (TrackablePocoTracker<IPerson>)poco.Tracker;
                await a.SaveAsync(connection, tracker, 1);
            }

            /*
            using (var connection = _db.Connection)
            using (var command = new SqlCommand("SELECT TOP 2 * FROM tblTest", connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    Console.WriteLine("{0} {1} {2}",
                    reader.GetInt32(0), reader.GetString(1), reader.GetString(2));
                }
            }*/
        }

        [Fact]
        public void Test_SqlMapper_UpdateChanges()
        {
        }
    }
}
