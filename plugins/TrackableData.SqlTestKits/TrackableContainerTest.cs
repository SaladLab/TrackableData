using System;
using System.Threading.Tasks;
using TrackableData.Sql;
using TrackableData.TestKits;

namespace TrackableData.SqlTestKits
{
    public abstract class TrackableContainerTest 
        : StorageContainerTestKit<TrackableTestContainer, TrackableTestPocoForContainer>
    {
        private IDbConnectionProvider _db;
        private TrackableContainerSqlMapper<ITestContainer> _mapper;
        private int _testId = 1;

        public TrackableContainerTest(IDbConnectionProvider dbConnectionProvider, ISqlProvider sqlProvider)
            : base(false)
        {
            _db = dbConnectionProvider;
            _mapper = new TrackableContainerSqlMapper<ITestContainer>(
                sqlProvider,
                new[]
                {
                    Tuple.Create("Person", new object[]
                    {
                        "TrackableContainerPerson",
                        new[] { new ColumnDefinition("ContainerId", typeof(int)) }
                    }),
                    Tuple.Create("Missions", new object[]
                    {
                        "TrackableContainerMission",
                        new ColumnDefinition("MissionId"),
                        new[] { new ColumnDefinition("ContainerId", typeof(int)) },
                    }),
                });
            _mapper.ResetTableAsync(_db.Connection, true).Wait();
        }

        protected override Task CreateAsync(TrackableTestContainer container)
        {
            return _mapper.CreateAsync(_db.Connection, container, _testId);
        }

        protected override Task<int> DeleteAsync()
        {
            return _mapper.DeleteAsync(_db.Connection, _testId);
        }

        protected override async Task<TrackableTestContainer> LoadAsync()
        {
            return (TrackableTestContainer)await _mapper.LoadAsync(_db.Connection, _testId);
        }

        protected override Task SaveAsync(IContainerTracker tracker)
        {
            return _mapper.SaveAsync(_db.Connection, (TrackableTestContainerTracker)tracker, _testId);
        }
    }
}
