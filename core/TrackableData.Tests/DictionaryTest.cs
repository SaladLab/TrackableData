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
        public void TestDictionary_Update_Existing_Key_Updated()
        {
            var dict = CreateTestDictionaryWithTracker();
            var ret = dict.Update(1, (key, value) => value + "!");
            Assert.Equal(true, ret);
            Assert.Equal(true, dict.Tracker.HasChange);
            Assert.Equal("One!", dict[1]);
        }

        [Fact]
        public void TestDictionary_Update_NonExisting_Key_NotChanged()
        {
            var dict = CreateTestDictionaryWithTracker();
            var ret = dict.Update(-1, (key, value) => value + "!");
            Assert.Equal(false, ret);
            Assert.Equal(false, dict.Tracker.HasChange);
        }

        [Fact]
        public void TestDictionary_AddOrUpdate_Existing_Key_Updated()
        {
            var dict = CreateTestDictionaryWithTracker();
            var ret = dict.AddOrUpdate(1, (key) => "", (key, value) => value + "!");
            Assert.Equal("One!", ret);
            Assert.Equal(true, dict.Tracker.HasChange);
            Assert.Equal("One!", dict[1]);
        }

        [Fact]
        public void TestDictionary_AddOrUpdate_NonExisting_Key_Added()
        {
            var dict = CreateTestDictionaryWithTracker();
            var ret = dict.AddOrUpdate(-1, (key) => "New" + key, (key, value) => value + "!");
            Assert.Equal("New-1", ret);
            Assert.Equal(true, dict.Tracker.HasChange);
            Assert.Equal("New-1", dict[-1]);
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
        public void TestDictionary_HasChangedSetEvent_Work()
        {
            var changed = false;
            var dict = CreateTestDictionaryWithTracker();
            dict.Tracker.HasChangeSet += _ => { changed = true; };
            dict[1] = "OneModified";
            Assert.Equal(true, changed);
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
                new KeyValuePair<int, string>[]
                {
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
                new KeyValuePair<int, string>[]
                {
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
                new KeyValuePair<int, string>[]
                {
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
                new KeyValuePair<int, string>[]
                {
                    new KeyValuePair<int, string>(1, "One"),
                    new KeyValuePair<int, string>(2, "Two"),
                    new KeyValuePair<int, string>(3, "Three")
                },
                dict2.OrderBy(kv => kv.Key));
        }

        [Fact]
        public void TestDictionary_Clone_Work()
        {
            var a = CreateTestDictionaryWithTracker();
            var b = a.Clone();

            Assert.Null(b.Tracker);
            Assert.False(ReferenceEquals(a._dictionary, b._dictionary));
            Assert.Equal(a._dictionary, b._dictionary);
        }
    }
}
