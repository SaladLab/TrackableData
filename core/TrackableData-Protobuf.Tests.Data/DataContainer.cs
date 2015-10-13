using System;
using System.Collections.Generic;
using ProtoBuf;

namespace TrackableData.Protobuf.Tests.Data
{
    [ProtoContract]
    public interface IDataContainer : ITrackableContainer
    {
        [ProtoMember(1)] IPerson Person { get; set; }
        [ProtoMember(2)] IDictionary<int, string> Dictionary { get; set; }
        [ProtoMember(3)] IList<string> List { get; set; }
    }
}
