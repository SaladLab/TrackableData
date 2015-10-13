using ProtoBuf;

namespace TrackableData.Protobuf.Tests.Data
{
    [ProtoContract]
    public interface IPerson : ITrackablePoco
    {
        [ProtoMember(1)] string Name { get; set; }
        [ProtoMember(2)] int Age { get; set; }
        [ProtoMember(3)] IHand LeftHand { get; set; }
        [ProtoMember(4)] IHand RightHand { get; set; }
    }

    [ProtoContract]
    public interface IHand : ITrackablePoco
    {
        [ProtoMember(1)] IRing MainRing { get; set; }
        [ProtoMember(2)] IRing SubRing { get; set; }
    }

    [ProtoContract]
    public interface IRing : ITrackablePoco
    {
        [ProtoMember(1)] string Name { get; set; }
        [ProtoMember(2)] int Power { get; set; }
    }
}
