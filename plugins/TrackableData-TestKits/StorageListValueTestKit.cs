using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace TrackableData.TestKits
{
    public abstract class StorageListValueTestKit
    {
        protected abstract Task CreateAsync(IList<string> list);
        protected abstract Task<int> DeleteAsync();
        protected abstract Task<TrackableList<string>> LoadAsync();
        protected abstract Task SaveAsync(ITracker tracker);

        private TrackableList<string> CreateTestList(bool withTracker)
        {
            var list = new TrackableList<string>();
            if (withTracker)
                list.SetDefaultTracker();
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

        private List<string> GetModifiedList()
        {
            var list = new List<string>(CreateTestList(false));
            ModifyListForTest(list);
            return list;
        }

        [Fact]
        public async Task Test_CreateAndLoad()
        {
            var list = CreateTestList(false);
            await CreateAsync(list);

            var list2 = await LoadAsync();
            Assert.Equal(list, list2);
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
            Assert.Equal(list, list2);
        }
    }
}
