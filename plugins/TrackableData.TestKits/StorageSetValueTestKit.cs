﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace TrackableData.TestKits
{
    public abstract class StorageSetValueTestKit
    {
        private bool _useDuplicateCheck;

        protected abstract Task CreateAsync(ICollection<int> set);
        protected abstract Task<int> DeleteAsync();
        protected abstract Task<TrackableSet<int>> LoadAsync();
        protected abstract Task SaveAsync(TrackableSet<int> set);

        protected StorageSetValueTestKit(bool useDuplicateCheck = false)
        {
            _useDuplicateCheck = useDuplicateCheck;
        }

        private TrackableSet<int> CreateTestSet()
        {
            var set = new TrackableSet<int>();
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
            var set = CreateTestSet();
            await CreateAsync(set);

            var set2 = await LoadAsync();
            Assert.Equal(set.OrderBy(x => x), set2.OrderBy(x => x));
        }

        [Fact]
        public async Task Test_CreateAndCreate_DuplicateError()
        {
            if (_useDuplicateCheck == false)
                return;

            var set = CreateTestSet();
            await CreateAsync(set);
            var e = await Record.ExceptionAsync(async () => await CreateAsync(set));
            Assert.NotNull(e);
        }

        [Fact]
        public async Task Test_Delete()
        {
            var set = CreateTestSet();
            await CreateAsync(set);

            var count = await DeleteAsync();
            var set2 = await LoadAsync();

            Assert.True(count > 0);
            Assert.True(set2 == null || set2.Count == 0);
        }

        [Fact]
        public async Task Test_Save()
        {
            var set = CreateTestSet();
            await CreateAsync(set);

            set.SetDefaultTracker();
            ModifySetForTest(set);
            await SaveAsync(set);

            var set2 = await LoadAsync();
            Assert.Equal(set.OrderBy(x => x), set2.OrderBy(x => x));
        }
    }
}
