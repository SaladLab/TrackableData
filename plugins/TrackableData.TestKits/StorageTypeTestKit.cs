using System;
using System.Threading.Tasks;
using Xunit;

namespace TrackableData.TestKits
{
    public enum TestEnum : byte
    {
        Spade = 1,
        Heart,
        Diamond,
        Club
    }

    public interface ITypeTestPoco : ITrackablePoco<ITypeTestPoco>
    {
        int Id { get; set; }
        bool ValBool { get; set; }
        byte ValByte { get; set; }
        short ValShort { get; set; }
        char ValChar { get; set; }
        int ValInt { get; set; }
        long ValLong { get; set; }
        float ValFloat { get; set; }
        double ValDouble { get; set; }
        decimal ValDecimal { get; set; }
        DateTime ValDateTime { get; set; }
        DateTimeOffset ValDateTimeOffset { get; set; }
        TimeSpan ValTimeSpan { get; set; }
        string ValString { get; set; }
        byte[] ValBytes { get; set; }
        Guid ValGuid { get; set; }
        TestEnum ValEnum { get; set; }
    }

    public abstract class StorageTypeTestKit
    {
        protected abstract Task CreateAsync(TrackableTypeTestPoco data);
        protected abstract Task<TrackableTypeTestPoco> LoadAsync(int id);
        protected abstract Task SaveAsync(TrackableTypeTestPoco data);

        protected virtual void OnDataInitialized(TrackableTypeTestPoco data) { }

        protected StorageTypeTestKit()
        {
        }

        private void AssertEqual(TrackableTypeTestPoco p0, TrackableTypeTestPoco p1)
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

        private void SetIdentity(TrackableTypeTestPoco p0)
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
        public async Task Test_CreateAsZero_LoadAndCheckEqual()
        {
            var p0 = new TrackableTypeTestPoco();
            p0.Id = 1;

            OnDataInitialized(p0);
            await CreateAsync(p0);

            var p1 = await LoadAsync(p0.Id);
            AssertEqual(p0, p1);
        }

        [Fact]
        public async Task Test_CreateAsIdentity_LoadAndCheckEqual()
        {
            var p0 = new TrackableTypeTestPoco();
            p0.Id = 2;
            SetIdentity(p0);

            OnDataInitialized(p0);
            await CreateAsync(p0);

            var p1 = await LoadAsync(p0.Id);
            AssertEqual(p0, p1);
        }

        [Fact]
        public async Task Test_CreateAsMinimum_LoadAndCheckEqual()
        {
            var p0 = new TrackableTypeTestPoco();
            p0.Id = 3;
            p0.ValBool = false;
            p0.ValByte = byte.MinValue;
            p0.ValShort = short.MinValue;
            p0.ValChar = char.MinValue;
            p0.ValInt = int.MinValue;
            p0.ValLong = long.MinValue;
            p0.ValFloat = float.MinValue;
            p0.ValDouble = double.MinValue;
            p0.ValDecimal = decimal.MinValue;
            p0.ValDateTime = DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);
            p0.ValDateTimeOffset = DateTimeOffset.MinValue;
            p0.ValTimeSpan = TimeSpan.MinValue;
            p0.ValString = "0";
            p0.ValBytes = new byte[] { 0 };
            p0.ValGuid = Guid.Empty;
            p0.ValEnum = (TestEnum)0;

            OnDataInitialized(p0);
            await CreateAsync(p0);

            var p1 = await LoadAsync(p0.Id);
            AssertEqual(p0, p1);
        }

        [Fact]
        public async Task Test_CreateAsMaximum_LoadAndCheckEqual()
        {
            var p0 = new TrackableTypeTestPoco();
            p0.Id = 4;
            p0.ValBool = true;
            p0.ValByte = byte.MaxValue;
            p0.ValShort = short.MaxValue;
            p0.ValChar = char.MaxValue;
            p0.ValInt = int.MaxValue;
            p0.ValLong = long.MaxValue;
            p0.ValFloat = float.MaxValue;
            p0.ValDouble = double.MaxValue;
            p0.ValDecimal = decimal.MaxValue;
            p0.ValDateTime = DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Utc);
            p0.ValDateTimeOffset = DateTimeOffset.MaxValue;
            p0.ValTimeSpan = TimeSpan.MaxValue;
            p0.ValString = "\xAC00\xD7A3";
            p0.ValBytes = new byte[] { 0, 127, 255 };
            p0.ValGuid = new Guid(0xFFFFFFFF, 0xFFFF, 0xFFFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF);
            p0.ValEnum = (TestEnum)0xFF;

            OnDataInitialized(p0);
            await CreateAsync(p0);

            var p1 = await LoadAsync(p0.Id);
            AssertEqual(p0, p1);
        }

        [Fact]
        public async Task Test_CreateAsZero_SaveAsIdentity()
        {
            var p0 = new TrackableTypeTestPoco();
            p0.Id = 5;

            OnDataInitialized(p0);
            await CreateAsync(p0);

            p0.SetDefaultTracker();
            SetIdentity(p0);
            OnDataInitialized(p0);
            await SaveAsync(p0);

            var p1 = await LoadAsync(p0.Id);
            AssertEqual(p0, p1);
        }
    }
}
