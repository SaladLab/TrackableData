namespace TrackableData.SqlTestKits
{
    public interface ITestPocoWithIdentity : ITrackablePoco<ITestPocoWithIdentity>
    {
        [TrackableProperty("sql.primary-key", "sql.identity")] int Id { get; set; }
        string Name { get; set; }
        int Age { get; set; }
    }
}
