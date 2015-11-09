using ProtoBuf;

namespace TrackableData.Protobuf.Tests
{
    [ProtoContract]
    public interface IPerson : ITrackablePoco<IPerson>
    {
        [ProtoMember(1)]
        string Name { get; set; }

        [ProtoMember(2)]
        int Age { get; set; }

        [ProtoMember(3)]
        TrackableHand LeftHand { get; set; }

        [ProtoMember(4)]
        TrackableHand RightHand { get; set; }
    }

    [ProtoContract]
    public interface IHand : ITrackablePoco<IHand>
    {
        [ProtoMember(1)]
        TrackableRing MainRing { get; set; }

        [ProtoMember(2)]
        TrackableRing SubRing { get; set; }
    }

    [ProtoContract]
    public interface IRing : ITrackablePoco<IRing>
    {
        [ProtoMember(1)]
        string Name { get; set; }

        [ProtoMember(2)]
        int Power { get; set; }
    }
}
