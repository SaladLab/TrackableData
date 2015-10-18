using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace TrackableData.TestKits
{
    public abstract class StorageListPocoTestKit<TPoco> where TPoco : ITrackable, new ()
    {
        protected abstract Task<TrackableList<TPoco>> LoadAsync();
        protected abstract Task SaveAsync(TrackableList<TPoco> list);

        private TrackableList<TPoco> CreateTestList(bool withTracker)
        {
            var list = new TrackableList<TPoco>();
            if (withTracker)
                list.SetDefaultTracker();

            dynamic value1 = new TPoco();
            value1.Kind = 101;
            value1.Count = 1;
            value1.Note = "Handmade Sword";
            list.Add(value1);

            dynamic value2 = new TPoco();
            value2.Kind = 102;
            value2.Count = 3;
            value2.Note = "Lord of Ring";
            list.Add(value2);

            return list;
        }

        private void ModifyListForTest(IList<TPoco> list)
        {
            list.RemoveAt(0);

            dynamic item2 = list[0];
            item2.Count = item2.Count - 1;
            item2.Note = "Destroyed";

            dynamic value3 = new TPoco();
            value3.Kind = 102;
            value3.Count = 3;
            value3.Note = "Just Arrived";
            ((ICollection<TPoco>)list).Add(value3);
        }

        private List<TPoco> GetModifiedList()
        {
            var list = new List<TPoco>(CreateTestList(false));
            ModifyListForTest(list);
            return list;
        }

        private void AssertEqualDictionary(TrackableList<TPoco> a, TrackableList<TPoco> b)
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
