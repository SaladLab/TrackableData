using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
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

        protected override Task CreateAsync(IDictionary<int, string> dictionary)
        {
            return _mapper.CreateAsync(_collection, dictionary, _testId);
        }

        protected override Task<int> DeleteAsync()
        {
            return _mapper.DeleteAsync(_collection, _testId);
        }

        protected override Task<TrackableDictionary<int, string>> LoadAsync()
        {
            return _mapper.LoadAsync(_collection, _testId);
        }

        protected override Task SaveAsync(TrackableDictionary<int, string> dictionary)
        {
            return _mapper.SaveAsync(_collection, dictionary.Tracker, _testId);
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

        protected override Task CreateAsync(IDictionary<int, ItemData> dictionary)
        {
            return _mapper.CreateAsync(_collection, dictionary, _testId);
        }

        protected override Task<int> DeleteAsync()
        {
            return _mapper.DeleteAsync(_collection, _testId);
        }

        protected override Task<TrackableDictionary<int, ItemData>> LoadAsync()
        {
            return _mapper.LoadAsync(_collection, _testId);
        }

        protected override Task SaveAsync(TrackableDictionary<int, ItemData> dictionary)
        {
            return _mapper.SaveAsync(_collection, dictionary.Tracker, _testId);
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

        protected override Task CreateAsync(IDictionary<int, ItemData> dictionary)
        {
            return _mapper.CreateAsync(_collection, dictionary, _testId, 1, "One");
        }

        protected override Task<int> DeleteAsync()
        {
            return _mapper.DeleteAsync(_collection, _testId, 1, "One");
        }

        protected override Task<TrackableDictionary<int, ItemData>> LoadAsync()
        {
            return _mapper.LoadAsync(_collection, _testId, 1, "One");
        }

        protected override Task SaveAsync(TrackableDictionary<int, ItemData> dictionary)
        {
            return _mapper.SaveAsync(_collection, dictionary.Tracker, _testId, 1, "One");
        }
    }
}
