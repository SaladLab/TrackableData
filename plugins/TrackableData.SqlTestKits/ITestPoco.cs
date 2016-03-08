namespace TrackableData.SqlTestKits
{
    public interface ITestPoco : ITrackablePoco<ITestPoco>
    {
        [TrackableProperty("sql.primary-key")] int Id { get; set; }
        string Name { get; set; }
        int Age { get; set; }
    }
}
