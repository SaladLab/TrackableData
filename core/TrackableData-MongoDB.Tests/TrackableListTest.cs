using System.Threading.Tasks;
using MongoDB.Bson;
using Xunit;
using System.Collections.Generic;

namespace TrackableData.MongoDB.Tests
{
    public class TrackableListTest : IClassFixture<Database>
    {
        private static TrackableListMongoDbMapper<string> StringListMapper =
            new TrackableListMongoDbMapper<string>();

        private Database _db;

        public TrackableListTest(Database db)
        {
            _db = db;
        }

        private TrackableList<string> CreateTestList(bool withTracker)
        {
            var list = new TrackableList<string>();
            if (withTracker)
                list.SetDefaultTracker();
            list.Add("One");
            list.Add("Two");
            list.Add("Three");
            return list;
        }

        private void ModifyListForTest(IList<string> list)
        {
            list[0] = "OneModified";
            list.Insert(0, "Zero");
            list.RemoveAt(0);
            list.Insert(0, "ZeroAgain");
            list.Insert(4, "Four");
            list.RemoveAt(4);
            list.Insert(4, "FourAgain");
        }

        private List<string> GetModifiedList()
        {
            var list = new List<string>(CreateTestList(false));
            ModifyListForTest(list);
            return list;
        }

        // Regular Test

        [Fact]
        public async Task Test_MongoDbMapper_CreateAndLoad()
        {
            var collection = _db.Test.GetCollection<BsonDocument>("Trackable");

            var id = ObjectId.GenerateNewId();
            var list = CreateTestList(true);
            await StringListMapper.SaveAsync(collection, list.Tracker, id, "V");

            var list2 = await StringListMapper.LoadAsync(collection, id, "V");
            Assert.Equal(list, list2);
        }

        [Fact]
        public async Task Test_MongoDbMapper_Update()
        {
            var collection = _db.Test.GetCollection<BsonDocument>("Trackable");

            var id = ObjectId.GenerateNewId();
            var list = CreateTestList(true);
            await StringListMapper.SaveAsync(collection, list.Tracker, id, "V");
            list.Tracker.Clear();

            ModifyListForTest(list);
            await StringListMapper.SaveAsync(collection, list.Tracker, id, "V");

            var list2 = await StringListMapper.LoadAsync(collection, id, "V");
            Assert.Equal(list, list2);
        }
    }
}
