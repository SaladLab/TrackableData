using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Xunit;

namespace TrackableData.Json.Tests
{
    public class TrackerJsonConverterTest
    {
        private JsonSerializerSettings GetJsonSerializerSettings()
        {
            return new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.None,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                Converters = new JsonConverter[]
                {
                    new TrackerJsonConverter(),
                }
            };
        }

        private void AssertTrackerSerialize<T>(T tracker)
        {
            var jsonSerializerSettings = GetJsonSerializerSettings();
            var json = JsonConvert.SerializeObject(tracker, jsonSerializerSettings);
            var tracker2 = JsonConvert.DeserializeObject<T>(json, jsonSerializerSettings);
            Assert.NotNull(tracker2);
        }

        [Fact]
        public void Test_PocoTracker_Serialize()
        {
            var tracker = new TrackablePocoTracker<IPerson>();
            tracker.TrackSet(typeof(IPerson).GetProperty("Age"), 10, 11);
            AssertTrackerSerialize(tracker);
        }

        [Fact]
        public void Test_DictionaryTracker_Serialize()
        {
            var tracker = new TrackableDictionaryTracker<int, string>();
            tracker.TrackAdd(1, "One");
            AssertTrackerSerialize(tracker);
        }

        [Fact]
        public void Test_ListTracker_Serialize()
        {
            var tracker = new TrackableListTracker<int>();
            tracker.TrackPushBack(0);
            AssertTrackerSerialize(tracker);
        }

        [Fact]
        public void Test_ContainerTracker_Serialize()
        {
            var tracker = new TrackableDataContainerTracker();
            AssertTrackerSerialize(tracker);
        }
    }
}
