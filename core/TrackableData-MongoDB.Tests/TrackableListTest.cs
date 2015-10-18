using System;
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

        public TrackableListStringTest(Database db)
        {
            _db = db;
            _db.Test.DropCollectionAsync(nameof(String)).Wait();
            _collection = _db.Test.GetCollection<BsonDocument>(nameof(String));
        }

        protected override Task<TrackableList<string>> LoadAsync()
        {
            return _mapper.LoadAsync(_collection, 1, "V");
        }

        protected override Task SaveAsync(ITracker tracker)
        {
            return _mapper.SaveAsync(_collection, (TrackableListTracker<string>)tracker, 1, "V");
        }
    }

    public class TrackableListDataTest : StorageListDataTestKit, IClassFixture<Database>
    {
        private static TrackableListMongoDbMapper<JobData> _mapper =
            new TrackableListMongoDbMapper<JobData>();

        private Database _db;
        private IMongoCollection<BsonDocument> _collection;

        public TrackableListDataTest(Database db)
        {
            _db = db;
            _db.Test.DropCollectionAsync(nameof(JobData)).Wait();
            _collection = _db.Test.GetCollection<BsonDocument>(nameof(JobData));
        }

        protected override Task<TrackableList<JobData>> LoadAsync()
        {
            return _mapper.LoadAsync(_collection, 1, "V");
        }

        protected override Task SaveAsync(ITracker tracker)
        {
            return _mapper.SaveAsync(_collection, (TrackableListTracker<JobData>)tracker, 1, "V");
        }
    }

    public class TrackableListDataWithHeadKeysTest : StorageListDataTestKit, IClassFixture<Database>
    {
        private static TrackableListMongoDbMapper<JobData> _mapper =
            new TrackableListMongoDbMapper<JobData>();

        private Database _db;
        private IMongoCollection<BsonDocument> _collection;

        public TrackableListDataWithHeadKeysTest(Database db)
        {
            _db = db;
            _db.Test.DropCollectionAsync(nameof(JobData)).Wait();
            _collection = _db.Test.GetCollection<BsonDocument>(nameof(JobData));
        }

        protected override Task<TrackableList<JobData>> LoadAsync()
        {
            return _mapper.LoadAsync(_collection, 1, "One", "V");
        }

        protected override Task SaveAsync(ITracker tracker)
        {
            return _mapper.SaveAsync(_collection, (TrackableListTracker<JobData>)tracker, 1, "One", "V");
        }
    }

    public interface IJob : ITrackablePoco
    {
        short Kind { get; set; }
        int Count { get; set; }
        string Note { get; set; }
    }

    public class TrackableListPocoTest : StorageListPocoTestKit<TrackableJob>, IClassFixture<Database>
    {
        private static TrackableListMongoDbMapper<TrackableJob> _mapper =
            new TrackableListMongoDbMapper<TrackableJob>();

        private Database _db;
        private IMongoCollection<BsonDocument> _collection;

        public TrackableListPocoTest(Database db)
        {
            _db = db;
            _db.Test.DropCollectionAsync(nameof(TrackableJob)).Wait();
            _collection = _db.Test.GetCollection<BsonDocument>(nameof(TrackableJob));
        }

        protected override Task<TrackableList<TrackableJob>> LoadAsync()
        {
            return _mapper.LoadAsync(_collection, 1, "V");
        }

        protected override Task SaveAsync(TrackableList<TrackableJob> list)
        {
            return _mapper.SaveAsync(_collection, list, 1, "V");
        }
    }
}
