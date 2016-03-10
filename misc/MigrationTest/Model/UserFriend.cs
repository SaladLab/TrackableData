using System;
using ProtoBuf;

namespace Model
{
    [ProtoContract]
    public class UserFriend
    {
        [ProtoMember(1)] public DateTime CoplayLastTime { get; internal set; }
        [ProtoMember(2)] public short CoplayCoolTime { get; internal set; }
        [ProtoMember(3)] public DateTime RegisterTime { get; internal set; }
        [ProtoMember(4)] public byte RewardCount { get; internal set; }
    }
}
