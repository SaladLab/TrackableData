using System.Threading.Tasks;
using Xunit;

namespace TrackableData.TestKits
{
    public abstract class StoragePocoTestKit<TTrackablePoco, TId>
        where TTrackablePoco : ITrackablePoco, new()
    {
        private bool _useDuplicateCheck;

        protected abstract Task CreateAsync(TTrackablePoco person);
        protected abstract Task<int> DeleteAsync(TId id);
        protected abstract Task<TTrackablePoco> LoadAsync(TId id);
        protected abstract Task SaveAsync(TTrackablePoco person, TId id);

        protected StoragePocoTestKit(bool useDuplicateCheck = false)
        {
            _useDuplicateCheck = useDuplicateCheck;
        }

        private TTrackablePoco CreateTestPoco()
        {
            dynamic person = new TTrackablePoco();
            person.Id = default(TId);
            person.Name = "Testor";
            person.Age = 10;
            return person;
        }

        private void AssertEqualPoco(dynamic a, dynamic b)
        {
            Assert.Equal(a.Id, b.Id);
            Assert.Equal(a.Name, b.Name);
            Assert.Equal(a.Age, b.Age);
        }

        [Fact]
        public async Task Test_CreateAndLoad()
        {
            dynamic person = CreateTestPoco();
            await CreateAsync(person);

            var person2 = await LoadAsync(person.Id);
            AssertEqualPoco(person, person2);
        }

        [Fact]
        public async Task Test_CreateAndCreate_DuplicateError()
        {
            if (_useDuplicateCheck == false)
                return;

            dynamic person = CreateTestPoco();
            await CreateAsync(person);
            var e = await Record.ExceptionAsync(async () => await CreateAsync(person));
            Assert.NotNull(e);
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
            dynamic person = CreateTestPoco();
            await CreateAsync(person);

            ((ITrackable)person).SetDefaultTracker();
            person.Name = "Alice";
            person.Age = 11;
            await SaveAsync(person, person.Id);

            var person2 = await LoadAsync(person.Id);
            AssertEqualPoco(person, person2);
        }
    }
}
