using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;
using TrackableData.TestKits;
using Xunit;

namespace TrackableData.MongoDB.Tests
{
    public class TrackableDictionaryStringTest : StorageDictionaryStringTestKit<int>, IClassFixture<Database>
    {
        private static TrackableDictionaryMongoDbMapper<int, string> _mapper =
            new TrackableDictionaryMongoDbMapper<int, string>();

        private Database _db;
        private IMongoCollection<BsonDocument> _collection;

        public TrackableDictionaryStringTest(Database db)
        {
            _db = db;
            _db.Test.DropCollectionAsync(nameof(String)).Wait();
            _collection = _db.Test.GetCollection<BsonDocument>(nameof(String));
        }

        protected override int CreateKey(int value)
        {
            return value;
        }

        protected override Task<TrackableDictionary<int, string>> LoadAsync()
        {
            return _mapper.LoadAsync(_collection, 1);
        }

        protected override Task SaveAsync(ITracker tracker)
        {
            return _mapper.SaveAsync(_collection, (TrackableDictionaryTracker<int, string>)tracker, 1);
        }
    }

    public class TrackableDictionaryItemDataTest : StorageDictionaryItemDataTestKit<int>, IClassFixture<Database>
    {
        private static TrackableDictionaryMongoDbMapper<int, ItemData> _mapper =
            new TrackableDictionaryMongoDbMapper<int, ItemData>();

        private Database _db;
        private IMongoCollection<BsonDocument> _collection;

        public TrackableDictionaryItemDataTest(Database db)
        {
            _db = db;
            _db.Test.DropCollectionAsync(nameof(ItemData)).Wait();
            _collection = _db.Test.GetCollection<BsonDocument>(nameof(ItemData));
        }

        protected override int CreateKey(int value)
        {
            return value;
        }

        protected override Task<TrackableDictionary<int, ItemData>> LoadAsync()
        {
            return _mapper.LoadAsync(_collection, 1);
        }

        protected override Task SaveAsync(ITracker tracker)
        {
            return _mapper.SaveAsync(_collection, (TrackableDictionaryTracker<int, ItemData>)tracker, 1);
        }
    }

    public class TrackableDictionaryItemDataWithHeadKeysTest : StorageDictionaryItemDataTestKit<int>, IClassFixture<Database>
    {
        private static TrackableDictionaryMongoDbMapper<int, ItemData> _mapper =
            new TrackableDictionaryMongoDbMapper<int, ItemData>();

        private Database _db;
        private IMongoCollection<BsonDocument> _collection;

        public TrackableDictionaryItemDataWithHeadKeysTest(Database db)
        {
            _db = db;
            _db.Test.DropCollectionAsync(nameof(ItemData)).Wait();
            _collection = _db.Test.GetCollection<BsonDocument>(nameof(ItemData));
        }

        protected override int CreateKey(int value)
        {
            return value;
        }

        protected override Task<TrackableDictionary<int, ItemData>> LoadAsync()
        {
            return _mapper.LoadAsync(_collection, 1, "One");
        }

        protected override Task SaveAsync(ITracker tracker)
        {
            return _mapper.SaveAsync(_collection, (TrackableDictionaryTracker<int, ItemData>)tracker, 1, "One");
        }
    }

    /*
    public interface IItem : ITrackablePoco
    {
        short Kind { get; set; }
        int Count { get; set; }
        string Note { get; set; }
    }

    public class TrackableDictionaryItemPocoTest : StorageDictionaryItemPocoKit<int, TrackableItem>, IClassFixture<Database>
    {
        private static TrackableDictionaryMongoDbMapper<int, TrackableItem> _mapper =
            new TrackableDictionaryMongoDbMapper<int, TrackableItem>();

        private Database _db;
        private IMongoCollection<BsonDocument> _collection;

        public TrackableDictionaryItemPocoTest(Database db)
        {
            _db = db;
            _db.Test.DropCollectionAsync(nameof(ItemData)).Wait();
            _collection = _db.Test.GetCollection<BsonDocument>(nameof(ItemData));
        }

        protected override int CreateKey(int value)
        {
            return value;
        }

        protected override Task<TrackableDictionary<int, TrackableItem>> LoadAsync()
        {
            return _mapper.LoadAsync(_collection, 1);
        }

        protected override Task SaveAsync(TrackableDictionary<int, TrackableItem> dictionary)
        {
            return _mapper.SaveAsync(_collection, dictionary, 1);
        }
    }
    */
}
