using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;
using TrackableData;

namespace Unity.Data
{
    [ProtoContract]
    public interface IUserData : ITrackablePoco<IUserData>
    {
        [ProtoMember(1)] string Name { get; set; }
        [ProtoMember(2)] int Gold { get; set; }
        [ProtoMember(3)] int Level { get; set; }
    }
}
