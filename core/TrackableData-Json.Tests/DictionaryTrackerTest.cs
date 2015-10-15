using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Xunit;

namespace TrackableData.Json.Tests
{
    public class DictionaryTrackerTest
    {
        private TrackableDictionary<int, string> CreateTestDictionary()
        {
            return new TrackableDictionary<int, string>()
            {
                { 1, "One" },
                { 2, "Two" },
                { 3, "Three" }
            };
        }

        private TrackableDictionary<int, string> CreateTestDictionaryWithTracker()
        {
            var dict = CreateTestDictionary();
            dict.SetDefaultTrackerDeep();
            return dict;
        }

        private JsonSerializerSettings GetJsonSerializerSettings()
        {
            return new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.None,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                Converters = new JsonConverter[]
                {
                    new TrackableDictionaryTrackerJsonConverter<int, string>()
                }
            };
        }

        [Fact]
        public void Test_Dictionary_Serialize_Work()
        {
            var dict = CreateTestDictionaryWithTracker();
            var jsonSerializerSettings = GetJsonSerializerSettings();
            var json = JsonConvert.SerializeObject(dict, jsonSerializerSettings);
            var dict2 = JsonConvert.DeserializeObject<TrackableDictionary<int, string>>(json, jsonSerializerSettings);
            Assert.Equal(dict.Count, dict2.Count);
        }

        [Fact]
        public void Test_DictionaryTracker_Serialize_Work()
        {
            var dict = CreateTestDictionaryWithTracker();
            dict[1] = "OneModified";
            dict.Remove(2);
            dict[4] = "FourAdded";

            var jsonSerializerSettings = GetJsonSerializerSettings();
            var json = JsonConvert.SerializeObject(dict.Tracker, jsonSerializerSettings);
            var tracker2 = JsonConvert.DeserializeObject<TrackableDictionaryTracker<int, string>>(json,
                                                                                                  jsonSerializerSettings);

            var dict2 = CreateTestDictionary();
            tracker2.ApplyTo(dict2);

            Assert.Equal(
                new[]
                {
                    new KeyValuePair<int, string>(1, "OneModified"),
                    new KeyValuePair<int, string>(3, "Three"),
                    new KeyValuePair<int, string>(4, "FourAdded")
                },
                dict2.OrderBy(kv => kv.Key));
        }
    }
}
