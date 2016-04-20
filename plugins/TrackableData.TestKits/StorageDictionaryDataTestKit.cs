using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace TrackableData.TestKits
{
    public class ItemData
    {
        public short Kind { get; set; }
        public int Count { get; set; }
        public string Note { get; set; }
    }

    public abstract class StorageDictionaryDataTestKit<TKey>
    {
        private bool _useDuplicateCheck;

        protected abstract TKey CreateKey(int value);
        protected abstract Task CreateAsync(IDictionary<TKey, ItemData> dictionary);
        protected abstract Task<int> DeleteAsync();
        protected abstract Task<TrackableDictionary<TKey, ItemData>> LoadAsync();
        protected abstract Task SaveAsync(TrackableDictionary<TKey, ItemData> dictionary);

        protected StorageDictionaryDataTestKit(bool useDuplicateCheck = false)
        {
            _useDuplicateCheck = useDuplicateCheck;
        }

        private TrackableDictionary<TKey, ItemData> CreateTestDictionary()
        {
            var dict = new TrackableDictionary<TKey, ItemData>();

            var value1 = new ItemData();
            value1.Kind = 101;
            value1.Count = 1;
            value1.Note = "Handmade Sword";
            dict.Add(CreateKey(1), value1);

            var value2 = new ItemData();
            value2.Kind = 102;
            value2.Count = 3;
            value2.Note = "Lord of Ring";
            dict.Add(CreateKey(2), value2);

            return dict;
        }

        private void AssertEqual(TrackableDictionary<TKey, ItemData> a, TrackableDictionary<TKey, ItemData> b)
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

        [Fact]
        public async Task Test_CreateAndLoad()
        {
            var dict = CreateTestDictionary();
            await CreateAsync(dict);

            var dict2 = await LoadAsync();
            AssertEqual(dict, dict2);
        }

        [Fact]
        public async Task Test_CreateAndCreate_DuplicateError()
        {
            if (_useDuplicateCheck == false)
                return;

            var dict = CreateTestDictionary();
            await CreateAsync(dict);
            var e = await Record.ExceptionAsync(async () => await CreateAsync(dict));
            Assert.NotNull(e);
        }

        [Fact]
        public async Task Test_Delete()
        {
            var dict = CreateTestDictionary();
            await CreateAsync(dict);

            var count = await DeleteAsync();
            var dict2 = await LoadAsync();

            Assert.True(count > 0);
            Assert.True(dict2 == null || dict2.Count == 0);
        }

        [Fact]
        public async Task Test_Save()
        {
            var dict = CreateTestDictionary();
            await CreateAsync(dict);

            // modify dictionary

            dict.SetDefaultTracker();

            dict.Remove(CreateKey(1));

            var item2 = dict[CreateKey(2)];
            var value2 = new ItemData();
            value2.Kind = item2.Kind;
            value2.Count = item2.Count - 1;
            value2.Note = "Destroyed";
            dict[CreateKey(2)] = value2;

            var value3 = new ItemData();
            value3.Kind = 103;
            value3.Count = 3;
            value3.Note = "Just Arrived";
            dict.Add(CreateKey(1), value3);

            // save modification

            await SaveAsync(dict);
            dict.Tracker.Clear();

            // check equality

            var dict2 = await LoadAsync();
            AssertEqual(dict, dict2);
        }
    }
}
