using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace TrackableData.TestKits
{
    public abstract class StorageSetValueTestKit
    {
        protected abstract Task CreateAsync(ICollection<int> set);
        protected abstract Task<int> DeleteAsync();
        protected abstract Task<TrackableSet<int>> LoadAsync();
        protected abstract Task SaveAsync(TrackableSet<int> set);

        private TrackableSet<int> CreateTestSet(bool withTracker)
        {
            var set = new TrackableSet<int>();
            if (withTracker)
                set.SetDefaultTracker();
            set.Add(1);
            set.Add(2);
            set.Add(3);
            return set;
        }

        private void ModifySetForTest(ICollection<int> set)
        {
            set.Remove(1);
            set.Remove(2);
            set.Add(4);
            set.Add(5);
        }

        [Fact]
        public async Task Test_CreateAndLoad()
        {
            var set = CreateTestSet(false);
            await CreateAsync(set);

            var set2 = await LoadAsync();
            Assert.Equal(set.OrderBy(x => x), set2.OrderBy(x => x));
        }

        [Fact]
        public async Task Test_Delete()
        {
            var set = CreateTestSet(false);
            await CreateAsync(set);

            var count = await DeleteAsync();
            var set2 = await LoadAsync();

            Assert.True(count > 0);
            Assert.True(set2 == null || set2.Count == 0);
        }

        [Fact]
        public async Task Test_Save()
        {
            var set = CreateTestSet(true);
            await SaveAsync(set);
            set.Tracker.Clear();

            ModifySetForTest(set);
            await SaveAsync(set);

            var set2 = await LoadAsync();
            Assert.Equal(set.OrderBy(x => x), set2.OrderBy(x => x));
        }
    }
}
