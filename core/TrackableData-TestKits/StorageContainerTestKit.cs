using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace TrackableData.TestKits
{
    public class MissionData
    {
        public short Kind { get; set; }
        public int Count { get; set; }
        public string Note { get; set; }
    }

    public class TagData
    {
        public string Text { get; set; }
        public int Priority { get; set; }
    }

    public abstract class StorageContainerTestKit<TTrackableContainer, TTrackablePerson>
        where TTrackableContainer : ITrackableContainer, new()
        where TTrackablePerson : ITrackablePoco, new()
    {
        private bool _useList;

        protected abstract Task CreateAsync(TTrackableContainer person);
        protected abstract Task<int> DeleteAsync();
        protected abstract Task<TTrackableContainer> LoadAsync();
        protected abstract Task SaveAsync(IContainerTracker tracker);

        protected StorageContainerTestKit(bool useList)
        {
            _useList = useList;
        }

        private TTrackableContainer CreateTestContainer(bool withTracker)
        {
            dynamic container = new TTrackableContainer();

            dynamic person = new TTrackablePerson();
            container.Person = person;
            var missions = new TrackableDictionary<int, MissionData>();
            container.Missions = missions;

            var tags = new TrackableList<TagData>();
            if (_useList)
            {
                container.Tags = tags;
            }

            if (withTracker)
                ((ITrackable)container).SetDefaultTracker();

            // Person
            {
                person.Name = "Testor";
                person.Age = 10;
            }

            // Missions
            {
                var value1 = new MissionData();
                value1.Kind = 101;
                value1.Count = 1;
                value1.Note = "Handmade Sword";
                missions.Add(1, value1);
                var value2 = new MissionData();
                value2.Kind = 102;
                value2.Count = 3;
                value2.Note = "Lord of Ring";
                missions.Add(2, value2);
            }

            // Tags
            if (_useList)
            {
                tags.Add(new TagData { Text = "Hello", Priority = 1 });
                tags.Add(new TagData { Text = "World", Priority = 2 });
            }

            return container;
        }

        private void AssertEqualDictionary(TrackableDictionary<int, MissionData> a, TrackableDictionary<int, MissionData> b)
        {
            Assert.Equal(a.Count, b.Count);
            foreach (var item in a)
            {
                var a_v = item.Value;
                var b_v = b[item.Key];
                Assert.Equal(a_v.Kind, b_v.Kind);
                Assert.Equal(a_v.Count, b_v.Count);
                Assert.Equal(a_v.Note, b_v.Note);
            }
        }

        private void AssertEqualDictionary(TrackableList<TagData> a, TrackableList<TagData> b)
        {
            Assert.Equal(a.Count, b.Count);
            for (var i = 0; i < a.Count; i++)
            {
                var a_v = a[i];
                var b_v = b[i];
                Assert.Equal(a_v.Text, b_v.Text);
                Assert.Equal(a_v.Priority, b_v.Priority);
            }
        }

        [Fact]
        public async Task Test_CreateAndLoad()
        {
            dynamic container = CreateTestContainer(false);
            await CreateAsync(container);

            dynamic container2 = await LoadAsync();
            Assert.Equal(container.Person.Name, container2.Person.Name);
            Assert.Equal(container.Person.Age, container2.Person.Age);
            Assert.Equal(container.Missions.Count, container2.Missions.Count);
            AssertEqualDictionary(container.Missions, container2.Missions);
            if (_useList)
                AssertEqualDictionary(container.Tags, container2.Tags);
        }

        [Fact]
        public async Task Test_Delete()
        {
            dynamic container = CreateTestContainer(false);
            await CreateAsync(container);

            var count = await DeleteAsync();
            dynamic container2 = await LoadAsync();

            Assert.True(count > 0);
            Assert.True(container2 == null || container2.Person == null);
        }

        [Fact]
        public async Task Test_Save()
        {
            dynamic container = CreateTestContainer(true);
            await SaveAsync(container.Tracker);
            container.Tracker.Clear();

            // modify person

            container.Person.Age += 1;

            // modify missions

            container.Missions.Remove(1);

            var item2 = container.Missions[2];
            var value2 = new MissionData();
            value2.Kind = item2.Kind;
            value2.Count = item2.Count - 1;
            value2.Note = "Destroyed";
            container.Missions[2] = value2;

            var value3 = new MissionData();
            value3.Kind = 103;
            value3.Count = 3;
            value3.Note = "Just Arrived";
            container.Missions.Add(1, value3);

            // modify tags

            if (_useList)
                container.Tags.Add(new TagData { Text = "Data", Priority = 3 });

            // save modification

            await SaveAsync(container.Tracker);
            container.Tracker.Clear();

            // check equality

            dynamic container2 = await LoadAsync();
            AssertEqualDictionary(container.Missions, container2.Missions);
            if (_useList)
                AssertEqualDictionary(container.Tags, container2.Tags);
        }
    }
}
