using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace TrackableData.TestKits
{
    public abstract class StorageDictionaryValueTestKit<TKey>
    {
        private bool _useDuplicateCheck;

        protected abstract TKey CreateKey(int value);
        protected abstract Task CreateAsync(IDictionary<TKey, string> dictionary);
        protected abstract Task<int> DeleteAsync();
        protected abstract Task<TrackableDictionary<TKey, string>> LoadAsync();
        protected abstract Task SaveAsync(TrackableDictionary<TKey, string> dictionary);

        protected StorageDictionaryValueTestKit(bool useDuplicateCheck = false)
        {
            _useDuplicateCheck = useDuplicateCheck;
        }

        private TrackableDictionary<TKey, string> CreateTestDictionary()
        {
            var dict = new TrackableDictionary<TKey, string>();
            dict.Add(CreateKey(1), "One");
            dict.Add(CreateKey(2), "Two");
            dict.Add(CreateKey(3), "Three");
            return dict;
        }

        private void AssertEqual(TrackableDictionary<TKey, string> a, TrackableDictionary<TKey, string> b)
        {
            Assert.Equal(a.OrderBy(x => x.Key), b.OrderBy(x => x.Key));
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

            dict.SetDefaultTracker();
            dict.Remove(CreateKey(1));
            dict[CreateKey(2)] = "TwoTwo";
            dict.Add(CreateKey(4), "Four");
            await SaveAsync(dict);

            var dict2 = await LoadAsync();
            AssertEqual(dict, dict2);
        }
    }
}
