using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace TrackableData.TestKits
{
    public abstract class StorageDictionaryValueTestKit<TKey>
    {
        protected abstract TKey CreateKey(int value);
        protected abstract Task CreateAsync(IDictionary<TKey, string> dictionary);
        protected abstract Task<int> DeleteAsync();
        protected abstract Task<TrackableDictionary<TKey, string>> LoadAsync();
        protected abstract Task SaveAsync(TrackableDictionary<TKey, string> dictionary);

        private void AssertEqualDictionary(TrackableDictionary<TKey, string> a, TrackableDictionary<TKey, string> b)
        {
            Assert.Equal(a.Count, b.Count);
            foreach (var item in a)
            {
                Assert.Equal(item.Value, item.Value);
            }
        }

        [Fact]
        public async Task Test_CreateAndLoad()
        {
            var dict = new TrackableDictionary<TKey, string>();
            dict.Add(CreateKey(1), "One");
            dict.Add(CreateKey(2), "Two");
            dict.Add(CreateKey(3), "Three");

            await CreateAsync(dict);

            var dict2 = await LoadAsync();
            AssertEqualDictionary(dict, dict2);
        }

        [Fact]
        public async Task Test_Delete()
        {
            var dict = new TrackableDictionary<TKey, string>();
            dict.Add(CreateKey(1), "One");

            await CreateAsync(dict);

            var count = await DeleteAsync();
            var dict2 = await LoadAsync();

            Assert.True(count > 0);
            Assert.True(dict2 == null || dict2.Count == 0);
        }

        [Fact]
        public async Task Test_Save()
        {
            var dict = new TrackableDictionary<TKey, string>();
            dict.SetDefaultTracker();
            dict.Add(CreateKey(1), "One");
            dict.Add(CreateKey(2), "Two");
            dict.Add(CreateKey(3), "Three");

            await SaveAsync(dict);
            dict.Tracker.Clear();

            dict.Remove(CreateKey(1));
            dict[CreateKey(2)] = "TwoTwo";
            dict.Add(CreateKey(4), "Four");
            await SaveAsync(dict);

            var dict2 = await LoadAsync();
            AssertEqualDictionary(dict, dict2);
        }
    }
}
