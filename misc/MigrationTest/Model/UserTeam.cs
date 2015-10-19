using System;
using Newtonsoft.Json;
using ProtoBuf;
using TrackableData;

namespace Model
{
    [ProtoContract]
    public class UserTeam
    {
        [ProtoMember(1)]
        public string Name { get; set; }

        // TODO: how to handle fixed array efficiently.

        [ProtoMember(2)]
        public int Member0 { get; set; }

        [ProtoMember(3)]
        public int Member1 { get; set; }

        [ProtoMember(4)]
        public int Member2 { get; set; }

        [ProtoMember(5)]
        public int Member3 { get; set; }

        // TODO: How to handle ignore field.
        /*
        [TrackableField("ignore")]
        public int[] Members
        {
            get { return new[] { Member0, Member1, Member2, Member3 }; }
            set
            {
                Member0 = value.Length > 0 ? value[0] : 0;
                Member1 = value.Length > 1 ? value[1] : 0;
                Member2 = value.Length > 2 ? value[2] : 0;
                Member3 = value.Length > 3 ? value[3] : 0;
            }
        }
        */
    }
}