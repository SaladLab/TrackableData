using System;
using System.Linq;
using TrackableData.Tests.Data;
using Xunit;

namespace TrackableData.Tests
{
    public class PocoTest
    {
        [Fact]
        public void TestPoco_Tracking_Work()
        {
            var employee = new TrackableEmployee();
            employee.Tracker = new TrackableData.TrackablePocoTracker<Employee>();
            employee.Name = "Bob";

            Assert.Equal(true, employee.Tracker.HasChange);
            Assert.Equal(1, employee.Tracker.ChangeMap.Count);
            Assert.Equal("Name", employee.Tracker.ChangeMap.Keys.First().Name);
            Assert.Equal("Bob", employee.Tracker.ChangeMap.Values.First().NewValue);
            Assert.Equal(null, employee.Tracker.ChangeMap.Values.First().OldValue);
        }

        [Fact]
        public void TestPoco_OverlappedTracking_Work()
        {
            var employee = new TrackableEmployee();
            employee.Tracker = new TrackableData.TrackablePocoTracker<Employee>();
            employee.Name = "Alice";
            employee.Name = "Bob";

            Assert.Equal(true, employee.Tracker.HasChange);
            Assert.Equal(1, employee.Tracker.ChangeMap.Count);
            Assert.Equal("Name", employee.Tracker.ChangeMap.Keys.First().Name);
            Assert.Equal("Bob", employee.Tracker.ChangeMap.Values.First().NewValue);
            Assert.Equal(null, employee.Tracker.ChangeMap.Values.First().OldValue);
        }

        [Fact]
        public void TestPoco_ApplyToObject_Work()
        {
            // TODO
        }

        [Fact]
        public void TestPoco_ApplyToTracker_Work()
        {
            // TODO
        }

        [Fact]
        public void TestPoco_RollbackToObject_Work()
        {
            // TODO
        }
    }
}
