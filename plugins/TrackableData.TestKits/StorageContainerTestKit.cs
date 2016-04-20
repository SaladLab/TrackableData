using System.Collections.Generic;
using System.Linq;
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
        private bool _useSet;

        protected abstract Task CreateAsync(TTrackableContainer person);
        protected abstract Task<int> DeleteAsync();
        protected abstract Task<TTrackableContainer> LoadAsync();
        protected abstract Task SaveAsync(TTrackableContainer person);
        protected abstract IEnumerable<ITrackable> GetTrackables(TTrackableContainer person);
        protected abstract IEnumerable<ITracker> GetTrackers(TTrackableContainer person);

        protected StorageContainerTestKit(bool useList, bool useSet)
        {
            _useList = useList;
            _useSet = useSet;
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

            var aliases = new TrackableSet<string>();
            if (_useSet)
            {
                container.Aliases = aliases;
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

            // Aliases
            if (_useSet)
            {
                aliases.Add("alpha");
                aliases.Add("beta");
                aliases.Add("gamma");
            }

            return container;
        }

        private void AssertEqual(TrackableDictionary<int, MissionData> a,
                                 TrackableDictionary<int, MissionData> b)
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

        private void AssertEqual(TrackableList<TagData> a, TrackableList<TagData> b)
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

        private void AssertEqual(TrackableSet<string> a, TrackableSet<string> b)
        {
            Assert.Equal(a.OrderBy(x => x).ToList(), b.OrderBy(x => x).ToList());
        }

        private void AssertContainerEqual(dynamic a, dynamic b)
        {
            Assert.Equal(a.Person.Name, b.Person.Name);
            Assert.Equal(a.Person.Age, b.Person.Age);
            Assert.Equal(a.Missions.Count, b.Missions.Count);
            AssertEqual(a.Missions, b.Missions);
            if (_useList)
                AssertEqual(a.Tags, b.Tags);
            if (_useSet)
                AssertEqual(a.Aliases, b.Aliases);
        }

        [Fact]
        public async Task Test_CreateAndLoad()
        {
            dynamic container = CreateTestContainer(false);
            await CreateAsync(container);

            dynamic container2 = await LoadAsync();
            AssertContainerEqual(container, container2);
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
            dynamic container = CreateTestContainer(false);
            await CreateAsync(container);
            ((ITrackable)container).SetDefaultTracker();

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
            {
                container.Tags.RemoveAt(0);
                container.Tags.Add(new TagData { Text = "Data", Priority = 3 });
            }

            // modify aliases

            if (_useSet)
            {
                container.Aliases.Remove("alpha");
                container.Aliases.Add("delta");
            }

            // save modification

            await SaveAsync(container);
            container.Tracker.Clear();

            // check equality

            dynamic container2 = await LoadAsync();
            AssertContainerEqual(container, container2);
        }

        [Fact]
        public void Test_GetTrackables()
        {
            dynamic container = CreateTestContainer(false);
            var trackables = new HashSet<ITrackable>(GetTrackables(container));
            Assert.Equal(new ITrackable[]
                         {
                            container.Person,
                            container.Missions,
                            _useList ? container.Tags : null,
                            _useSet ? container.Aliases : null,
                         }.Where(x => x != null).OrderBy(x => x.GetHashCode()),
                         trackables.OrderBy(x => x.GetHashCode()));
        }

        [Fact]
        public void Test_GetTrackers()
        {
            dynamic container = CreateTestContainer(true);
            var trackers = new HashSet<ITracker>(GetTrackers(container));
            Assert.Equal(new ITracker[]
                        {
                            container.Person.Tracker,
                            container.Missions.Tracker,
                            _useList ? container.Tags.Tracker : null,
                            _useSet ? container.Aliases.Tracker : null,
                        }.Where(x => x != null).OrderBy(x => x.GetHashCode()),
                        trackers.OrderBy(x => x.GetHashCode()));
        }
    }
}
