using System;

namespace TrackableData.SqlTestKits
{
    public interface ITestTypeNullablePoco : ITrackablePoco<ITestTypeNullablePoco>
    {
        [TrackableProperty("sql.primary-key", "sql.identity")] int Id { get; set; }
        bool? vBool { get; set; }
        byte? vByte { get; set; }
        short? vShort { get; set; }
        char? vChar { get; set; }
        int? vInt { get; set; }
        long? vLong { get; set; }
        float? vFloat { get; set; }
        double? vDouble { get; set; }
        decimal? vDecimal { get; set; }
        DateTime? vDateTime { get; set; }
        DateTimeOffset? vDateTimeOffset { get; set; }
        TimeSpan? vTimeSpan { get; set; }
        Guid? vGuid { get; set; }
    }
}
