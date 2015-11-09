using ProtoBuf;

namespace TrackableData
{
    [ProtoContract]
    public class EnvelopedObject<T>
    {
        [ProtoMember(1)] public T Value;
    }
}
