using ProtoBuf;
using TrackableData;

namespace Model
{
    [ProtoContract]
    public interface IUserContext : ITrackableContainer
    {
        [ProtoMember(1)] TrackableUserData Data { get; set; }
        [ProtoMember(2)] TrackableDictionary<int, UserItem> Items { get; set; }
    }
}