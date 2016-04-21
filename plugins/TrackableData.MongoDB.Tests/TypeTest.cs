using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using TrackableData.TestKits;
using Xunit;

namespace TrackableData.MongoDB.Tests
{
    public class TypeTest : StorageTypeTestKit, IClassFixture<Database>
    {
        private static TrackablePocoMongoDbMapper<ITypeTestPoco> _mapper =
            new TrackablePocoMongoDbMapper<ITypeTestPoco>();

        private Database _db;
        private IMongoCollection<BsonDocument> _collection;

        public TypeTest(Database db)
        {
            _db = db;
            _db.Test.DropCollectionAsync(nameof(TypeTest)).Wait();
            _collection = _db.Test.GetCollection<BsonDocument>(nameof(TypeTest));
        }

        protected override Task CreateAsync(TrackableTypeTestPoco data)
        {
            return _mapper.CreateAsync(_collection, data);
        }

        protected override async Task<TrackableTypeTestPoco> LoadAsync(int id)
        {
            return (TrackableTypeTestPoco)(await _mapper.LoadAsync(_collection, id));
        }

        protected override Task SaveAsync(TrackableTypeTestPoco data)
        {
            return _mapper.SaveAsync(_collection, data.Tracker, data.Id);
        }
    }

    public class TypeNullableTest : StorageTypeNullableTestKit, IClassFixture<Database>
    {
        private static TrackablePocoMongoDbMapper<ITypeNullableTestPoco> _mapper =
            new TrackablePocoMongoDbMapper<ITypeNullableTestPoco>();

        private Database _db;
        private IMongoCollection<BsonDocument> _collection;

        public TypeNullableTest(Database db)
        {
            _db = db;
            _db.Test.DropCollectionAsync(nameof(TypeNullableTest)).Wait();
            _collection = _db.Test.GetCollection<BsonDocument>(nameof(TypeNullableTest));
        }

        protected override Task CreateAsync(TrackableTypeNullableTestPoco data)
        {
            return _mapper.CreateAsync(_collection, data);
        }

        protected override async Task<TrackableTypeNullableTestPoco> LoadAsync(int id)
        {
            return (TrackableTypeNullableTestPoco)(await _mapper.LoadAsync(_collection, id));
        }

        protected override Task SaveAsync(TrackableTypeNullableTestPoco data)
        {
            return _mapper.SaveAsync(_collection, data.Tracker, data.Id);
        }
    }
}
