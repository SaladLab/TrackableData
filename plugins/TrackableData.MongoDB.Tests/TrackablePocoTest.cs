using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Xunit;

namespace TrackableData.MongoDB.Tests
{
    public interface ITestPoco : ITrackablePoco<ITestPoco>
    {
        ObjectId Id { get; set; }
        string Name { get; set; }
        int Age { get; set; }
        int Extra { get; set; }
    }

    public interface ITestPocoWithCustomId : ITrackablePoco<ITestPocoWithCustomId>
    {
        [TrackableProperty("mongodb.identity")] long CustomId { get; set; }
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
            _db.Test.DropCollectionAsync(nameof(TrackablePocoTest)).Wait();
            _collection = _db.Test.GetCollection<BsonDocument>(nameof(TrackablePocoTest));
        }

        protected override Task CreateAsync(TrackableTestPoco person)
        {
            return _mapper.CreateAsync(_collection, person);
        }

        protected override async Task<TrackableTestPoco> LoadAsync(ObjectId id)
        {
            return (TrackableTestPoco)(await _mapper.LoadAsync(_collection, id));
        }

        protected override Task<int> DeleteAsync(ObjectId id)
        {
            return _mapper.DeleteAsync(_collection, id);
        }

        protected override Task SaveAsync(TrackableTestPoco person, ObjectId id)
        {
            return _mapper.SaveAsync(_collection, person.Tracker, id);
        }
    }

    public class TrackablePocoWithHeadKeysTest : TestKits.StoragePocoTestKit<TrackableTestPoco, ObjectId>,
        IClassFixture<Database>
    {
        private static TrackablePocoMongoDbMapper<ITestPoco> _mapper =
            new TrackablePocoMongoDbMapper<ITestPoco>();

        private Database _db;
        private IMongoCollection<BsonDocument> _collection;
        private ObjectId _testId = ObjectId.GenerateNewId();

        public TrackablePocoWithHeadKeysTest(Database db)
        {
            _db = db;
            _db.Test.DropCollectionAsync(nameof(TrackablePocoWithHeadKeysTest)).Wait();
            _collection = _db.Test.GetCollection<BsonDocument>(nameof(TrackablePocoWithHeadKeysTest));
        }

        protected override Task CreateAsync(TrackableTestPoco person)
        {
            return _mapper.CreateAsync(_collection, person, _testId, "One");
        }

        protected override async Task<TrackableTestPoco> LoadAsync(ObjectId id)
        {
            return (TrackableTestPoco)(await _mapper.LoadAsync(_collection, _testId, "One", id));
        }

        protected override Task<int> DeleteAsync(ObjectId id)
        {
            return _mapper.DeleteAsync(_collection, _testId, "One", id);
        }

        protected override Task SaveAsync(TrackableTestPoco person, ObjectId id)
        {
            return _mapper.SaveAsync(_collection, person.Tracker, _testId, "One", id);
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
            _db.Test.DropCollectionAsync(nameof(TrackablePocoWithCustomIdTest)).Wait();
            _collection = _db.Test.GetCollection<BsonDocument>(nameof(TrackablePocoWithCustomIdTest));
        }

        [Fact]
        public async Task Test_MongoDbMapperWithCustomKey_CreateAndLoadPoco()
        {
            var person = new TrackableTestPocoWithCustomId
            {
                CustomId = UniqueInt64Id.GenerateNewId(),
                Name = "Testor",
                Age = 25
            };
            await _mapper.CreateAsync(_collection, person);

            var person2 = await _mapper.LoadAsync(_collection, person.CustomId);
            Assert.Equal(person.CustomId, person2.CustomId);
            Assert.Equal(person.Name, person2.Name);
            Assert.Equal(person.Age, person2.Age);
        }
    }
}
