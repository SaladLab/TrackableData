using ProtoBuf;

namespace Model
{
    [ProtoContract]
    public class UserMission
    {
        [ProtoMember(1)] public int State { get; internal set; }
        [ProtoMember(2)] public int Progress { get; internal set; }
    }
}
