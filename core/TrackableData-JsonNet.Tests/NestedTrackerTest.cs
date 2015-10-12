using System;
using System.Linq;
using Newtonsoft.Json;
using TrackableData.JsonNet.Tests.Data;
using Xunit;

namespace TrackableData.JsonNet.Tests
{
    public class NestedTrackerTest
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

        private TrackableDictionary<int, TrackablePerson> CreateTestDictionary()
        {
            return new TrackableDictionary<int, TrackablePerson>
            {
                {1, new TrackablePerson {Name = "Alice", Age = 20}},
                {2, new TrackablePerson {Name = "Bob", Age = 30}},
                {3, new TrackablePerson {Name = "Cindy", Age = 10}},
            };
        }

        private TrackableDictionary<int, TrackablePerson> CreateTestDictionaryWithTracker()
        {
            var dict = CreateTestDictionary();
            dict.SetDefaultTrackerDeep();
            return dict;
        }

        private TrackableList<TrackablePerson> CreateTestList()
        {
            return new TrackableList<TrackablePerson>
            {
                new TrackablePerson {Name = "Alice", Age = 20},
                new TrackablePerson {Name = "Bob", Age = 30},
                new TrackablePerson {Name = "Cindy", Age = 10},
            };
        }

        private TrackableList<TrackablePerson> CreateTestListWithTracker()
        {
            var list = CreateTestList();
            list.SetDefaultTrackerDeep();
            return list;
        }

        private JsonSerializerSettings GetJsonSerializerSettings()
        {
            return new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.None,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                Converters = new JsonConverter[]
                {
                    new TrackablePocoTrackerJsonConverter<Person>(),
                    new TrackablePocoTrackerJsonConverter<Hand>(),
                    new TrackablePocoTrackerJsonConverter<Ring>(),
                    new TrackableDictionaryTrackerJsonConverter<int, string>(),
                    new TrackableListTrackerJsonConverter<string>(),
                }
            };
        }

        [Fact]
        public void Test_NestedTracker_Poco_Serialize_Work()
        {
            var person = CreateTestPersonWithTracker();
            person.Name = "Bob";
            person.Age = 30;
            person.LeftHand.MainRing.Name = "EnhancedRing";
            person.RightHand.SubRing.Power = 2;

            var jsonSerializerSettings = GetJsonSerializerSettings();
            var json = person.SerializeChangedTrackersWithPath(jsonSerializerSettings);

            var person2 = CreateTestPerson();
            json.ApplyTo(person2, jsonSerializerSettings);

            Assert.Equal(person.Name, person2.Name);
            Assert.Equal(person.Age, person2.Age);
            Assert.Equal(person.LeftHand.MainRing.Name, person2.LeftHand.MainRing.Name);
            Assert.Equal(person.RightHand.SubRing.Power, person2.RightHand.SubRing.Power);
        }

        [Fact]
        public void Test_NestedTracker_Dictionary_Serialize_Work()
        {
            var dict = CreateTestDictionaryWithTracker();
            dict[1].Age += 1;
            dict[2].Age += 1;
            dict[3].Age += 1;

            var jsonSerializerSettings = GetJsonSerializerSettings();
            var json = dict.SerializeChangedTrackersWithPath(jsonSerializerSettings);

            var dict2 = CreateTestDictionary();
            json.ApplyTo(dict2, jsonSerializerSettings);

            Assert.Equal(dict[1].Age, dict2[1].Age);
            Assert.Equal(dict[2].Age, dict2[2].Age);
            Assert.Equal(dict[3].Age, dict2[3].Age);
        }

        [Fact]
        public void Test_NestedTracker_List_Serialize_Work()
        {
            var list = CreateTestListWithTracker();
            list[0].Age += 1;
            list[1].Age += 1;
            list[2].Age += 1;

            var jsonSerializerSettings = GetJsonSerializerSettings();
            var json = list.SerializeChangedTrackersWithPath(jsonSerializerSettings);

            var list2 = CreateTestList();
            json.ApplyTo(list2, jsonSerializerSettings);

            Assert.Equal(list[0].Age, list2[0].Age);
            Assert.Equal(list[1].Age, list2[1].Age);
            Assert.Equal(list[2].Age, list2[2].Age);
        }
    }
}
