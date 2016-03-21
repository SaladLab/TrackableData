﻿using System;
using StackExchange.Redis;
using System.Threading.Tasks;
using TrackableData.TestKits;
using Xunit;

namespace TrackableData.Redis.Tests
{
    public class TrackableContainerHashesTest :
        StorageContainerTestKit<TrackableTestContainer, TrackableTestPocoForContainer>,
        IClassFixture<Redis>
    {
        private static TrackableContainerHashesRedisMapper<ITestContainer> _mapper =
            new TrackableContainerHashesRedisMapper<ITestContainer>();

        private IDatabase _db;
        private string _testId = "TestContainerHashes";

        public TrackableContainerHashesTest(Redis redis)
            : base(true)
        {
            _db = redis.Db;
        }

        protected override Task CreateAsync(TrackableTestContainer container)
        {
            return _mapper.CreateAsync(_db, container, _testId);
        }

        protected override Task<int> DeleteAsync()
        {
            return _mapper.DeleteAsync(_db, _testId);
        }

        protected override async Task<TrackableTestContainer> LoadAsync()
        {
            return (TrackableTestContainer)await _mapper.LoadAsync(_db, _testId);
        }

        protected override Task SaveAsync(TrackableTestContainer container)
        {
            return _mapper.SaveAsync(_db, container, _testId);
        }
    }
}