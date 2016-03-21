using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace TrackableData.Tests
{
    public class SetTest
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

        [Fact]
        public void TestSet_Tracking_Work()
        {
            var set = CreateTestSetWithTracker();
            set.Remove(2);
            set.Add(4);

            var changeMap = ((TrackableSetTracker<int>)set.Tracker).ChangeMap;
            Assert.Equal(2, changeMap.Count);

            var change2 = changeMap[2];
            Assert.Equal(TrackableSetOperation.Remove, change2);

            var change4 = changeMap[4];
            Assert.Equal(TrackableSetOperation.Add, change4);
        }

        [Fact]
        public void TestSet_HasChangedSetEvent_Work()
        {
            var changed = false;
            var set = CreateTestSetWithTracker();
            set.Tracker.HasChangeSet += _ => { changed = true; };
            set.Add(4);
            Assert.Equal(true, changed);
        }

        [Fact]
        public void TestSet_ApplyToTrackable_Work()
        {
            var set = CreateTestSetWithTracker();
            set.Remove(2);
            set.Add(4);

            var set2 = CreateTestSet();
            set.Tracker.ApplyTo(set2);

            Assert.Equal(
                new[] { 1, 3, 4 },
                set2.OrderBy(v => v));
        }

        [Fact]
        public void TestSet_ApplyToTracker_Work()
        {
            var set = CreateTestSetWithTracker();
            set.Remove(2);
            set.Add(4);

            var tracker2 = new TrackableSetTracker<int>();
            set.Tracker.ApplyTo(tracker2);

            var set2 = CreateTestSet();
            tracker2.ApplyTo(set2);

            Assert.Equal(
                new[] { 1, 3, 4 },
                set2.OrderBy(v => v));
        }

        [Fact]
        public void TestSet_RollbackToTrackable_Work()
        {
            var set = CreateTestSetWithTracker();
            set.Remove(2);
            set.Add(4);

            var set2 = CreateTestSet();
            set.Tracker.ApplyTo(set2);
            set.Tracker.RollbackTo(set2);

            Assert.Equal(
                new[] { 1, 2, 3 },
                set2.OrderBy(v => v));
        }

        [Fact]
        public void TestSet_RollbackToTracker_Work()
        {
            var set = CreateTestSetWithTracker();
            set.Remove(2);
            set.Add(4);

            var tracker2 = new TrackableSetTracker<int>();
            set.Tracker.ApplyTo(tracker2);
            set.Tracker.RollbackTo(tracker2);

            var set2 = CreateTestSet();
            tracker2.ApplyTo(set2);

            Assert.Equal(
                new[] { 1, 2, 3 },
                set2.OrderBy(v => v));
        }
    }
}
