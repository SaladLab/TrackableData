using System.Collections.Generic;
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
        private bool _useDuplicateCheck;

        protected abstract Task CreateAsync(IList<JobData> list);
        protected abstract Task<int> DeleteAsync();
        protected abstract Task<TrackableList<JobData>> LoadAsync();
        protected abstract Task SaveAsync(TrackableList<JobData> list);

        protected StorageListDataTestKit(bool useDuplicateCheck = false)
        {
            _useDuplicateCheck = useDuplicateCheck;
        }

        private TrackableList<JobData> CreateTestList()
        {
            var list = new TrackableList<JobData>();

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

        private void AssertEqual(TrackableList<JobData> a, TrackableList<JobData> b)
        {
            Assert.Equal(a.Count, b.Count);
            for (int i = 0; i < a.Count; i++)
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
            var list = CreateTestList();
            await CreateAsync(list);

            var list2 = await LoadAsync();
            AssertEqual(list, list2);
        }

        [Fact]
        public async Task Test_CreateAndCreate_DuplicateError()
        {
            if (_useDuplicateCheck == false)
                return;

            var list = CreateTestList();
            await CreateAsync(list);
            var e = await Record.ExceptionAsync(async () => await CreateAsync(list));
            Assert.NotNull(e);
        }

        [Fact]
        public async Task Test_Delete()
        {
            var list = CreateTestList();
            await CreateAsync(list);

            var count = await DeleteAsync();
            var list2 = await LoadAsync();

            Assert.Equal(1, count);
            Assert.Equal(null, list2);
        }

        [Fact]
        public async Task Test_Save()
        {
            var list = CreateTestList();
            await CreateAsync(list);

            list.SetDefaultTracker();
            ModifyListForTest(list);
            await SaveAsync(list);

            var list2 = await LoadAsync();
            AssertEqual(list, list2);
        }
    }
}
