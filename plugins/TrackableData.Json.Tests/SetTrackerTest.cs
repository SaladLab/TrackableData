using System.Collections.Generic;
using Newtonsoft.Json;
using Xunit;

namespace TrackableData.Json.Tests
{
    public class SetTrackerTest
    {
        private TrackableSet<int> CreateTestSet()
        {
            return new TrackableSet<int>() { 1, 2, 3 };
        }

        private TrackableSet<int> CreateTestSetWithTracker()
        {
            var set = CreateTestSet();
            set.SetDefaultTrackerDeep();
            return set;
        }

        private JsonSerializerSettings GetJsonSerializerSettings()
        {
            return new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.None,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                Converters = new JsonConverter[]
                {
                    new TrackableSetTrackerJsonConverter<int>()
                }
            };
        }

        [Fact]
        public void Test_Set_Serialize_Work()
        {
            var set = CreateTestSetWithTracker();
            var jsonSerializerSettings = GetJsonSerializerSettings();
            var json = JsonConvert.SerializeObject(set, jsonSerializerSettings);
            var set2 = JsonConvert.DeserializeObject<TrackableSet<int>>(json, jsonSerializerSettings);
            Assert.Equal(set.Count, set2.Count);
        }

        [Fact]
        public void Test_SetTracker_Serialize_Work()
        {
            var set = CreateTestSetWithTracker();
            set.Remove(1);
            set.Remove(2);
            set.Add(4);
            set.Add(5);

            var jsonSerializerSettings = GetJsonSerializerSettings();
            var json = JsonConvert.SerializeObject(set.Tracker, jsonSerializerSettings);
            var tracker2 = JsonConvert.DeserializeObject<TrackableSetTracker<int>>(json, jsonSerializerSettings);

            var set2 = CreateTestSet();
            tracker2.ApplyTo(set2);

            Assert.Equal(new HashSet<int> { 3, 4, 5 },
                         set2);
        }
    }
}
