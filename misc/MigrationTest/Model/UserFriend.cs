using System;
using Newtonsoft.Json;
using ProtoBuf;
using TrackableData;

namespace Model
{
    [ProtoContract]
    public class UserFriend
    {
        [ProtoMember(1)] public DateTime CoplayLastTime { get; set; }
        [ProtoMember(2)] public short CoplayCoolTime { get; set; }
        [ProtoMember(3)] public byte Flag { get; set; }
        [ProtoMember(4)] public DateTime RegisterTime { get; set; }
    }
}
