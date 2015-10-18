using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace TrackableData.TestKits
{
    public abstract class StorageDictionaryPocoKit<TKey, TPoco> where TPoco : ITrackable, new()
    {
        protected abstract TKey CreateKey(int value);
        protected abstract Task<TrackableDictionary<TKey, TPoco>> LoadAsync();
        protected abstract Task SaveAsync(TrackableDictionary<TKey, TPoco> dictionary);

        private TrackableDictionary<TKey, TPoco> CreateTestDictionary(bool withTracker)
        {
            var dict = new TrackableDictionary<TKey, TPoco>();
            if (withTracker)
                dict.SetDefaultTracker();

            dynamic value1 = new TPoco();
            if (withTracker)
                ((ITrackable)value1).SetDefaultTracker();
            value1.Kind = 101;
            value1.Count = 1;
            value1.Note = "Handmade Sword";
            dict.Add(CreateKey(1), value1);

            dynamic value2 = new TPoco();
            if (withTracker)
                ((ITrackable)value2).SetDefaultTracker();
            value2.Kind = 102;
            value2.Count = 3;
            value2.Note = "Lord of Ring";
            dict.Add(CreateKey(2), value2);

            return dict;
        }

        private void AssertEqualDictionary(TrackableDictionary<TKey, TPoco> a, TrackableDictionary<TKey, TPoco> b)
        {
            Assert.Equal(a.Count, b.Count);
            foreach (var item in a)
            {
                dynamic a_v = item.Value;
                dynamic b_v = b[item.Key];
                Assert.Equal(a_v.Kind, b_v.Kind);
                Assert.Equal(a_v.Count, b_v.Count);
                Assert.Equal(a_v.Note, b_v.Note);
            }
        }

        [Fact]
        public async Task Test_CreateAndLoad()
        {
            var dict = CreateTestDictionary(true);
            await SaveAsync(dict);

            var dict2 = await LoadAsync();
            AssertEqualDictionary(dict, dict2);
        }

        [Fact]
        public async Task Test_Save()
        {
            var dict = CreateTestDictionary(true);

            await SaveAsync(dict);
            dict.ClearTrackerDeep();

            // modify dictionary

            dict.Remove(CreateKey(1));

            dynamic item2 = dict[CreateKey(2)];
            item2.Count = item2.Count - 1;
            item2.Note = "Destroyed";

            dynamic value3 = new TPoco();
            value3.Kind = 103;
            value3.Count = 13;
            value3.Note = "Just Arrived";
            dict.Add(CreateKey(3), value3);

            // save modification

            await SaveAsync(dict);

            // check equality

            var dict2 = await LoadAsync();
            AssertEqualDictionary(dict, dict2);
        }
    }
}
