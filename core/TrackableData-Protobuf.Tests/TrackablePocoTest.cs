using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ProtoBuf;
using ProtoBuf.Meta;
using TrackableData.Protobuf.Tests;
using Xunit;

namespace TrackableData.Protobuf.Tests
{
    public class TrackablePocoTest
    {
        private TrackablePerson CreateTestPerson()
        {
            return new TrackablePerson
            {
                Name = "Alice",
                Age = 20,
            };
        }

        private TrackablePerson CreateTestPersonWithTracker()
        {
            var person = CreateTestPerson();
            person.SetDefaultTrackerDeep();
            return person;
        }

        private TypeModel CreateTypeModel()
        {
            var model = TypeModel.Create();
            model.Add(typeof(TrackablePocoTracker<IPerson>), false)
                 .SetSurrogate(typeof(TrackablePersonTrackerSurrogate));
            return model;
        }

        [Fact]
        public void Test_TrackablePoco_Serialize_Work()
        {
            var person = CreateTestPersonWithTracker();
            var typeModel = CreateTypeModel();
            var person2 = (IPerson)typeModel.DeepClone(person);

            Assert.Equal(person.Name, person2.Name);
            Assert.Equal(person.Age, person2.Age);
        }

        [Fact]
        public void Test_TrackablePocoTracker_Serialize_Work()
        {
            var person = CreateTestPersonWithTracker();
            person.Name = "Bob";
            person.Age = 30;

            var typeModel = CreateTypeModel();
            var tracker2 = (TrackablePocoTracker<IPerson>)typeModel.DeepClone(person.Tracker);

            var person2 = CreateTestPerson();
            tracker2.ApplyTo(person2);

            Assert.Equal(person.Name, person2.Name);
            Assert.Equal(person.Age, person2.Age);
        }
    }
}
