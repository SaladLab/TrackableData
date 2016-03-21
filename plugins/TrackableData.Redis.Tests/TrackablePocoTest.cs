using System.Threading.Tasks;
using StackExchange.Redis;
using Xunit;

namespace TrackableData.Redis.Tests
{
    public interface ITestPoco : ITrackablePoco<ITestPoco>
    {
        int Id { get; set; }
        string Name { get; set; }
        int Age { get; set; }
        int Extra { get; set; }
    }

    public class TrackablePocoTest : TestKits.StoragePocoTestKit<TrackableTestPoco, int>, IClassFixture<Redis>
    {
        private static TrackablePocoRedisMapper<ITestPoco> _mapper =
            new TrackablePocoRedisMapper<ITestPoco>();

        private IDatabase _db;

        public TrackablePocoTest(Redis redis)
        {
            _db = redis.Db;
        }

        protected override Task CreateAsync(TrackableTestPoco person)
        {
            return _mapper.CreateAsync(_db, person, person.Id.ToString());
        }

        protected override async Task<TrackableTestPoco> LoadAsync(int id)
        {
            return (TrackableTestPoco)(await _mapper.LoadAsync(_db, id.ToString()));
        }

        protected override Task<int> DeleteAsync(int id)
        {
            return _mapper.DeleteAsync(_db, id.ToString());
        }

        protected override Task SaveAsync(TrackableTestPoco person, int id)
        {
            return _mapper.SaveAsync(_db, person.Tracker, id.ToString());
        }
    }
}
