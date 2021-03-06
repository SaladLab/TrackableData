﻿using System.Linq;
using ProtoBuf.Meta;
using Xunit;

namespace TrackableData.Protobuf.Tests
{
    public class TrackableContainerTest
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
                    { 1, "One" },
                    { 2, "Two" },
                    { 3, "Three" }
                },
                List = new TrackableList<string>
                {
                    "One",
                    "Two",
                    "Three"
                },
                Set = new TrackableSet<int>()
                {
                    1, 2, 3
                }
            };
        }

        private TrackableDataContainer CreateTestContainerWithTracker()
        {
            var container = CreateTestContainer();
            container.Tracker = new TrackableDataContainerTracker();
            return container;
        }

        private TypeModel CreateTypeModel()
        {
            var model = TypeModel.Create();
            model.Add(typeof(TrackablePocoTracker<IPerson>), false)
                 .SetSurrogate(typeof(TrackablePersonTrackerSurrogate));
            model.Add(typeof(TrackableDictionaryTracker<int, string>), false)
                 .SetSurrogate(typeof(TrackableDictionaryTrackerSurrogate<int, string>));
            model.Add(typeof(TrackableListTracker<string>), false)
                 .SetSurrogate(typeof(TrackableListTrackerSurrogate<string>));
            model.Add(typeof(TrackableSetTracker<int>), false)
                 .SetSurrogate(typeof(TrackableSetTrackerSurrogate<int>));
            return model;
        }

        [Fact]
        public void Test_TrackableContainer_Serialize_Work()
        {
            var c = CreateTestContainerWithTracker();
            var typeModel = CreateTypeModel();
            var c2 = (TrackableDataContainer)typeModel.DeepClone(c);

            Assert.Equal(c.Person.Name, c2.Person.Name);
            Assert.Equal(c.Person.Age, c2.Person.Age);
            Assert.Equal(c.Dictionary.Count, c2.Dictionary.Count);
            Assert.Equal(c.List.Count, c2.List.Count);
        }

        [Fact]
        public void Test_TrackableContainerTracker_Serialize_Work()
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

            c.Set.Remove(1);
            c.Set.Remove(2);
            c.Set.Add(4);
            c.Set.Add(5);

            // Assert

            var typeModel = CreateTypeModel();
            var tracker2 = (TrackableDataContainerTracker)typeModel.DeepClone(c.Tracker);

            var c2 = CreateTestContainer();
            tracker2.ApplyTo(c2);

            Assert.Equal(c.Person.Name, c2.Person.Name);
            Assert.Equal(c.Person.Age, c2.Person.Age);
            Assert.Equal(c.Dictionary.OrderBy(x => x.Key), c2.Dictionary.OrderBy(x => x.Key));
            Assert.Equal(c.List, c2.List);
            Assert.Equal(c.Set.OrderBy(x => x), c2.Set.OrderBy(x => x));
        }
    }
}
