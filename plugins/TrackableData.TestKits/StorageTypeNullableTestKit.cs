using System;
using System.Threading.Tasks;
using Xunit;

namespace TrackableData.TestKits
{
    public interface ITypeNullableTestPoco : ITrackablePoco<ITypeNullableTestPoco>
    {
        int Id { get; set; }
        bool? ValBool { get; set; }
        byte? ValByte { get; set; }
        short? ValShort { get; set; }
        char? ValChar { get; set; }
        int? ValInt { get; set; }
        long? ValLong { get; set; }
        float? ValFloat { get; set; }
        double? ValDouble { get; set; }
        decimal? ValDecimal { get; set; }
        DateTime? ValDateTime { get; set; }
        DateTimeOffset? ValDateTimeOffset { get; set; }
        TimeSpan? ValTimeSpan { get; set; }
        string ValString { get; set; }
        byte[] ValBytes { get; set; }
        Guid? ValGuid { get; set; }
        TestEnum? ValEnum { get; set; }
    }

    public abstract class StorageTypeNullableTestKit
    {
        protected abstract Task CreateAsync(TrackableTypeNullableTestPoco data);
        protected abstract Task<TrackableTypeNullableTestPoco> LoadAsync(int id);
        protected abstract Task SaveAsync(TrackableTypeNullableTestPoco data);

        protected virtual void OnDataInitialized(TrackableTypeNullableTestPoco data) { }

        protected StorageTypeNullableTestKit()
        {
        }

        private void AssertEqual(TrackableTypeNullableTestPoco p0, TrackableTypeNullableTestPoco p1)
        {
            Assert.Equal(p0.ValBool, p1.ValBool);
            Assert.Equal(p0.ValByte, p1.ValByte);
            Assert.Equal(p0.ValShort, p1.ValShort);
            Assert.Equal(p0.ValChar, p1.ValChar);
            Assert.Equal(p0.ValInt, p1.ValInt);
            Assert.Equal(p0.ValLong, p1.ValLong);
            Assert.Equal(p0.ValFloat, p1.ValFloat);
            Assert.Equal(p0.ValDouble, p1.ValDouble);
            Assert.Equal(p0.ValDecimal, p1.ValDecimal);
            Assert.Equal(p0.ValDateTime, p1.ValDateTime);
            Assert.Equal(p0.ValDateTimeOffset, p1.ValDateTimeOffset);
            Assert.Equal(p0.ValTimeSpan, p1.ValTimeSpan);
            Assert.Equal(p0.ValString, p1.ValString);
            Assert.Equal(p0.ValBytes, p1.ValBytes);
            Assert.Equal(p0.ValGuid, p1.ValGuid);
            Assert.Equal(p0.ValEnum, p1.ValEnum);
        }

        private void SetNull(TrackableTypeNullableTestPoco p0)
        {
            p0.ValBool = null;
            p0.ValByte = null;
            p0.ValShort = null;
            p0.ValChar = null;
            p0.ValInt = null;
            p0.ValLong = null;
            p0.ValFloat = null;
            p0.ValDouble = null;
            p0.ValDecimal = null;
            p0.ValDateTime = null;
            p0.ValDateTimeOffset = null;
            p0.ValTimeSpan = null;
            p0.ValString = null;
            p0.ValBytes = null;
            p0.ValGuid = null;
            p0.ValEnum = null;
        }

        private void SetIdentity(TrackableTypeNullableTestPoco p0)
        {
            p0.ValBool = true;
            p0.ValByte = 1;
            p0.ValShort = 1;
            p0.ValChar = '\x1';
            p0.ValInt = 1;
            p0.ValLong = 1;
            p0.ValFloat = 1;
            p0.ValDouble = 1;
            p0.ValDecimal = 1;
            p0.ValDateTime = new DateTime(2001, 1, 1, 1, 1, 1, DateTimeKind.Utc);
            p0.ValDateTimeOffset = new DateTimeOffset(2001, 1, 1, 1, 1, 1, TimeSpan.FromHours(1));
            p0.ValTimeSpan = new TimeSpan(1, 1, 1);
            p0.ValString = "1";
            p0.ValBytes = new byte[] { 1 };
            p0.ValGuid = new Guid(1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1);
            p0.ValEnum = TestEnum.Spade;
        }

        [Fact]
        public async Task Test_CreateAsNull_LoadAndCheckEqual()
        {
            var p0 = new TrackableTypeNullableTestPoco();
            p0.Id = 1;

            OnDataInitialized(p0);
            await CreateAsync(p0);

            var p1 = await LoadAsync(p0.Id);
            AssertEqual(p0, p1);
        }

        [Fact]
        public async Task Test_CreateAsIdentity_LoadAndCheckEqual()
        {
            var p0 = new TrackableTypeNullableTestPoco();
            p0.Id = 2;
            SetIdentity(p0);

            OnDataInitialized(p0);
            await CreateAsync(p0);

            var p1 = await LoadAsync(p0.Id);
            AssertEqual(p0, p1);
        }

        [Fact]
        public async Task Test_CreateAsNull_SaveAsIdentity()
        {
            var p0 = new TrackableTypeNullableTestPoco();
            p0.Id = 3;

            OnDataInitialized(p0);
            await CreateAsync(p0);

            p0.SetDefaultTracker();
            SetIdentity(p0);
            OnDataInitialized(p0);
            await SaveAsync(p0);

            var p1 = await LoadAsync(p0.Id);
            AssertEqual(p0, p1);
        }

        [Fact]
        public async Task Test_CreateAsIdentity_SaveAsNull()
        {
            var p0 = new TrackableTypeNullableTestPoco();
            p0.Id = 4;
            SetIdentity(p0);

            OnDataInitialized(p0);
            await CreateAsync(p0);

            p0.SetDefaultTracker();
            SetNull(p0);
            OnDataInitialized(p0);
            await SaveAsync(p0);

            var p1 = await LoadAsync(p0.Id);
            AssertEqual(p0, p1);
        }
    }
}
