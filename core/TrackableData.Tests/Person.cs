﻿namespace TrackableData.Tests
{
    public interface IPerson : ITrackablePoco<IPerson>
    {
        string Name { get; set; }
        int Age { get; set; }
        TrackableHand LeftHand { get; set; }
        TrackableHand RightHand { get; set; }
    }

    public interface IHand : ITrackablePoco<IHand>
    {
        TrackableRing MainRing { get; set; }
        TrackableRing SubRing { get; set; }
    }

    public interface IRing : ITrackablePoco<IRing>
    {
        string Name { get; set; }
        int Power { get; set; }
    }
}
