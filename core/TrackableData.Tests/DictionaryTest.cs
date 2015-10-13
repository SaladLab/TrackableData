using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace TrackableData.Tests
{
    public class DictionaryTest
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

        [Fact]
        public void TestDictionary_Tracking_Work()
        {
            var dict = CreateTestDictionaryWithTracker();
            dict[1] = "OneModified";
            dict.Remove(2);
            dict[4] = "FourAdded";

            var changeMap = ((TrackableDictionaryTracker<int, string>)dict.Tracker).ChangeMap;
            Assert.Equal(3, changeMap.Count);

            var change1 = changeMap[1];
            Assert.Equal(TrackableDictionaryOperation.Modify, change1.Operation);
            Assert.Equal("One", change1.OldValue);
            Assert.Equal("OneModified", change1.NewValue);

            var change2 = changeMap[2];
            Assert.Equal(TrackableDictionaryOperation.Remove, change2.Operation);
            Assert.Equal("Two", change2.OldValue);
            Assert.Equal(null, change2.NewValue);

            var change4 = changeMap[4];
            Assert.Equal(TrackableDictionaryOperation.Add, change4.Operation);
            Assert.Equal(null, change4.OldValue);
            Assert.Equal("FourAdded", change4.NewValue);
        }

        [Fact]
        public void TestDictionary_ApplyToTrackable_Work()
        {
            var dict = CreateTestDictionaryWithTracker();
            dict[1] = "OneModified";
            dict.Remove(2);
            dict[4] = "FourAdded";

            var dict2 = CreateTestDictionary();
            dict.Tracker.ApplyTo(dict2);

            Assert.Equal(
                new KeyValuePair<int, string>[] {
                    new KeyValuePair<int, string>(1, "OneModified"),
                    new KeyValuePair<int, string>(3, "Three"),
                    new KeyValuePair<int, string>(4, "FourAdded")
                },
                dict2.OrderBy(kv => kv.Key));
        }

        [Fact]
        public void TestDictionary_ApplyToTracker_Work()
        {
            var dict = CreateTestDictionaryWithTracker();
            dict[1] = "OneModified";
            dict.Remove(2);
            dict[4] = "FourAdded";

            var tracker2 = new TrackableDictionaryTracker<int, string>();
            dict.Tracker.ApplyTo(tracker2);

            var dict2 = CreateTestDictionary();
            tracker2.ApplyTo(dict2);

            Assert.Equal(
                new KeyValuePair<int, string>[] {
                    new KeyValuePair<int, string>(1, "OneModified"),
                    new KeyValuePair<int, string>(3, "Three"),
                    new KeyValuePair<int, string>(4, "FourAdded")
                },
                dict2.OrderBy(kv => kv.Key));
        }

        [Fact]
        public void TestDictionary_RollbackToTrackable_Work()
        {
            var dict = CreateTestDictionaryWithTracker();
            dict[1] = "OneModified";
            dict.Remove(2);
            dict[4] = "FourAdded";

            var dict2 = CreateTestDictionary();
            dict.Tracker.ApplyTo(dict2);
            dict.Tracker.RollbackTo(dict2);

            Assert.Equal(
                new KeyValuePair<int, string>[] {
                    new KeyValuePair<int, string>(1, "One"),
                    new KeyValuePair<int, string>(2, "Two"),
                    new KeyValuePair<int, string>(3, "Three")
                },
                dict2.OrderBy(kv => kv.Key));
        }

        [Fact]
        public void TestDictionary_RollbackToTracker_Work()
        {
            var dict = CreateTestDictionaryWithTracker();
            dict[1] = "OneModified";
            dict.Remove(2);
            dict[4] = "FourAdded";

            var tracker2 = new TrackableDictionaryTracker<int, string>();
            dict.Tracker.ApplyTo(tracker2);
            dict.Tracker.RollbackTo(tracker2);

            var dict2 = CreateTestDictionary();
            tracker2.ApplyTo(dict2);

            Assert.Equal(
                new KeyValuePair<int, string>[] {
                    new KeyValuePair<int, string>(1, "One"),
                    new KeyValuePair<int, string>(2, "Two"),
                    new KeyValuePair<int, string>(3, "Three")
                },
                dict2.OrderBy(kv => kv.Key));
        }
    }
}
