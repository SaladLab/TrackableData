using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using TrackableData.TestKits;
using Xunit;

namespace TrackableData.MongoDB.Tests
{
    public class TrackableSetValueTest : StorageSetValueTestKit, IClassFixture<Database>
    {
        private static TrackableSetMongoDbMapper<int> _mapper =
            new TrackableSetMongoDbMapper<int>();

        private Database _db;
        private IMongoCollection<BsonDocument> _collection;
        private ObjectId _testId = ObjectId.GenerateNewId();

        public TrackableSetValueTest(Database db)
            : base(useDuplicateCheck: true)
        {
            _db = db;
            _db.Test.DropCollectionAsync(nameof(TrackableSetValueTest)).Wait();
            _collection = _db.Test.GetCollection<BsonDocument>(nameof(TrackableSetValueTest));
        }

        protected override Task CreateAsync(ICollection<int> set)
        {
            return _mapper.CreateAsync(_collection, set, _testId, "V");
        }

        protected override Task<int> DeleteAsync()
        {
            return _mapper.DeleteAsync(_collection, _testId, "V");
        }

        protected override Task<TrackableSet<int>> LoadAsync()
        {
            return _mapper.LoadAsync(_collection, _testId, "V");
        }

        protected override Task SaveAsync(TrackableSet<int> set)
        {
            return _mapper.SaveAsync(_collection, set.Tracker, _testId, "V");
        }
    }

    public class TrackableSetValueWithHeadKeysTest : StorageSetValueTestKit, IClassFixture<Database>
    {
        private static TrackableSetMongoDbMapper<int> _mapper =
            new TrackableSetMongoDbMapper<int>();

        private Database _db;
        private IMongoCollection<BsonDocument> _collection;
        private ObjectId _testId = ObjectId.GenerateNewId();

        public TrackableSetValueWithHeadKeysTest(Database db)
        {
            _db = db;
            _db.Test.DropCollectionAsync(nameof(TrackableSetValueWithHeadKeysTest)).Wait();
            _collection = _db.Test.GetCollection<BsonDocument>(nameof(TrackableSetValueWithHeadKeysTest));
        }

        protected override Task CreateAsync(ICollection<int> set)
        {
            return _mapper.CreateAsync(_collection, set, _testId, 1, "V");
        }

        protected override Task<int> DeleteAsync()
        {
            return _mapper.DeleteAsync(_collection, _testId, 1, "V");
        }

        protected override Task<TrackableSet<int>> LoadAsync()
        {
            return _mapper.LoadAsync(_collection, _testId, 1, "V");
        }

        protected override Task SaveAsync(TrackableSet<int> set)
        {
            return _mapper.SaveAsync(_collection, set.Tracker, _testId, 1, "V");
        }
    }
}
