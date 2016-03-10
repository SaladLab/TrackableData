using System;
using ProtoBuf;

namespace Model
{
    [ProtoContract]
    public class UserItem
    {
        [ProtoMember(1)] public DateTime Time { get; internal set; }
        [ProtoMember(2)] public int CharacterId { get; internal set; }
        [ProtoMember(3)] public byte Flag { get; internal set; }
        [ProtoMember(4)] public short Level { get; internal set; }
        [ProtoMember(5)] public int Exp { get; internal set; }
        [ProtoMember(6)] public short PowerLevel { get; internal set; }
        [ProtoMember(7)] public byte Transcendence { get; internal set; }
        [ProtoMember(8)] public int TranscendenceExp { get; internal set; }
        [ProtoMember(9)] public int AwakeningData { get; internal set; }
    }
}
