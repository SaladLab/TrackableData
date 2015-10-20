using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace TrackableData.TestKits
{
    public class JobData
    {
        public short Kind { get; set; }
        public int Count { get; set; }
        public string Note { get; set; }
    }

    public abstract class StorageListDataTestKit
    {
        protected abstract Task CreateAsync(IList<JobData> list);
        protected abstract Task<int> DeleteAsync();
        protected abstract Task<TrackableList<JobData>> LoadAsync();
        protected abstract Task SaveAsync(ITracker tracker);

        private TrackableList<JobData> CreateTestList(bool withTracker)
        {
            var list = new TrackableList<JobData>();
            if (withTracker)
                list.SetDefaultTracker();

            var value1 = new JobData();
            value1.Kind = 101;
            value1.Count = 1;
            value1.Note = "Handmade Sword";
            list.Add(value1);

            var value2 = new JobData();
            value2.Kind = 102;
            value2.Count = 3;
            value2.Note = "Lord of Ring";
            list.Add(value2);

            return list;
        }

        private void ModifyListForTest(IList<JobData> list)
        {
            list.RemoveAt(0);

            var item2 = list[0];
            var value2 = new JobData();
            value2.Kind = item2.Kind;
            value2.Count = item2.Count - 1;
            value2.Note = "Destroyed";
            list[0] = value2;

            var value3 = new JobData();
            value3.Kind = 102;
            value3.Count = 3;
            value3.Note = "Just Arrived";
            list.Add(value3);
        }

        private List<JobData> GetModifiedList()
        {
            var list = new List<JobData>(CreateTestList(false));
            ModifyListForTest(list);
            return list;
        }

        private void AssertEqualDictionary(TrackableList<JobData> a, TrackableList<JobData> b)
        {
            Assert.Equal(a.Count, b.Count);
            for (int i=0; i<a.Count; i++)
            {
                var a_v = a[i];
                var b_v = b[i];
                Assert.Equal(a_v.Kind, b_v.Kind);
                Assert.Equal(a_v.Count, b_v.Count);
                Assert.Equal(a_v.Note, b_v.Note);
            }
        }

        [Fact]
        public async Task Test_CreateAndLoad()
        {
            var list = CreateTestList(false);
            await CreateAsync(list);

            var list2 = await LoadAsync();
            AssertEqualDictionary(list, list2);
        }

        [Fact]
        public async Task Test_Delete()
        {
            var list = CreateTestList(false);
            await CreateAsync(list);

            var count = await DeleteAsync();
            var list2 = await LoadAsync();

            Assert.Equal(1, count);
            Assert.Equal(null, list2);
        }

        [Fact]
        public async Task Test_Save()
        {
            var list = CreateTestList(true);
            await SaveAsync(list.Tracker);
            list.Tracker.Clear();

            ModifyListForTest(list);
            await SaveAsync(list.Tracker);

            var list2 = await LoadAsync();
            AssertEqualDictionary(list, list2);
        }
    }
}
