using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;
using TrackableData;
using TrackableData.Protobuf;

namespace Unity.Data
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
    public class ProtobufPrecompileDirectives
    {
        public TrackableDictionaryTrackerSurrogate<int, ItemData> T1;
        public TrackableDictionaryTrackerSurrogate<int, string> T2;
        public TrackableListTrackerSurrogate<string> T3;
    }
}
