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
        public void TestSet_UnionWith_Work()
        {
            var evenNumbers = new TrackableSet<int>() { 0, 2, 4, 6, 8 };
            var oddNumbers = new TrackableSet<int>() { 1, 3, 5, 7, 9 };

            evenNumbers.SetDefaultTrackerDeep();
            evenNumbers.UnionWith(oddNumbers);

            Assert.Equal(new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 },
                         evenNumbers.OrderBy(x => x));

            var tracker = (TrackableSetTracker<int>)evenNumbers.Tracker;
            Assert.Equal(new[] { 1, 3, 5, 7, 9 },
                         tracker.AddValues.OrderBy(x => x));
            Assert.Equal(new int[0],
                         tracker.RemoveValues.OrderBy(x => x));
        }

        [Fact]
        public void TestSet_IntersectWith_Work()
        {
            var lowNumbers = new TrackableSet<int>() { 0, 1, 2, 3, 4, 5 };
            var highNumbers = new TrackableSet<int>() { 3, 4, 5, 6, 7, 8, 9 };

            lowNumbers.SetDefaultTrackerDeep();
            lowNumbers.IntersectWith(highNumbers);

            Assert.Equal(new[] { 3, 4, 5 },
                         lowNumbers.OrderBy(x => x));

            var tracker = (TrackableSetTracker<int>)lowNumbers.Tracker;
            Assert.Equal(new int[0],
                         tracker.AddValues.OrderBy(x => x));
            Assert.Equal(new[] { 0, 1, 2 },
                         tracker.RemoveValues.OrderBy(x => x));
        }

        [Fact]
        public void TestSet_ExceptWith_Work()
        {
            var lowNumbers = new TrackableSet<int>() { 0, 1, 2, 3, 4, 5 };
            var highNumbers = new TrackableSet<int>() { 3, 4, 5, 6, 7, 8, 9 };

            lowNumbers.SetDefaultTrackerDeep();
            lowNumbers.ExceptWith(highNumbers);

            Assert.Equal(new[] { 0, 1, 2 },
                         lowNumbers.OrderBy(x => x));

            var tracker = (TrackableSetTracker<int>)lowNumbers.Tracker;
            Assert.Equal(new int[0],
                         tracker.AddValues.OrderBy(x => x));
            Assert.Equal(new[] { 3, 4, 5 },
                         tracker.RemoveValues.OrderBy(x => x));
        }

        [Fact]
        public void TestSet_SymmetricExceptWith_Work()
        {
            var lowNumbers = new TrackableSet<int>() { 0, 1, 2, 3, 4, 5 };
            var highNumbers = new TrackableSet<int>() { 3, 4, 5, 6, 7, 8, 9 };

            lowNumbers.SetDefaultTrackerDeep();
            lowNumbers.SymmetricExceptWith(highNumbers);

            Assert.Equal(new[] { 0, 1, 2, 6, 7, 8, 9 },
                         lowNumbers.OrderBy(x => x));

            var tracker = (TrackableSetTracker<int>)lowNumbers.Tracker;
            Assert.Equal(new[] { 6, 7, 8, 9 },
                         tracker.AddValues.OrderBy(x => x));
            Assert.Equal(new[] { 3, 4, 5 },
                         tracker.RemoveValues.OrderBy(x => x));
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
