using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using TrackableData.TestKits;
using Xunit;

namespace TrackableData.MongoDB.Tests
{
    public class TrackableListStringTest : StorageListValueTestKit, IClassFixture<Database>
    {
        private static TrackableListMongoDbMapper<string> _mapper =
            new TrackableListMongoDbMapper<string>();

        private Database _db;
        private IMongoCollection<BsonDocument> _collection;
        private ObjectId _testId = ObjectId.GenerateNewId();

        public TrackableListStringTest(Database db)
        {
            _db = db;
            _db.Test.DropCollectionAsync(nameof(TrackableListStringTest)).Wait();
            _collection = _db.Test.GetCollection<BsonDocument>(nameof(TrackableListStringTest));
        }

        protected override Task CreateAsync(IList<string> list)
        {
            return _mapper.CreateAsync(_collection, list, _testId, "V");
        }

        protected override Task<int> DeleteAsync()
        {
            return _mapper.DeleteAsync(_collection, _testId, "V");
        }

        protected override Task<TrackableList<string>> LoadAsync()
        {
            return _mapper.LoadAsync(_collection, _testId, "V");
        }

        protected override Task SaveAsync(TrackableList<string> list)
        {
            return _mapper.SaveAsync(_collection, list.Tracker, _testId, "V");
        }
    }

    public class TrackableListDataTest : StorageListDataTestKit, IClassFixture<Database>
    {
        private static TrackableListMongoDbMapper<JobData> _mapper =
            new TrackableListMongoDbMapper<JobData>();

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

        protected override Task SaveAsync(TrackableList<JobData> list)
        {
            return _mapper.SaveAsync(_collection, list.Tracker, _testId, "V");
        }
    }

    public class TrackableListDataWithHeadKeysTest : StorageListDataTestKit, IClassFixture<Database>
    {
        private static TrackableListMongoDbMapper<JobData> _mapper =
            new TrackableListMongoDbMapper<JobData>();

        private Database _db;
        private IMongoCollection<BsonDocument> _collection;
        private ObjectId _testId = ObjectId.GenerateNewId();

        public TrackableListDataWithHeadKeysTest(Database db)
        {
            _db = db;
            _db.Test.DropCollectionAsync(nameof(TrackableListDataWithHeadKeysTest)).Wait();
            _collection = _db.Test.GetCollection<BsonDocument>(nameof(TrackableListDataWithHeadKeysTest));
        }

        protected override Task CreateAsync(IList<JobData> list)
        {
            return _mapper.CreateAsync(_collection, list, _testId, 1, "One");
        }

        protected override Task<int> DeleteAsync()
        {
            return _mapper.DeleteAsync(_collection, _testId, 1, "One");
        }

        protected override Task<TrackableList<JobData>> LoadAsync()
        {
            return _mapper.LoadAsync(_collection, _testId, 1, "One");
        }

        protected override Task SaveAsync(TrackableList<JobData> list)
        {
            return _mapper.SaveAsync(_collection, list.Tracker, _testId, 1, "One");
        }
    }
}
