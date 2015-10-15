using ProtoBuf;

namespace TrackableData.Protobuf.Tests
{
    [ProtoContract]
    public interface IDataContainer : ITrackableContainer
    {
        [ProtoMember(1)]
        TrackablePerson Person { get; set; }

        [ProtoMember(2)]
        TrackableDictionary<int, string> Dictionary { get; set; }

        [ProtoMember(3)]
        TrackableList<string> List { get; set; }
    }
}
