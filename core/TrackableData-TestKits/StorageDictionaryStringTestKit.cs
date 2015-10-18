using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace TrackableData.TestKits
{
    public abstract class StorageDictionaryStringTestKit<TKey>
    {
        protected abstract TKey CreateKey(int value);
        protected abstract Task<TrackableDictionary<TKey, string>> LoadAsync();
        protected abstract Task SaveAsync(ITracker tracker);

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
            dict.SetDefaultTracker();
            dict.Add(CreateKey(1), "One");
            dict.Add(CreateKey(2), "Two");
            dict.Add(CreateKey(3), "Three");

            await SaveAsync(dict.Tracker);

            var dict2 = await LoadAsync();
            AssertEqualDictionary(dict, dict2);
        }

        [Fact]
        public async Task Test_Save()
        {
            var dict = new TrackableDictionary<TKey, string>();
            dict.SetDefaultTracker();
            dict.Add(CreateKey(1), "One");
            dict.Add(CreateKey(2), "Two");
            dict.Add(CreateKey(3), "Three");

            await SaveAsync(dict.Tracker);
            dict.Tracker.Clear();

            dict.Remove(CreateKey(1));
            dict[CreateKey(2)] = "TwoTwo";
            dict.Add(CreateKey(4), "Four");
            await SaveAsync(dict.Tracker);

            var dict2 = await LoadAsync();
            AssertEqualDictionary(dict, dict2);
        }
    }
}
