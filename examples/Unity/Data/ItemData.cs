using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;
using TrackableData;

namespace Unity.Data
{
    [ProtoContract]
    public class ItemData
    {
        [ProtoMember(1)] int Kind { get; set; }
        [ProtoMember(2)] int Count { get; set; }
        [ProtoMember(3)] string Note { get; set; }
    }
}
