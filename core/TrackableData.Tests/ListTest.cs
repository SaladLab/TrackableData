using System;
using System.Linq;
using TrackableData.Tests;
using Xunit;

namespace TrackableData.Tests
{
    public class ListTest
    {
        private TrackableList<string> CreateTestList()
        {
            return new TrackableList<string>()
            {
                "One", "Two", "Three"
            };
        }

        private TrackableList<string> CreateTestListWithTracker()
        {
            var list = CreateTestList();
            list.SetDefaultTrackerDeep();
            return list;
        }

        [Fact]
        public void TestList_Tracking_Work()
        {
            var list = CreateTestListWithTracker();
            list[0] = "OneModified";
            list.RemoveAt(1);
            list.Insert(1, "TwoInserted");

            Assert.Equal(3, list.Tracker.ChangeList.Count);

            var change0 = list.Tracker.ChangeList[0];
            Assert.Equal(TrackableListOperation.Modify, change0.Operation);
            Assert.Equal(0, change0.Index);
            Assert.Equal("One", change0.OldValue);
            Assert.Equal("OneModified", change0.NewValue);

            var change1 = list.Tracker.ChangeList[1];
            Assert.Equal(TrackableListOperation.Remove, change1.Operation);
            Assert.Equal(1, change1.Index);
            Assert.Equal("Two", change1.OldValue);
            Assert.Equal(null, change1.NewValue);

            var change2 = list.Tracker.ChangeList[2];
            Assert.Equal(TrackableListOperation.Insert, change2.Operation);
            Assert.Equal(1, change2.Index);
            Assert.Equal(null, change2.OldValue);
            Assert.Equal("TwoInserted", change2.NewValue);
        }

        [Fact]
        public void TestList_ApplyToTrackable_Work()
        {
            var list = CreateTestListWithTracker();
            list[0] = "OneModified";
            list.RemoveAt(1);
            list.Insert(1, "TwoInserted");

            var list2 = CreateTestList();
            list.Tracker.ApplyTo(list2);

            Assert.Equal(new[] { "OneModified", "TwoInserted", "Three" }, list2);
        }

        [Fact]
        public void TestList_ApplyToTracker_Work()
        {
            var list = CreateTestListWithTracker();
            list[0] = "OneModified";
            list.RemoveAt(1);
            list.Insert(1, "TwoInserted");

            var tracker2 = new TrackableListTracker<string>();
            list.Tracker.ApplyTo(tracker2);

            var list2 = CreateTestList();
            tracker2.ApplyTo(list2);

            Assert.Equal(new[] { "OneModified", "TwoInserted", "Three" }, list2);
        }

        [Fact]
        public void TestList_RollbackToTrackable_Work()
        {
            var list = CreateTestListWithTracker();
            list[0] = "OneModified";
            list.RemoveAt(1);
            list.Insert(1, "TwoInserted");

            var list2 = CreateTestList();
            list.Tracker.ApplyTo(list2);
            list.Tracker.RollbackTo(list2);

            Assert.Equal(new[] { "One", "Two", "Three" }, list2);
        }

        [Fact]
        public void TestList_RollbackToTracker_Work()
        {
            var list = CreateTestListWithTracker();
            list[0] = "OneModified";
            list.RemoveAt(1);
            list.Insert(1, "TwoInserted");

            var tracker2 = new TrackableListTracker<string>();
            list.Tracker.ApplyTo(tracker2);
            list.Tracker.RollbackTo(tracker2);

            var list2 = CreateTestList();
            tracker2.ApplyTo(list2);

            Assert.Equal(new[] { "One", "Two", "Three" }, list2);
        }
    }
}
