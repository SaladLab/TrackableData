using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        protected abstract TKey CreateKey(int value);
        protected abstract Task<TrackableDictionary<TKey, ItemData>> LoadAsync();
        protected abstract Task SaveAsync(ITracker tracker);

        private TrackableDictionary<TKey, ItemData> CreateTestDictionary(bool withTracker)
        {
            var dict = new TrackableDictionary<TKey, ItemData>();
            if (withTracker)
                dict.SetDefaultTracker();

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

        private void AssertEqualDictionary(TrackableDictionary<TKey, ItemData> a, TrackableDictionary<TKey, ItemData> b)
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
            var dict = CreateTestDictionary(true);
            await SaveAsync(dict.Tracker);

            var dict2 = await LoadAsync();
            AssertEqualDictionary(dict, dict2);
        }

        [Fact]
        public async Task Test_Save()
        {
            var dict = CreateTestDictionary(true);

            await SaveAsync(dict.Tracker);
            dict.Tracker.Clear();

            // modify dictionary

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

            await SaveAsync(dict.Tracker);
            dict.Tracker.Clear();

            // check equality

            var dict2 = await LoadAsync();
            AssertEqualDictionary(dict, dict2);
        }
    }
}
