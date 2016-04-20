using System;
using System.Threading.Tasks;
using TrackableData.Sql;
using TrackableData.TestKits;
using Xunit;

namespace TrackableData.SqlTestKits
{
    public abstract class TrackableContainerIgnoreTest
    {
        private IDbConnectionProvider _db;
        private TrackableContainerSqlMapper<ITestContainerWithIgnore> _mapper;

        public TrackableContainerIgnoreTest(IDbConnectionProvider dbConnectionProvider, ISqlProvider sqlProvider)
        {
            _db = dbConnectionProvider;
            _mapper = new TrackableContainerSqlMapper<ITestContainerWithIgnore>(
                sqlProvider,
                new[]
                {
                    Tuple.Create("Person", new object[]
                    {
                        "TrackableContainerIgnoreTestPerson",
                        new[] { new ColumnDefinition("ContainerId", typeof(int)) }
                    }),
                });
            _mapper.ResetTableAsync(_db.Connection, true).Wait();
        }

        [Fact]
        public async Task Test_CreateAndLoad_CheckIgnored()
        {
            var id = 1;
            var c0 = new TrackableTestContainerWithIgnore();
            c0.Person.Name = "Testor";
            c0.Person.Age = 10;
            c0.Missions[1] = new MissionData { Kind = 101, Count = 20, Note = "Ignored" };
            await _mapper.CreateAsync(_db.Connection, c0, id);

            var c1 = await _mapper.LoadAsync(_db.Connection, id);
            Assert.NotNull(c1.Person);
            Assert.Equal(c0.Person.Name, c1.Person.Name);
            Assert.Equal(c0.Person.Age, c1.Person.Age);
            Assert.Equal(0, c1.Missions.Count);
        }

        [Fact]
        public async Task Test_SaveAndLoad_CheckIgnored()
        {
            var id = 2;
            var c0 = new TrackableTestContainerWithIgnore();
            await _mapper.CreateAsync(_db.Connection, c0, id);

            ((ITrackable)c0).SetDefaultTracker();
            c0.Person.Name = "Testor";
            c0.Person.Age = 10;
            c0.Missions[1] = new MissionData { Kind = 101, Count = 20, Note = "Ignored" };
            await _mapper.SaveAsync(_db.Connection, c0.Tracker, id);

            var c1 = await _mapper.LoadAsync(_db.Connection, id);
            Assert.NotNull(c1.Person);
            Assert.Equal(c0.Person.Name, c1.Person.Name);
            Assert.Equal(c0.Person.Age, c1.Person.Age);
            Assert.Equal(0, c1.Missions.Count);
        }
    }
}
