namespace TrackableData.Json.Tests
{
    public interface IDataContainer : ITrackableContainer<IDataContainer>
    {
        TrackablePerson Person { get; set; }
        TrackableDictionary<int, string> Dictionary { get; set; }
        TrackableSet<int> Set { get; set; }
        TrackableList<string> List { get; set; }
    }
}
