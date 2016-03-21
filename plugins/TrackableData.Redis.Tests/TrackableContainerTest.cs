using System.Threading.Tasks;
using StackExchange.Redis;
using TrackableData.TestKits;
using Xunit;

namespace TrackableData.Redis.Tests
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
        TrackableList<TagData> Tags { get; set; }
    }

    public class TrackableContainerTest :
        StorageContainerTestKit<TrackableTestContainer, TrackableTestPocoForContainer>,
        IClassFixture<Redis>
    {
        private static TrackableContainerRedisMapper<ITestContainer> _mapper =
            new TrackableContainerRedisMapper<ITestContainer>();

        private IDatabase _db;
        private string _testId = "TestContainer";

        public TrackableContainerTest(Redis redis)
            : base(true)
        {
            _db = redis.Db;
        }

        protected override Task CreateAsync(TrackableTestContainer container)
        {
            return _mapper.CreateAsync(_db, container, _testId);
        }

        protected override Task<int> DeleteAsync()
        {
            return _mapper.DeleteAsync(_db, _testId);
        }

        protected override async Task<TrackableTestContainer> LoadAsync()
        {
            return (TrackableTestContainer)await _mapper.LoadAsync(_db, _testId);
        }

        protected override Task SaveAsync(TrackableTestContainer container)
        {
            return _mapper.SaveAsync(_db, container.Tracker, _testId);
        }
    }
}
