using System;
using Newtonsoft.Json;
using ProtoBuf;

namespace Model
{
    [ProtoContract]
    public class UserMission
    {
        [ProtoMember(1)] public int MissionId { get; set; }
        [ProtoMember(2)] public int Progress { get; set; }
        [ProtoMember(3)] public bool RewardReceived { get; set; }
    }
}