using TrackableData.TestKits;

namespace TrackableData.SqlTestKits
{
    public interface ITestContainer : ITrackableContainer<ITestContainer>
    {
        TrackableTestPocoForContainer Person { get; set; }
        TrackableDictionary<int, MissionData> Missions { get; set; }
    }
}