using System.Collections.Generic;
using System.Threading.Tasks;
using StackExchange.Redis;
using TrackableData.TestKits;
using Xunit;

namespace TrackableData.Redis.Tests
{
    public class TrackableListStringTest : StorageListValueTestKit, IClassFixture<Redis>
    {
        private static TrackableListRedisMapper<string> _mapper =
            new TrackableListRedisMapper<string>();

        private IDatabase _db;
        private string _testId = "TestListString";

        public TrackableListStringTest(Redis redis)
        {
            _db = redis.Db;
            _db.KeyDelete(_testId);
        }

        protected override Task CreateAsync(IList<string> list)
        {
            return _mapper.CreateAsync(_db, list, _testId);
        }

        protected override Task<int> DeleteAsync()
        {
            return _mapper.DeleteAsync(_db, _testId);
        }

        protected override Task<TrackableList<string>> LoadAsync()
        {
            return _mapper.LoadAsync(_db, _testId);
        }

        protected override Task SaveAsync(TrackableList<string> list)
        {
            return _mapper.SaveAsync(_db, list.Tracker, _testId);
        }
    }

    public class TrackableListDataTest : StorageListDataTestKit, IClassFixture<Redis>
    {
        private static TrackableListRedisMapper<JobData> _mapper =
            new TrackableListRedisMapper<JobData>();

        private IDatabase _db;
        private string _testId = "TrackableListData";

        public TrackableListDataTest(Redis redis)
        {
            _db = redis.Db;
            _db.KeyDelete(_testId);
        }

        protected override Task CreateAsync(IList<JobData> list)
        {
            return _mapper.CreateAsync(_db, list, _testId);
        }

        protected override Task<int> DeleteAsync()
        {
            return _mapper.DeleteAsync(_db, _testId);
        }

        protected override Task<TrackableList<JobData>> LoadAsync()
        {
            return _mapper.LoadAsync(_db, _testId);
        }

        protected override Task SaveAsync(TrackableList<JobData> list)
        {
            return _mapper.SaveAsync(_db, list.Tracker, _testId);
        }
    }
}
