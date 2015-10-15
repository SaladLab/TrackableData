namespace TrackableData.Json.Tests
{
    public interface IPerson : ITrackablePoco
    {
        string Name { get; set; }
        int Age { get; set; }
        TrackableHand LeftHand { get; set; }
        TrackableHand RightHand { get; set; }
    }

    public interface IHand : ITrackablePoco
    {
        TrackableRing MainRing { get; set; }
        TrackableRing SubRing { get; set; }
    }

    public interface IRing : ITrackablePoco
    {
        string Name { get; set; }
        int Power { get; set; }
    }
}
