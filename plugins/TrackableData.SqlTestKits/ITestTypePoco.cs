using System;

namespace TrackableData.SqlTestKits
{
    public enum Suit : byte
    {
        Spade = 1,
        Heart,
        Diamond,
        Club
    }

    public interface ITestTypePoco : ITrackablePoco<ITestTypePoco>
    {
        [TrackableProperty("sql.primary-key", "sql.identity")] int Id { get; set; }
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
        Suit ValSuit { get; set; }
    }
}
