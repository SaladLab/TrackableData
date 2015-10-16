using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Xunit;

namespace TrackableData.MongoDB.Tests
{
    public class TrackablePocoTest : IClassFixture<Database>
    {
        private static TrackablePocoMongoDbMapper<IPerson> PersonMapper =
            new TrackablePocoMongoDbMapper<IPerson>();

        private static TrackablePocoMongoDbMapper<IPersonWithCustomId> PersonWithCustomIdMapper =
            new TrackablePocoMongoDbMapper<IPersonWithCustomId>();

        private Database _db;

        public TrackablePocoTest(Database db)
        {
            _db = db;
        }

        // Regular Test

        [Fact]
        public async Task Test_MongoDbMapper_CreateAndLoadPoco()
        {
            var collection = _db.Test.GetCollection<BsonDocument>("Trackable");

            var person = new TrackablePerson
            {
                Id = ObjectId.GenerateNewId(),
                Name = "Testor",
                Age = 10
            };
            await PersonMapper.CreateAsync(collection, person);

            var person2 = await PersonMapper.LoadAsync(collection, person.Id);
            Assert.Equal(person.Id, person2.Id);
            Assert.Equal(person.Name, person2.Name);
            Assert.Equal(person.Age, person2.Age);
        }

        [Fact]
        public async Task Test_MongoDbMapper_DeletePoco()
        {
            var collection = _db.Test.GetCollection<BsonDocument>("Trackable");

            var person = new TrackablePerson
            {
                Id = ObjectId.GenerateNewId(),
                Name = "Testor",
                Age = 10
            };
            await PersonMapper.CreateAsync(collection, person);

            await PersonMapper.RemoveAsync(collection, person.Id);

            var person2 = await PersonMapper.LoadAsync(collection, person.Id);
            Assert.Equal(null, person2);
        }

        [Fact]
        public async Task Test_MongoDbMapper_SavePoco()
        {
            var collection = _db.Test.GetCollection<BsonDocument>("Trackable");

            var person = new TrackablePerson
            {
                Id = ObjectId.GenerateNewId(),
                Name = "Testor",
                Age = 10
            };
            await PersonMapper.CreateAsync(collection, person);

            person.SetDefaultTracker();
            person.Name = "SuperTestor";
            person.Age += 1;
            await PersonMapper.SaveAsync(collection, person.Tracker, person.Id);
            person.Tracker.Clear();

            var person2 = await PersonMapper.LoadAsync(collection, person.Id);
            Assert.Equal(person.Id, person2.Id);
            Assert.Equal(person.Name, person2.Name);
            Assert.Equal(person.Age, person2.Age);
        }

        // With Head Keys

        [Fact]
        public async Task Test_MongoDbMapperWithHead_CreateAndLoadPoco()
        {
            var collection = _db.Test.GetCollection<BsonDocument>("Trackable");

            var person = new TrackablePerson
            {
                Id = ObjectId.GenerateNewId(),
                Name = "Testor",
                Age = 25
            };
            await PersonMapper.CreateAsync(collection, person, 1, "One");

            var person2 = await PersonMapper.LoadAsync(collection, 1, "One", person.Id);
            Assert.Equal(person.Id, person2.Id);
            Assert.Equal(person.Name, person2.Name);
            Assert.Equal(person.Age, person2.Age);
        }

        [Fact]
        public async Task Test_MongoDbMapperWithHead_DeletePoco()
        {
            var collection = _db.Test.GetCollection<BsonDocument>("Trackable");

            var person = new TrackablePerson
            {
                Id = ObjectId.GenerateNewId(),
                Name = "Testor",
                Age = 10
            };
            await PersonMapper.CreateAsync(collection, person, 1, "One");

            await PersonMapper.RemoveAsync(collection, 1, "One", person.Id);

            var person2 = await PersonMapper.LoadAsync(collection, 1, "One", person.Id);
            Assert.Equal(null, person2);
        }

        [Fact]
        public async Task Test_MongoDbMapperWithHead_SavePoco()
        {
            var collection = _db.Test.GetCollection<BsonDocument>("Trackable");

            var person = new TrackablePerson
            {
                Id = ObjectId.GenerateNewId(),
                Name = "Testor",
                Age = 10
            };
            await PersonMapper.CreateAsync(collection, person, 1, "One");

            person.SetDefaultTracker();
            person.Name = "SuperTestor";
            person.Age += 1;
            await PersonMapper.SaveAsync(collection, person.Tracker, 1, "One", person.Id);
            person.Tracker.Clear();

            var person2 = await PersonMapper.LoadAsync(collection, 1, "One", person.Id);
            Assert.Equal(person.Id, person2.Id);
            Assert.Equal(person.Name, person2.Name);
            Assert.Equal(person.Age, person2.Age);
        }

        // With Custom Key

        [Fact]
        public async Task Test_MongoDbMapperWithCustomKey_CreateAndLoadPoco()
        {
            var collection = _db.Test.GetCollection<BsonDocument>("Trackable");

            var person = new TrackablePersonWithCustomId
            {
                CustomId = UniqueInt64Id.GenerateNewId(),
                Name = "Testor",
                Age = 25
            };
            await PersonWithCustomIdMapper.CreateAsync(collection, person);

            var person2 = await PersonWithCustomIdMapper.LoadAsync(collection, person.CustomId);
            Assert.Equal(person.CustomId, person2.CustomId);
            Assert.Equal(person.Name, person2.Name);
            Assert.Equal(person.Age, person2.Age);
        }

        // Workshop

        [Fact]
        public async Task Test_Workshop()
        {
            var collection = _db.Test.GetCollection<BsonDocument>("Trackable");

            var person = new TrackablePerson
            {
                Id = ObjectId.GenerateNewId(),
                Name = "Alice",
                Age = 10
            };
            var bson = person.ToBsonDocument();
            await collection.InsertOneAsync(bson);
            var id = bson["_id"];
            Assert.Equal(person.Id, id);

            await collection.UpdateOneAsync(
                Builders<BsonDocument>.Filter.Eq("_id", person.Id),
                Builders<BsonDocument>.Update.Set("1.def.ghi", 10));
        }
    }
}
