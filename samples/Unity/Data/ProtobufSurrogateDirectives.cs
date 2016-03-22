using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;
using TrackableData;
using TrackableData.Protobuf;

namespace Unity.Data
{
    [ProtoContract]
    public class ProtobufSurrogateDirectives
    {
        public TrackableDictionaryTrackerSurrogate<int, ItemData> T1;
        public TrackableDictionaryTrackerSurrogate<int, string> T2;
        public TrackableSetTrackerSurrogate<int> T3;
        public TrackableListTrackerSurrogate<string> T4;
    }
}
