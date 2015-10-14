using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using TrackableData.Json.Tests;
using Xunit;

namespace TrackableData.Json.Tests
{
    public class PocoTrackerTest
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

        private JsonSerializerSettings GetJsonSerializerSettings()
        {
            return new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.None,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                Converters = new JsonConverter[]
                {
                    new TrackablePocoTrackerJsonConverter<IPerson>()
                }
            };
        }

        [Fact]
        public void Test_Poco_Serialize_Work()
        {
            var person = CreateTestPersonWithTracker();
            var jsonSerializerSettings = GetJsonSerializerSettings();
            var json = JsonConvert.SerializeObject(person, jsonSerializerSettings);
            var person2 = JsonConvert.DeserializeObject<TrackablePerson>(json, jsonSerializerSettings);
            Assert.Equal(person.Name, person2.Name);
        }

        [Fact]
        public void Test_PocoTracker_Serialize_Work()
        {
            var person = CreateTestPersonWithTracker();
            person.Name = "Bob";
            person.Age = 30;

            var jsonSerializerSettings = GetJsonSerializerSettings();
            var json = JsonConvert.SerializeObject(person.Tracker, jsonSerializerSettings);
            var tracker2 = JsonConvert.DeserializeObject<TrackablePocoTracker<IPerson>>(json, jsonSerializerSettings);

            var person2 = CreateTestPerson();
            tracker2.ApplyTo(person2);

            Assert.Equal(person.Name, person2.Name);
            Assert.Equal(person.Age, person2.Age);
        }
    }
}
