using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TrackableData.TestKits;
using Xunit;

namespace TrackableData.Redis.Tests
{
    public class TrackableListStringTest : StorageListValueTestKit, IClassFixture<Redis>
    {
        private static TrackableListRedisMapper<string> _mapper =
            new TrackableListRedisMapper<string>();

        private IDatabase _db;
        private string _testId = "TestList";

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

    /*
    public class TrackableListDataTest : StorageListDataTestKit, IClassFixture<Database>
    {
        private static TrackableListRedisMapper<JobData> _mapper =
            new TrackableListRedisMapper<JobData>();

        private Database _db;
        private IMongoCollection<BsonDocument> _collection;
        private ObjectId _testId = ObjectId.GenerateNewId();

        public TrackableListDataTest(Database db)
        {
            _db = db;
            _db.Test.DropCollectionAsync(nameof(TrackableListDataTest)).Wait();
            _collection = _db.Test.GetCollection<BsonDocument>(nameof(TrackableListDataTest));
        }

        protected override Task CreateAsync(IList<JobData> list)
        {
            return _mapper.CreateAsync(_collection, list, _testId, "V");
        }

        protected override Task<int> DeleteAsync()
        {
            return _mapper.DeleteAsync(_collection, _testId, "V");
        }

        protected override Task<TrackableList<JobData>> LoadAsync()
        {
            return _mapper.LoadAsync(_collection, _testId, "V");
        }

        protected override Task SaveAsync(ITracker tracker)
        {
            return _mapper.SaveAsync(_collection, (TrackableListTracker<JobData>)tracker, _testId, "V");
        }
    }
    */
}
