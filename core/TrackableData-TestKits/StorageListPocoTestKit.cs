using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace TrackableData.TestKits
{
    public abstract class StorageListPocoTestKit<TTrackablePoco> where TTrackablePoco : ITrackable, new ()
    {
        protected abstract Task<TrackableList<TTrackablePoco>> LoadAsync();
        protected abstract Task SaveAsync(TrackableList<TTrackablePoco> list);

        private TrackableList<TTrackablePoco> CreateTestList(bool withTracker)
        {
            var list = new TrackableList<TTrackablePoco>();
            if (withTracker)
                list.SetDefaultTracker();

            dynamic value1 = new TTrackablePoco();
            value1.Kind = 101;
            value1.Count = 1;
            value1.Note = "Handmade Sword";
            list.Add(value1);

            dynamic value2 = new TTrackablePoco();
            value2.Kind = 102;
            value2.Count = 3;
            value2.Note = "Lord of Ring";
            list.Add(value2);

            return list;
        }

        private void ModifyListForTest(IList<TTrackablePoco> list)
        {
            list.RemoveAt(0);

            dynamic item2 = list[0];
            item2.Count = item2.Count - 1;
            item2.Note = "Destroyed";

            dynamic value3 = new TTrackablePoco();
            value3.Kind = 102;
            value3.Count = 3;
            value3.Note = "Just Arrived";
            ((ICollection<TTrackablePoco>)list).Add(value3);
        }

        private List<TTrackablePoco> GetModifiedList()
        {
            var list = new List<TTrackablePoco>(CreateTestList(false));
            ModifyListForTest(list);
            return list;
        }

        private void AssertEqualDictionary(TrackableList<TTrackablePoco> a, TrackableList<TTrackablePoco> b)
        {
            Assert.Equal(a.Count, b.Count);
            for (int i = 0; i < a.Count; i++)
            {
                dynamic a_v = a[i];
                dynamic b_v = b[i];
                Assert.Equal(a_v.Kind, b_v.Kind);
                Assert.Equal(a_v.Count, b_v.Count);
                Assert.Equal(a_v.Note, b_v.Note);
            }
        }

        [Fact]
        public async Task Test_CreateAndLoad()
        {
            var list = CreateTestList(true);
            await SaveAsync(list);

            var list2 = await LoadAsync();
            AssertEqualDictionary(list, list2);
        }

        [Fact]
        public async Task Test_Save()
        {
            var list = CreateTestList(true);
            await SaveAsync(list);
            list.ClearTrackerDeep();

            ModifyListForTest(list);
            await SaveAsync(list);

            var list2 = await LoadAsync();
            AssertEqualDictionary(list, list2);
        }
    }
}
