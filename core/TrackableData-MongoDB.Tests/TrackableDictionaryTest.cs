using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;
using TrackableData.TestKits;
using Xunit;

namespace TrackableData.MongoDB.Tests
{
    public class TrackableDictionaryStringTest : StorageDictionaryValueTestKit<int>, IClassFixture<Database>
    {
        private static TrackableDictionaryMongoDbMapper<int, string> _mapper =
            new TrackableDictionaryMongoDbMapper<int, string>();

        private Database _db;
        private IMongoCollection<BsonDocument> _collection;
        private ObjectId _testId = ObjectId.GenerateNewId();

        public TrackableDictionaryStringTest(Database db)
        {
            _db = db;
            _db.Test.DropCollectionAsync(nameof(TrackableDictionaryStringTest)).Wait();
            _collection = _db.Test.GetCollection<BsonDocument>(nameof(TrackableDictionaryStringTest));
        }

        protected override int CreateKey(int value)
        {
            return value;
        }

        protected override Task<TrackableDictionary<int, string>> LoadAsync()
        {
            return _mapper.LoadAsync(_collection, _testId);
        }

        protected override Task SaveAsync(ITracker tracker)
        {
            return _mapper.SaveAsync(_collection, (TrackableDictionaryTracker<int, string>)tracker, _testId);
        }
    }

    public class TrackableDictionaryDataTest : StorageDictionaryDataTestKit<int>, IClassFixture<Database>
    {
        private static TrackableDictionaryMongoDbMapper<int, ItemData> _mapper =
            new TrackableDictionaryMongoDbMapper<int, ItemData>();

        private Database _db;
        private IMongoCollection<BsonDocument> _collection;
        private ObjectId _testId = ObjectId.GenerateNewId();

        public TrackableDictionaryDataTest(Database db)
        {
            _db = db;
            _db.Test.DropCollectionAsync(nameof(TrackableDictionaryDataTest)).Wait();
            _collection = _db.Test.GetCollection<BsonDocument>(nameof(TrackableDictionaryDataTest));
        }

        protected override int CreateKey(int value)
        {
            return value;
        }

        protected override Task<TrackableDictionary<int, ItemData>> LoadAsync()
        {
            return _mapper.LoadAsync(_collection, _testId);
        }

        protected override Task SaveAsync(ITracker tracker)
        {
            return _mapper.SaveAsync(_collection, (TrackableDictionaryTracker<int, ItemData>)tracker, _testId);
        }
    }

    public class TrackableDictionaryDataWithHeadKeysTest : StorageDictionaryDataTestKit<int>, IClassFixture<Database>
    {
        private static TrackableDictionaryMongoDbMapper<int, ItemData> _mapper =
            new TrackableDictionaryMongoDbMapper<int, ItemData>();
        
        private Database _db;
        private IMongoCollection<BsonDocument> _collection;
        private ObjectId _testId = ObjectId.GenerateNewId();

        public TrackableDictionaryDataWithHeadKeysTest(Database db)
        {
            _db = db;
            _db.Test.DropCollectionAsync(nameof(TrackableDictionaryDataWithHeadKeysTest)).Wait();
            _collection = _db.Test.GetCollection<BsonDocument>(nameof(TrackableDictionaryDataWithHeadKeysTest));
        }

        protected override int CreateKey(int value)
        {
            return value;
        }

        protected override Task<TrackableDictionary<int, ItemData>> LoadAsync()
        {
            return _mapper.LoadAsync(_collection, _testId, 1, "One");
        }

        protected override Task SaveAsync(ITracker tracker)
        {
            return _mapper.SaveAsync(_collection, (TrackableDictionaryTracker<int, ItemData>)tracker, _testId, 1, "One");
        }
    }

    public interface IItem : ITrackablePoco<IItem>
    {
        short Kind { get; set; }
        int Count { get; set; }
        string Note { get; set; }
    }

}
