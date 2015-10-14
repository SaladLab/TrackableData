using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using ProtoBuf;
using ProtoBuf.Meta;

namespace TrackableData
{
    [ProtoContract]
    public class EnvelopedObject<T>
    {
        [ProtoMember(1)] public T Value;
    }
}
