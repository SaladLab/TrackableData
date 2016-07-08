using System.Linq;
using Xunit;

namespace TrackableData.Tests
{
    public class PocoTest
    {
        private TrackablePerson CreateTestPerson()
        {
            return new TrackablePerson
            {
                Name = "Alice",
                Age = 20,
                LeftHand = new TrackableHand
                {
                    MainRing = new TrackableRing { Name = "NormalRing", Power = 10 },
                    SubRing = new TrackableRing { Name = "TutorialRing", Power = 5 }
                },
                RightHand = new TrackableHand
                {
                    MainRing = new TrackableRing { Name = "NormalRing", Power = 9 },
                    SubRing = new TrackableRing { Name = "DummyRing", Power = 1 }
                }
            };
        }

        private TrackablePerson CreateTestPersonWithTracker()
        {
            var person = CreateTestPerson();
            person.SetDefaultTrackerDeep();
            return person;
        }

        [Fact]
        public void TestPoco_Tracking_Work()
        {
            var person = CreateTestPersonWithTracker();
            person.Name = "Bob";

            var changeMap = ((TrackablePocoTracker<IPerson>)person.Tracker).ChangeMap;
            Assert.Equal(true, person.Tracker.HasChange);
            Assert.Equal(1, changeMap.Count);
            Assert.Equal("Name", changeMap.Keys.First().Name);
            Assert.Equal("Bob", changeMap.Values.First().NewValue);
            Assert.Equal("Alice", changeMap.Values.First().OldValue);
        }

        [Fact]
        public void TestPoco_HasChangedSetEvent_Work()
        {
            var changed = false;
            var person = CreateTestPersonWithTracker();
            person.Tracker.HasChangeSet += _ => { changed = true; };
            person.Name = "Bob";
            Assert.Equal(true, changed);
        }

        [Fact]
        public void TestPoco_OverlappedTracking_Work()
        {
            var person = CreateTestPersonWithTracker();
            person.Name = "Bob";
            person.Name = "Cindy";

            var changeMap = ((TrackablePocoTracker<IPerson>)person.Tracker).ChangeMap;
            Assert.Equal(true, person.Tracker.HasChange);
            Assert.Equal(1, changeMap.Count);
            Assert.Equal("Name", changeMap.Keys.First().Name);
            Assert.Equal("Cindy", changeMap.Values.First().NewValue);
            Assert.Equal("Alice", changeMap.Values.First().OldValue);
        }

        [Fact]
        public void TestPoco_ApplyToTrackable_Work()
        {
            var person = CreateTestPersonWithTracker();
            person.Name = "Bob";
            person.Age = 30;

            var person2 = CreateTestPerson();
            person.Tracker.ApplyTo(person2);

            Assert.Equal(person.Name, person2.Name);
            Assert.Equal(person.Age, person2.Age);
        }

        [Fact]
        public void TestPoco_ApplyToTracker_Work()
        {
            var person = CreateTestPersonWithTracker();
            person.Name = "Bob";
            person.Age = 30;

            var tracker2 = new TrackablePocoTracker<IPerson>();
            person.Tracker.ApplyTo(tracker2);

            var person2 = CreateTestPerson();
            tracker2.ApplyTo(person2);

            Assert.Equal(person.Name, person2.Name);
            Assert.Equal(person.Age, person2.Age);
        }

        [Fact]
        public void TestPoco_RollbackToTrackable_Work()
        {
            var person = CreateTestPersonWithTracker();
            person.Name = "Bob";
            person.Age = 30;

            var person2 = CreateTestPerson();
            person.Tracker.ApplyTo(person2);
            person.Tracker.RollbackTo(person2);

            Assert.Equal("Alice", person2.Name);
            Assert.Equal(20, person2.Age);
        }

        [Fact]
        public void TestPoco_RollbackToTracker_Work()
        {
            var person = CreateTestPersonWithTracker();
            person.Name = "Bob";
            person.Age = 30;

            var tracker2 = new TrackablePocoTracker<IPerson>();
            person.Tracker.ApplyTo(tracker2);
            person.Tracker.RollbackTo(tracker2);

            var person2 = CreateTestPerson();
            tracker2.ApplyTo(person2);

            Assert.Equal("Alice", person2.Name);
            Assert.Equal(20, person2.Age);
        }

        [Fact]
        public void TestPoco_Clone_Work()
        {
            var a = CreateTestPersonWithTracker();
            var b = a.Clone();

            Assert.Null(b.Tracker);
            Assert.Equal(a.Name, b.Name);
            Assert.Equal(a.Age, b.Age);
            Assert.False(ReferenceEquals(a.LeftHand, b.LeftHand));
            Assert.Equal(a.LeftHand.MainRing.Name, b.LeftHand.MainRing.Name);
            Assert.Equal(a.LeftHand.MainRing.Power, b.LeftHand.MainRing.Power);
            Assert.False(ReferenceEquals(a.RightHand, b.RightHand));
            Assert.Equal(a.RightHand.MainRing.Name, b.RightHand.MainRing.Name);
            Assert.Equal(a.RightHand.MainRing.Power, b.RightHand.MainRing.Power);
        }
    }
}
