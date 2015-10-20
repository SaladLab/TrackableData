using System;
using ProtoBuf;
using TrackableData;

namespace Model
{
    [ProtoContract]
    public interface IUserContext : ITrackableContainer<IUserContext>
    {
        [ProtoMember(1)] TrackableUserData Data { get; set; }
        [ProtoMember(2)] TrackableDictionary<int, UserItem> Items { get; set; }
        [ProtoMember(3)] TrackableDictionary<byte, UserTeam> Teams { get; set; }
        [ProtoMember(4)] TrackableDictionary<int, UserTank> Tanks { get; set; }
        [ProtoMember(5)] TrackableDictionary<byte, long> Cards { get; set; }
        [ProtoMember(6)] TrackableDictionary<int, UserFriend> Friends { get; set; }
        [ProtoMember(7)] TrackableDictionary<byte, UserMission> Missions { get; set; }
        [ProtoMember(8)] TrackableDictionary<byte, long> StageGrades { get; set; }
        [ProtoMember(9)] TrackableDictionary<int, UserPost> Posts { get; set; }
    }
}
