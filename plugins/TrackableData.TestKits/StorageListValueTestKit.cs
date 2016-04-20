using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace TrackableData.TestKits
{
    public abstract class StorageListValueTestKit
    {
        private bool _useDuplicateCheck;

        protected abstract Task CreateAsync(IList<string> list);
        protected abstract Task<int> DeleteAsync();
        protected abstract Task<TrackableList<string>> LoadAsync();
        protected abstract Task SaveAsync(TrackableList<string> list);

        protected StorageListValueTestKit(bool useDuplicateCheck = false)
        {
            _useDuplicateCheck = useDuplicateCheck;
        }

        private TrackableList<string> CreateTestList()
        {
            var list = new TrackableList<string>();
            list.Add("One");
            list.Add("Two");
            list.Add("Three");
            return list;
        }

        private void ModifyListForTest(IList<string> list)
        {
            list[0] = "OneModified";
            list.Insert(0, "Zero");
            list.RemoveAt(0);
            list.Insert(0, "ZeroAgain");
            list.Insert(4, "Four");
            list.RemoveAt(4);
            list.Insert(4, "FourAgain");
        }

        [Fact]
        public async Task Test_CreateAndLoad()
        {
            var list = CreateTestList();
            await CreateAsync(list);

            var list2 = await LoadAsync();
            Assert.Equal(list, list2);
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
            Assert.Equal(list, list2);
        }
    }
}
