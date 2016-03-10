using System;
using ProtoBuf;

namespace Model
{
    [ProtoContract]
    public class UserPost
    {
        [ProtoMember(1)] public int SenderUid { get; internal set; }
        [ProtoMember(2)] public int SendReason { get; internal set; }
        [ProtoMember(3)] public DateTime SentTime { get; internal set; }
        [ProtoMember(4)] public DateTime ExpireTime { get; internal set; }
        [ProtoMember(5)] public byte Type { get; internal set; }
        [ProtoMember(6)] public int Value { get; internal set; }
        [ProtoMember(7)] public int ValueExtra { get; internal set; }
        [ProtoMember(8)] public string Comment { get; internal set; }
        [ProtoMember(9)] public int CommentType { get; internal set; }
        [ProtoMember(10)] public int CommentValue { get; internal set; }
    }
}
