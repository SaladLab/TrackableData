using System;
using Newtonsoft.Json;
using ProtoBuf;
using TrackableData;

namespace Model
{
    [ProtoContract]
    public class UserPost
    {
        [ProtoMember(1)] public int SenderUid { get; set; }
        [ProtoMember(2)] public int SendReason { get; set; }
        [ProtoMember(3)] public DateTime SentTime { get; set; }
        [ProtoMember(4)] public DateTime ExpireTime { get; set; }
        [ProtoMember(5)] public byte Type { get; set; }
        [ProtoMember(6)] public int Value { get; set; }
        [ProtoMember(7)] public int ValueExtra { get; set; }
        [ProtoMember(8)] public string Comment { get; set; }
        [ProtoMember(9)] public int CommentType { get; set; }
        [ProtoMember(10)] public int CommentValue { get; set; }
    }
}