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

        private struct Context<T> : IDisposable 
            where T : ITrackablePoco
        {
            public TrackablePocoSqlMapper<T> SqlMapper;
            public SqlConnection Connection;

            public void Dispose()
            {
                if (Connection != null)
                {
                    Connection.Dispose();
                    Connection = null;
                }
            }
        }

        private async Task<Context<T>> PrepareAsync<T>()
            where T : ITrackablePoco
        {
            var sqlMapper = new TrackablePocoSqlMapper<T>(typeof(T).Name);
            var connection = _db.Connection;
            await sqlMapper.ResetTableAsync(connection);
            return new Context<T> { SqlMapper = sqlMapper, Connection = connection };
        }

        [Fact]
        public async Task Test_SqlMapper_ResetTable()
        {
            using (var ctx = await PrepareAsync<IPerson>())
            {
                var persons = await ctx.SqlMapper.LoadAllAsync(ctx.Connection);
                Assert.Equal(0, persons.Count);
            }
        }

        [Fact]
        public async Task Test_SqlMapper_CreateAndLoadPoco()
        {
            using (var ctx = await PrepareAsync<IPerson>())
            {
                var person = new TrackablePerson();
                person.Id = 1;
                person.Name = "Testor";
                person.Age = 10;

                await ctx.SqlMapper.CreateAsync(ctx.Connection, person);

                var person2 = await ctx.SqlMapper.LoadAsync(ctx.Connection);
                Assert.Equal(person.Id, person2.Id);
                Assert.Equal(person.Name, person2.Name);
                Assert.Equal(person.Age, person2.Age);
            }
        }

        [Fact]
        public async Task Test_SqlMapper_DeletePoco()
        {
            using (var ctx = await PrepareAsync<IPerson>())
            {
                var person = new TrackablePerson();
                await ctx.SqlMapper.CreateAsync(ctx.Connection, person);

                var count = await ctx.SqlMapper.RemoveAsync(ctx.Connection, 1);
                Assert.Equal(1, count);
            }
        }

        [Fact]
        public async Task Test_SqlMapper_SavePoco()
        {
            using (var ctx = await PrepareAsync<IPerson>())
            {
                var person = new TrackablePerson();
                person.Id = 1;
                person.Name = "";
                await ctx.SqlMapper.CreateAsync(ctx.Connection, person);

                person.SetDefaultTracker();
                person.Name = "Testor";
                person.Age = 10;
                await ctx.SqlMapper.SaveAsync(ctx.Connection, person.Tracker, person.Id);

                var person2 = await ctx.SqlMapper.LoadAsync(ctx.Connection, person.Id);
                Assert.Equal(person.Id, person2.Id);
                Assert.Equal(person.Name, person2.Name);
                Assert.Equal(person.Age, person2.Age);
            }
        }

        [Fact]
        public async Task Test_SqlMapper_CreateAndLoadPoco_WithIdentity()
        {
            using (var ctx = await PrepareAsync<IPersonWithIdentity>())
            {
                var person = new TrackablePersonWithIdentity();
                person.Name = "Testor";
                person.Age = 10;

                await ctx.SqlMapper.CreateAsync(ctx.Connection, person);
                Assert.Equal(1, person.Id);
            }
        }
    }
}
