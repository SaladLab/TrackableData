using TrackableData.TestKits;

namespace TrackableData.SqlTestKits
{
    public interface ITestContainerWithIgnore : ITrackableContainer<ITestContainerWithIgnore>
    {
        TrackableTestPocoForContainer Person { get; set; }
        [TrackableProperty("sql.ignore")] TrackableDictionary<int, MissionData> Missions { get; set; }
    }
}
