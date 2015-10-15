using Newtonsoft.Json;
using Xunit;

namespace TrackableData.Json.Tests
{
    public class ListTrackerTest
    {
        private TrackableList<string> CreateTestList()
        {
            return new TrackableList<string>()
            {
                "One",
                "Two",
                "Three"
            };
        }

        private TrackableList<string> CreateTestListWithTracker()
        {
            var list = CreateTestList();
            list.SetDefaultTrackerDeep();
            return list;
        }

        private JsonSerializerSettings GetJsonSerializerSettings()
        {
            return new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.None,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                Converters = new JsonConverter[]
                {
                    new TrackableListTrackerJsonConverter<string>()
                }
            };
        }

        [Fact]
        public void Test_List_Serialize_Work()
        {
            var list = CreateTestListWithTracker();
            var jsonSerializerSettings = GetJsonSerializerSettings();
            var json = JsonConvert.SerializeObject(list, jsonSerializerSettings);
            var list2 = JsonConvert.DeserializeObject<TrackableList<string>>(json, jsonSerializerSettings);
            Assert.Equal(list.Count, list2.Count);
        }

        [Fact]
        public void Test_ListTracker_Serialize_Work()
        {
            var list = CreateTestListWithTracker();
            list[0] = "OneModified";
            list.RemoveAt(1);
            list.Insert(1, "TwoInserted");

            var jsonSerializerSettings = GetJsonSerializerSettings();
            var json = JsonConvert.SerializeObject(list.Tracker, jsonSerializerSettings);
            var tracker2 = JsonConvert.DeserializeObject<TrackableListTracker<string>>(json, jsonSerializerSettings);

            var list2 = CreateTestList();
            tracker2.ApplyTo(list2);

            Assert.Equal(new[] { "OneModified", "TwoInserted", "Three" }, list2);
        }
    }
}
