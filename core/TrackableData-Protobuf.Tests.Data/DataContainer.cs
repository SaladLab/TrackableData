using System;
using System.Collections.Generic;
using ProtoBuf;

namespace TrackableData.Protobuf.Tests.Data
{
    [ProtoContract]
    public class DataContainer : ITrackableContainer
    {
        [ProtoMember(1)] public virtual Person Person { get; set; }
        [ProtoMember(2)] public virtual IDictionary<int, string> Dictionary { get; set; }
        [ProtoMember(3)] public virtual IList<string> List { get; set; }
    }
}
