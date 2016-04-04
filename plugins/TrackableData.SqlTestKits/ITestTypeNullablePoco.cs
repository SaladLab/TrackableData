using System;

namespace TrackableData.SqlTestKits
{
    public interface ITestTypeNullablePoco : ITrackablePoco<ITestTypeNullablePoco>
    {
        [TrackableProperty("sql.primary-key", "sql.identity")] int Id { get; set; }
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
        Guid? ValGuid { get; set; }
        Suit? ValSuit { get; set; }
    }
}
