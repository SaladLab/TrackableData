using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf.Meta;
using Xunit;

namespace TrackableData.Protobuf.Tests
{
    public class TrackableDictionaryTest
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

        private TypeModel CreateTypeModel()
        {
            var model = TypeModel.Create();
            model.Add(typeof (TrackableDictionaryTracker<int, string>), false)
                 .SetSurrogate(typeof (TrackableDictionaryTrackerSurrogate<int, string>));
            return model;
        }

        [Fact]
        public void Test_TrackableDictionary_Serialize_Work()
        {
            var dict = CreateTestDictionaryWithTracker();

            var typeModel = CreateTypeModel();
            var dict2 = (TrackableDictionary<int, string>)typeModel.DeepClone(dict);

            Assert.Equal(
                new[] {
                    new KeyValuePair<int, string>(1, "One"),
                    new KeyValuePair<int, string>(2, "Two"),
                    new KeyValuePair<int, string>(3, "Three")
                },
                dict2.OrderBy(kv => kv.Key));
        }

        [Fact]
        public void Test_TrackableDictionaryTracker_Serialize_Work()
        {
            var dict = CreateTestDictionaryWithTracker();
            dict[1] = "OneModified";
            dict.Remove(2);
            dict[4] = "FourAdded";

            var typeModel = CreateTypeModel();
            var tracker2 = (TrackableDictionaryTracker<int, string>)typeModel.DeepClone(dict.Tracker);

            var dict2 = CreateTestDictionary();
            tracker2.ApplyTo(dict2);

            Assert.Equal(
                new[] {
                    new KeyValuePair<int, string>(1, "OneModified"),
                    new KeyValuePair<int, string>(3, "Three"),
                    new KeyValuePair<int, string>(4, "FourAdded")
                },
                dict2.OrderBy(kv => kv.Key));
        }
    }
}
