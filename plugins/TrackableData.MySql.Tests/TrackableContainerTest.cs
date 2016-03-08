using MySql.Data.MySqlClient;
using System;
using System.Threading.Tasks;
using TrackableData.Sql;
using TrackableData.TestKits;
using Xunit;

namespace TrackableData.MySql.Tests
{
    public interface ITestPocoForContainer : ITrackablePoco<ITestPocoForContainer>
    {
        string Name { get; set; }
        int Age { get; set; }
        int Extra { get; set; }
    }

    public interface ITestContainer : ITrackableContainer<ITestContainer>
    {
        TrackableTestPocoForContainer Person { get; set; }
        TrackableDictionary<int, MissionData> Missions { get; set; }
    }

    public class TrackableContainerTest :
        StorageContainerTestKit<TrackableTestContainer, TrackableTestPocoForContainer>,
        IClassFixture<Database>
    {
        private static readonly ColumnDefinition[] HeadKeyColumnDefs =
        {
            new ColumnDefinition("ContainerId", typeof(int)),
        };

        private static TrackableContainerSqlMapper<ITestContainer> _mapper =
            new TrackableContainerSqlMapper<ITestContainer>(
                MySqlProvider.Instance,
                new[]
                {
                    Tuple.Create("Person", new object[]
                    {
                        "TrackableContainerPerson",
                        HeadKeyColumnDefs
                    }),
                    Tuple.Create("Missions", new object[]
                    {
                        "TrackableContainerMission",
                        new ColumnDefinition("MissionId"),
                        HeadKeyColumnDefs,
                    }),
                });

        private Database _db;
        private MySqlConnection _connection;
        private int _testId = 1;

        public TrackableContainerTest(Database db)
            : base(false)
        {
            _db = db;
            _connection = db.Connection;
            _mapper.ResetTableAsync(_connection).Wait();
        }

        protected override Task CreateAsync(TrackableTestContainer container)
        {
            return _mapper.CreateAsync(_connection, container, _testId);
        }

        protected override Task<int> DeleteAsync()
        {
            return _mapper.DeleteAsync(_connection, _testId);
        }

        protected override async Task<TrackableTestContainer> LoadAsync()
        {
            return (TrackableTestContainer)await _mapper.LoadAsync(_connection, _testId);
        }

        protected override Task SaveAsync(IContainerTracker tracker)
        {
            return _mapper.SaveAsync(_connection, (TrackableTestContainerTracker)tracker, _testId);
        }
    }
}
