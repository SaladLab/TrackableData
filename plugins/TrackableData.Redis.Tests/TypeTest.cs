using System.Threading.Tasks;
using StackExchange.Redis;
using TrackableData.TestKits;
using Xunit;

namespace TrackableData.Redis.Tests
{
    public class TypeTest : StorageTypeTestKit, IClassFixture<Redis>
    {
        private static TrackablePocoRedisMapper<ITypeTestPoco> _mapper =
           new TrackablePocoRedisMapper<ITypeTestPoco>();

        private IDatabase _db;

        public TypeTest(Redis redis)
        {
            _db = redis.Db;
        }

        protected override Task CreateAsync(TrackableTypeTestPoco data)
        {
            return _mapper.CreateAsync(_db, data, $"TypeTest:{data.Id}");
        }

        protected override async Task<TrackableTypeTestPoco> LoadAsync(int id)
        {
            return (TrackableTypeTestPoco)(await _mapper.LoadAsync(_db, $"TypeTest:{id}"));
        }

        protected override Task SaveAsync(TrackableTypeTestPoco data)
        {
            return _mapper.SaveAsync(_db, data.Tracker, $"TypeTest:{data.Id}");
        }

        protected override void OnDataInitialized(TrackableTypeTestPoco data)
        {
            base.OnDataInitialized(data);

            if (data.ValInt == int.MinValue)
            {
                // workaround overflow bug
                data.ValLong += 1;

                // just arbitrary value for float, double, decimal
                data.ValFloat = -9999.99f;
                data.ValDouble = -99999.99;
                data.ValDecimal = -999999.99m;
            }
            else if (data.ValInt == int.MaxValue)
            {
                // just arbitrary value for float, double, decimal
                data.ValFloat = 9999.99f;
                data.ValDouble = 99999.99;
                data.ValDecimal = 999999.99m;
            }
        }
    }

    public class TypeNullableTest : StorageTypeNullableTestKit, IClassFixture<Redis>
    {
        private static TrackablePocoRedisMapper<ITypeNullableTestPoco> _mapper =
            new TrackablePocoRedisMapper<ITypeNullableTestPoco>();

        private IDatabase _db;

        public TypeNullableTest(Redis redis)
        {
            _db = redis.Db;
        }

        protected override Task CreateAsync(TrackableTypeNullableTestPoco data)
        {
            return _mapper.CreateAsync(_db, data, $"TypeNullableTest:{data.Id}");
        }

        protected override async Task<TrackableTypeNullableTestPoco> LoadAsync(int id)
        {
            return (TrackableTypeNullableTestPoco)(await _mapper.LoadAsync(_db, $"TypeNullableTest:{id}"));
        }

        protected override Task SaveAsync(TrackableTypeNullableTestPoco data)
        {
            return _mapper.SaveAsync(_db, data.Tracker, $"TypeNullableTest:{data.Id}");
        }
    }
}
