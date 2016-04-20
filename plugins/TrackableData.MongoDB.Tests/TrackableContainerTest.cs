using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using TrackableData.TestKits;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace TrackableData.MongoDB.Tests
{
    public interface ITestPocoForContainer : ITrackablePoco<ITestPocoForContainer>
    {
        string Name { get; set; }
        int Age { get; set; }
        int Extra { get; set; }
    }

    public interface ITestContainer : ITrackableContainer<ITestContainer>
    {
        TrackableTestPocoForContainer Person { get; set; }
        TrackableDictionary<int, MissionData> Missions { get; set; }
        TrackableList<TagData> Tags { get; set; }
    }

    public class TrackableContainerTest :
        StorageContainerTestKit<TrackableTestContainer, TrackableTestPocoForContainer>,
        IClassFixture<Database>
    {
        private static TrackableContainerMongoDbMapper<ITestContainer> _mapper =
            new TrackableContainerMongoDbMapper<ITestContainer>();

        private Database _db;
        private IMongoCollection<BsonDocument> _collection;
        private ObjectId _testId = ObjectId.GenerateNewId();

        public TrackableContainerTest(Database db)
            : base(true)
        {
            _db = db;
            _db.Test.DropCollectionAsync(nameof(TrackableContainerTest)).Wait();
            _collection = _db.Test.GetCollection<BsonDocument>(nameof(TrackableContainerTest));
        }

        protected override Task CreateAsync(TrackableTestContainer container)
        {
            return _mapper.CreateAsync(_collection, container, _testId);
        }

        protected override Task<int> DeleteAsync()
        {
            return _mapper.DeleteAsync(_collection, _testId);
        }

        protected override async Task<TrackableTestContainer> LoadAsync()
        {
            return (TrackableTestContainer)await _mapper.LoadAsync(_collection, _testId);
        }

        protected override Task SaveAsync(TrackableTestContainer container)
        {
            return _mapper.SaveAsync(_collection, container.Tracker, _testId);
        }

        protected override IEnumerable<ITrackable> GetTrackables(TrackableTestContainer person)
        {
            return _mapper.GetTrackables(person);
        }

        protected override IEnumerable<ITracker> GetTrackers(TrackableTestContainer person)
        {
            return _mapper.GetTrackers(person);
        }
    }

    public class TrackableContainerTestWithHeadKeysTest :
        StorageContainerTestKit<TrackableTestContainer, TrackableTestPocoForContainer>,
        IClassFixture<Database>
    {
        private static TrackableContainerMongoDbMapper<ITestContainer> _mapper =
            new TrackableContainerMongoDbMapper<ITestContainer>();

        private Database _db;
        private IMongoCollection<BsonDocument> _collection;
        private ObjectId _testId = ObjectId.GenerateNewId();

        public TrackableContainerTestWithHeadKeysTest(Database db)
            : base(true)
        {
            _db = db;
            _db.Test.DropCollectionAsync(nameof(TrackableContainerTestWithHeadKeysTest)).Wait();
            _collection = _db.Test.GetCollection<BsonDocument>(nameof(TrackableContainerTestWithHeadKeysTest));
            _collection.InsertOneAsync(new BsonDocument { { "_id", _testId } }).Wait();
        }

        protected override Task CreateAsync(TrackableTestContainer container)
        {
            return _mapper.CreateAsync(_collection, container, _testId, "One");
        }

        protected override Task<int> DeleteAsync()
        {
            return _mapper.DeleteAsync(_collection, _testId, "One");
        }

        protected override async Task<TrackableTestContainer> LoadAsync()
        {
            return (TrackableTestContainer)await _mapper.LoadAsync(_collection, _testId, "One");
        }

        protected override Task SaveAsync(TrackableTestContainer container)
        {
            return _mapper.SaveAsync(_collection, container.Tracker, _testId, "One");
        }

        protected override IEnumerable<ITrackable> GetTrackables(TrackableTestContainer person)
        {
            return _mapper.GetTrackables(person);
        }

        protected override IEnumerable<ITracker> GetTrackers(TrackableTestContainer person)
        {
            return _mapper.GetTrackers(person);
        }
    }
}
