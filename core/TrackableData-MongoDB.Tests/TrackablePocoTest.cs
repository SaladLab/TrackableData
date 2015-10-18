using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Xunit;

namespace TrackableData.MongoDB.Tests
{
    public interface ITestPoco : ITrackablePoco
    {
        ObjectId Id { get; set; }
        string Name { get; set; }
        int Age { get; set; }
        int Extra { get; set; }
    }

    public interface ITestPocoWithCustomId : ITrackablePoco
    {
        [TrackableField("mongodb.identity")]
        long CustomId { get; set; }
        string Name { get; set; }
        int Age { get; set; }
        int Extra { get; set; }
    }

    public class TrackablePocoTest : TestKits.StoragePocoTestKit<TrackableTestPoco, ObjectId>, IClassFixture<Database>
    {
        private static TrackablePocoMongoDbMapper<ITestPoco> _mapper =
            new TrackablePocoMongoDbMapper<ITestPoco>();

        private Database _db;
        private IMongoCollection<BsonDocument> _collection;

        public TrackablePocoTest(Database db)
        {
            _db = db;
            _db.Test.DropCollectionAsync(nameof(ITestPoco)).Wait();
            _collection = _db.Test.GetCollection<BsonDocument>(nameof(ITestPoco));
        }

        protected override Task CreateAsync(TrackableTestPoco person)
        {
            return _mapper.CreateAsync(_collection, person);
        }

        protected override async Task<TrackableTestPoco> LoadAsync(ObjectId id)
        {
            return (TrackableTestPoco)(await _mapper.LoadAsync(_collection, id));
        }

        protected override Task<int> RemoveAsync(ObjectId id)
        {
            return _mapper.RemoveAsync(_collection, id);
        }

        protected override Task SaveAsync(ITracker tracker, ObjectId id)
        {
            return _mapper.SaveAsync(_collection, (TrackablePocoTracker<ITestPoco>)tracker, id);
        }
    }

    public class TrackablePocoWithHeadKeysTest : TestKits.StoragePocoTestKit<TrackableTestPoco, ObjectId>, IClassFixture<Database>
    {
        private static TrackablePocoMongoDbMapper<ITestPoco> _mapper =
                   new TrackablePocoMongoDbMapper<ITestPoco>();

        private Database _db;
        private IMongoCollection<BsonDocument> _collection;

        public TrackablePocoWithHeadKeysTest(Database db)
        {
            _db = db;
            _collection = _db.Test.GetCollection<BsonDocument>(nameof(ITestPoco));
        }

        protected override Task CreateAsync(TrackableTestPoco person)
        {
            return _mapper.CreateAsync(_collection, person, 1, "One");
        }

        protected override async Task<TrackableTestPoco> LoadAsync(ObjectId id)
        {
            return (TrackableTestPoco)(await _mapper.LoadAsync(_collection, 1, "One", id));
        }

        protected override Task<int> RemoveAsync(ObjectId id)
        {
            return _mapper.RemoveAsync(_collection, 1, "One", id);
        }

        protected override Task SaveAsync(ITracker tracker, ObjectId id)
        {
            return _mapper.SaveAsync(_collection, (TrackablePocoTracker<ITestPoco>)tracker, 1, "One", id);
        }
    }

    public class TrackablePocoWithCustomIdTest : IClassFixture<Database>
    {
        private static TrackablePocoMongoDbMapper<ITestPocoWithCustomId> _mapper =
            new TrackablePocoMongoDbMapper<ITestPocoWithCustomId>();

        private Database _db;
        private IMongoCollection<BsonDocument> _collection;

        public TrackablePocoWithCustomIdTest(Database db)
        {
            _db = db;
            _db.Test.DropCollectionAsync(nameof(ITestPocoWithCustomId)).Wait();
            _collection = _db.Test.GetCollection<BsonDocument>(nameof(ITestPocoWithCustomId));
        }

        [Fact]
        public async Task Test_MongoDbMapperWithCustomKey_CreateAndLoadPoco()
        {
            var collection = _db.Test.GetCollection<BsonDocument>("Trackable");

            var person = new TrackableTestPocoWithCustomId
            {
                CustomId = UniqueInt64Id.GenerateNewId(),
                Name = "Testor",
                Age = 25
            };
            await _mapper.CreateAsync(collection, person);

            var person2 = await _mapper.LoadAsync(collection, person.CustomId);
            Assert.Equal(person.CustomId, person2.CustomId);
            Assert.Equal(person.Name, person2.Name);
            Assert.Equal(person.Age, person2.Age);
        }
    }
}
