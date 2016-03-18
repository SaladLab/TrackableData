using System;
using System.Threading.Tasks;
using TrackableData.TestKits;
using Xunit;
using System.Collections.Generic;
using StackExchange.Redis;

namespace TrackableData.Redis.Tests
{
    public class TrackableDictionaryStringTest : StorageDictionaryValueTestKit<int>, IClassFixture<Redis>
    {
        private static TrackableDictionaryRedisMapper<int, string> _mapper =
            new TrackableDictionaryRedisMapper<int, string>();

        private IDatabase _db;
        private string _testId = "TrackableDictionaryString";

        public TrackableDictionaryStringTest(Redis redis)
        {
            _db = redis.Db;
            _db.KeyDelete(_testId);
        }

        protected override int CreateKey(int value)
        {
            return value;
        }

        protected override Task CreateAsync(IDictionary<int, string> dictionary)
        {
            return _mapper.CreateAsync(_db, dictionary, _testId);
        }

        protected override Task<int> DeleteAsync()
        {
            return _mapper.DeleteAsync(_db, _testId);
        }

        protected override Task<TrackableDictionary<int, string>> LoadAsync()
        {
            return _mapper.LoadAsync(_db, _testId);
        }

        protected override Task SaveAsync(TrackableDictionary<int, string> dictionary)
        {
            return _mapper.SaveAsync(_db, dictionary.Tracker, _testId);
        }
    }

    public class TrackableDictionaryDataTest : StorageDictionaryDataTestKit<int>, IClassFixture<Redis>
    {
        private static TrackableDictionaryRedisMapper<int, ItemData> _mapper =
            new TrackableDictionaryRedisMapper<int, ItemData>();

        private IDatabase _db;
        private string _testId = "TrackableDictionaryData";

        public TrackableDictionaryDataTest(Redis redis)
        {
            _db = redis.Db;
            _db.KeyDelete(_testId);
        }

        protected override int CreateKey(int value)
        {
            return value;
        }

        protected override Task CreateAsync(IDictionary<int, ItemData> dictionary)
        {
            return _mapper.CreateAsync(_db, dictionary, _testId);
        }

        protected override Task<int> DeleteAsync()
        {
            return _mapper.DeleteAsync(_db, _testId);
        }

        protected override Task<TrackableDictionary<int, ItemData>> LoadAsync()
        {
            return _mapper.LoadAsync(_db, _testId);
        }

        protected override Task SaveAsync(TrackableDictionary<int, ItemData> dictionary)
        {
            return _mapper.SaveAsync(_db, dictionary.Tracker, _testId);
        }
    }
}
