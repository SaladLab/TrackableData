using System.Collections.Generic;
using System.Threading.Tasks;
using StackExchange.Redis;
using TrackableData.TestKits;
using Xunit;

namespace TrackableData.Redis.Tests
{
    public class TrackableSetTest : StorageSetValueTestKit, IClassFixture<Redis>
    {
        private static TrackableSetRedisMapper<int> _mapper =
            new TrackableSetRedisMapper<int>();

        private IDatabase _db;
        private string _testId = "TrackableSetTest";

        public TrackableSetTest(Redis redis)
        {
            _db = redis.Db;
            _db.KeyDelete(_testId);
        }

        protected override Task CreateAsync(ICollection<int> set)
        {
            return _mapper.CreateAsync(_db, set, _testId);
        }

        protected override Task<int> DeleteAsync()
        {
            return _mapper.DeleteAsync(_db, _testId);
        }

        protected override Task<TrackableSet<int>> LoadAsync()
        {
            return _mapper.LoadAsync(_db, _testId);
        }

        protected override Task SaveAsync(TrackableSet<int> set)
        {
            return _mapper.SaveAsync(_db, set.Tracker, _testId);
        }
    }
}
