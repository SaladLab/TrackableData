using ProtoBuf;

namespace Model
{
    [ProtoContract]
    public class UserTank
    {
        [ProtoMember(1)] public short Level { get; set; }
        [ProtoMember(2)] public short RaisePoint { get; set; }
    }
}
