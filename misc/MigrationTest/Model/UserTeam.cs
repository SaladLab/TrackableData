using ProtoBuf;

namespace Model
{
    [ProtoContract]
    public class UserTeam
    {
        [ProtoMember(1)] public string Name { get; internal set; }
        [ProtoMember(2)] public int Member0 { get; internal set; }
        [ProtoMember(3)] public int Member1 { get; internal set; }
        [ProtoMember(4)] public int Member2 { get; internal set; }
        [ProtoMember(5)] public int Member3 { get; internal set; }
    }
}
