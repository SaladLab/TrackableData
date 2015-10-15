namespace TrackableData.Json.Tests
{
    public interface IDataContainer : ITrackableContainer
    {
        TrackablePerson Person { get; set; }
        TrackableDictionary<int, string> Dictionary { get; set; }
        TrackableList<string> List { get; set; }
    }
}
