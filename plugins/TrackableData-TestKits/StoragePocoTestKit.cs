using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace TrackableData.TestKits
{
    public abstract class StoragePocoTestKit<TTrackablePoco, TId>
        where TTrackablePoco : ITrackablePoco, new()
    {
        protected abstract Task CreateAsync(TTrackablePoco person);
        protected abstract Task<int> DeleteAsync(TId id);
        protected abstract Task<TTrackablePoco> LoadAsync(TId id);
        protected abstract Task SaveAsync(ITracker tracker, TId id);
        
        [Fact]
        public async Task Test_CreateAndLoad()
        {
            dynamic person = new TTrackablePoco();
            person.Id = default(TId);
            person.Name = "Testor";
            person.Age = 10;
            await CreateAsync(person);

            var person2 = await LoadAsync(person.Id);
            Assert.Equal(person.Id, person2.Id);
            Assert.Equal(person.Name, person2.Name);
            Assert.Equal(person.Age, person2.Age);
        }

        [Fact]
        public async Task Test_Delete()
        {
            dynamic person = new TTrackablePoco();
            person.Id = default(TId);

            await CreateAsync(person);

            var count = await DeleteAsync(person.Id);
            var person2 = await LoadAsync(person.Id);

            Assert.Equal(1, count);
            Assert.Equal(null, person2);
        }

        [Fact]
        public async Task Test_Save()
        {
            dynamic person = new TTrackablePoco();
            person.Id = default(TId);
            person.Name = "Alice";
            await CreateAsync(person);

            ((ITrackable)person).SetDefaultTracker();
            person.Name = "Testor";
            person.Age = 10;
            await SaveAsync(person.Tracker, person.Id);

            var person2 = await LoadAsync(person.Id);
            Assert.Equal(person.Id, person2.Id);
            Assert.Equal(person.Name, person2.Name);
            Assert.Equal(person.Age, person2.Age);
        }
    }
}
