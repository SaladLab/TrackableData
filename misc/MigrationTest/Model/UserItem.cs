using System;
using ProtoBuf;

namespace Model
{
    [ProtoContract]
    public class UserItem
    {
        [ProtoMember(1)] public DateTime Time;
        [ProtoMember(2)] public int CharacterId;
        [ProtoMember(3)] public byte Flag;
        [ProtoMember(4)] public short Level;
        [ProtoMember(5)] public int Exp;
        [ProtoMember(6)] public short PowerLevel;
        [ProtoMember(7)] public byte Transcendence;
        [ProtoMember(8)] public int AwakeningData;
    }
}