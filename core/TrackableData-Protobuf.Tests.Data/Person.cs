using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;
using TrackableData;

namespace TrackableData.Protobuf.Tests.Data
{
    [ProtoContract]
    public class Person : ITrackablePoco
    {
        [ProtoMember(1)] public virtual string Name { get; set; }
        [ProtoMember(2)] public virtual int Age { get; set; }
        [ProtoMember(3)] public virtual Hand LeftHand { get; set; }
        [ProtoMember(4)] public virtual Hand RightHand { get; set; }
    }

    [ProtoContract]
    public class Hand : ITrackablePoco
    {
        [ProtoMember(1)] public virtual Ring MainRing { get; set; }
        [ProtoMember(2)] public virtual Ring SubRing { get; set; }
    }

    [ProtoContract]
    public class Ring : ITrackablePoco
    {
        [ProtoMember(1)] public virtual string Name { get; set; }
        [ProtoMember(2)] public virtual int Power { get; set; }
    }
}
