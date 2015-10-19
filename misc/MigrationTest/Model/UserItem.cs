using System;
using ProtoBuf;

namespace Model
{
    [ProtoContract]
    public class UserItem
    {
        [ProtoMember(1)] public DateTime Time { get; set; }
        [ProtoMember(2)] public int CharacterId { get; set; }
        [ProtoMember(3)] public byte Flag { get; set; }
        [ProtoMember(4)] public short Level { get; set; }
        [ProtoMember(5)] public int Exp { get; set; }
        [ProtoMember(6)] public short PowerLevel { get; set; }
        [ProtoMember(7)] public byte Transcendence { get; set; }
        [ProtoMember(8)] public int AwakeningData { get; set; }
    }
}