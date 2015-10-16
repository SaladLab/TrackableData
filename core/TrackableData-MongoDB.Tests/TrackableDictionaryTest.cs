using System.Threading.Tasks;
using MongoDB.Bson;
using Xunit;

namespace TrackableData.MongoDB.Tests
{
    public class TrackableDictionaryTest : IClassFixture<Database>
    {
        private static TrackableDictionaryMongoDbMapper<int, ItemData> ItemDataMapper =
            new TrackableDictionaryMongoDbMapper<int, ItemData>();

        private static TrackableDictionaryMongoDbMapper<int, string> StringMapper =
            new TrackableDictionaryMongoDbMapper<int, string>();

        private Database _db;

        public TrackableDictionaryTest(Database db)
        {
            _db = db;
        }

        private TrackableDictionary<int, ItemData> CreateTestInventory(bool withTracker)
        {
            var dict = new TrackableDictionary<int, ItemData>();
            if (withTracker)
                dict.SetDefaultTracker();
            dict.Add(1, new ItemData { Kind = 101, Count = 1, Note = "Handmade Sword" });
            dict.Add(2, new ItemData { Kind = 102, Count = 3, Note = "Lord of Ring" });
            return dict;
        }

        private enum ModificationWayType
        {
            Intrusive,
            IntrusiveAndMark,
            SetNew,
        }

        private void ModifyTestInventory(TrackableDictionary<int, ItemData> dict, ModificationWayType type)
        {
            dict.Remove(1);
            switch (type)
            {
                case ModificationWayType.Intrusive:
                    dict[2].Count -= 1;
                    dict[2].Note = "Destroyed";
                    break;

                case ModificationWayType.IntrusiveAndMark:
                    dict[2].Count -= 1;
                    dict[2].Note = "Destroyed";
                    dict.MarkModify(2);
                    break;

                case ModificationWayType.SetNew:
                    var item = dict[2];
                    dict[2] = new ItemData { Kind = item.Kind, Count = item.Count - 1, Note = "Destroyed" };
                    break;
            }
            dict.Add(3, new ItemData { Kind = 103, Count = 3, Note = "Just Arrived" });
        }

        // Regular Test

        [Fact]
        public async Task Test_MongoDbMapper_CreateAndLoad()
        {
            var collection = _db.Test.GetCollection<BsonDocument>("Trackable");

            var id = ObjectId.GenerateNewId();
            var dict = CreateTestInventory(true);
            await ItemDataMapper.SaveAsync(collection, dict.Tracker, id);

            var dict2 = await ItemDataMapper.LoadAsync(collection, id);
            Assert.Equal(dict.Count, dict2.Count);
            foreach (var item in dict)
            {
                Assert.Equal(item.Value.Kind, dict2[item.Key].Kind);
                Assert.Equal(item.Value.Count, dict2[item.Key].Count);
                Assert.Equal(item.Value.Note, dict2[item.Key].Note);
            }
        }

        [Fact]
        public async Task Test_MongoDbMapper_Update()
        {
            var collection = _db.Test.GetCollection<BsonDocument>("Trackable");

            var id = ObjectId.GenerateNewId();
            var dict = CreateTestInventory(true);
            await ItemDataMapper.SaveAsync(collection, dict.Tracker, id);
            dict.Tracker.Clear();

            ModifyTestInventory(dict, ModificationWayType.SetNew);
            await ItemDataMapper.SaveAsync(collection, dict.Tracker, id);

            var dict2 = await ItemDataMapper.LoadAsync(collection, id);
            Assert.Equal(dict.Count, dict2.Count);
            foreach (var item in dict)
            {
                Assert.Equal(item.Value.Kind, dict2[item.Key].Kind);
                Assert.Equal(item.Value.Count, dict2[item.Key].Count);
                Assert.Equal(item.Value.Note, dict2[item.Key].Note);
            }
        }

        // With Value

        [Fact]
        public async Task Test_MongoDbMapperWitValue_CreateAndLoad()
        {
            var collection = _db.Test.GetCollection<BsonDocument>("Trackable");

            var id = ObjectId.GenerateNewId();
            var dict = new TrackableDictionary<int, string>();
            dict.SetDefaultTracker();
            dict.Add(1, "One");
            dict.Add(2, "Two");
            dict.Add(3, "Three");

            await StringMapper.SaveAsync(collection, dict.Tracker, id);

            var dict2 = await StringMapper.LoadAsync(collection, id);
            Assert.Equal(dict.Count, dict2.Count);
            foreach (var item in dict)
            {
                Assert.Equal(item.Value, dict2[item.Key]);
            }
        }

        [Fact]
        public async Task Test_MongoDbMapperWithValue_Update()
        {
            var collection = _db.Test.GetCollection<BsonDocument>("Trackable");

            var id = ObjectId.GenerateNewId();
            var dict = new TrackableDictionary<int, string>();
            dict.SetDefaultTracker();
            dict.Add(1, "One");
            dict.Add(2, "Two");
            dict.Add(3, "Three");

            await StringMapper.SaveAsync(collection, dict.Tracker, id);
            dict.Tracker.Clear();

            dict.Remove(1);
            dict[2] = "TwoTwo";
            dict.Add(4, "Four");
            await StringMapper.SaveAsync(collection, dict.Tracker, id);

            var dict2 = await StringMapper.LoadAsync(collection, id);
            Assert.Equal(dict.Count, dict2.Count);
            foreach (var item in dict)
            {
                Assert.Equal(item.Value, dict2[item.Key]);
            }
        }
    }
}
