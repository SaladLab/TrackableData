using System;
using System.Linq;
using TrackableData.Tests;
using Xunit;

namespace TrackableData.Tests
{
    public class ContainerTest
    {
        private TrackableDataContainer CreateTestContainer()
        {
            return new TrackableDataContainer
            {
                Person = new TrackablePerson
                {
                    Name = "Alice",
                    Age = 20,
                },
                Dictionary = new TrackableDictionary<int, string>
                {
                    {1, "One"},
                    {2, "Two"},
                    {3, "Three"}
                },
                List = new TrackableList<string>
                {
                    "One",
                    "Two",
                    "Three"
                }
            };
        }

        private TrackableDataContainer CreateTestContainerWithTracker()
        {
            var container = CreateTestContainer();
            container.Tracker = new TrackableDataContainerTracker();
            return container;
        }

        [Fact]
        public void ContainerTest_Tracking_Work()
        {
            var c = CreateTestContainerWithTracker();

            // Act

            c.Person.Name = "Bob";
            c.Person.Age = 30;

            c.Dictionary[1] = "OneModified";
            c.Dictionary.Remove(2);
            c.Dictionary[4] = "FourAdded";

            c.List[0] = "OneModified";
            c.List.RemoveAt(1);
            c.List.Insert(1, "TwoInserted");

            // Assert

            Assert.Equal(2, c.Tracker.PersonTracker.ChangeMap.Count);
            Assert.Equal(3, c.Tracker.DictionaryTracker.ChangeMap.Count);
            Assert.Equal(3, c.Tracker.ListTracker.ChangeList.Count);
        }

        [Fact]
        public void ContainerTest_ApplyToTrackable_Work()
        {
            var c = CreateTestContainerWithTracker();
            c.Person.Name = "Bob";
            c.Person.Age = 30;

            var c2 = CreateTestContainer();
            c.Tracker.ApplyTo(c2);

            Assert.Equal(c.Person.Name, c2.Person.Name);
            Assert.Equal(c.Person.Age, c2.Person.Age);
        }

        [Fact]
        public void ContainerTest_ApplyToTracker_Work()
        {
            var c = CreateTestContainerWithTracker();
            c.Person.Name = "Bob";
            c.Person.Age = 30;

            var tracker2 = new TrackableDataContainerTracker();
            c.Tracker.ApplyTo(tracker2);

            var c2 = CreateTestContainer();
            tracker2.ApplyTo(c2);

            Assert.Equal(c.Person.Name, c2.Person.Name);
            Assert.Equal(c.Person.Age, c2.Person.Age);
        }
    }
}
