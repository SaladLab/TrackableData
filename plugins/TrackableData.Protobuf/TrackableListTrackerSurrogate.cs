using System.Collections.Generic;
using ProtoBuf;

namespace TrackableData.Protobuf
{
    [ProtoContract]
    public class TrackableListTrackerSurrogate<T>
    {
        [ProtoContract]
        public struct Change
        {
            [ProtoMember(1, IsRequired = true)] public TrackableListOperation Operation;
            [ProtoMember(2, IsRequired = true)] public int Index;
            [ProtoMember(3)] public T NewValue;
        }

        [ProtoMember(1)] public List<Change> ChangeList = new List<Change>();

        [ProtoConverter]
        public static TrackableListTrackerSurrogate<T> Convert(TrackableListTracker<T> tracker)
        {
            if (tracker == null)
                return null;

            var surrogate = new TrackableListTrackerSurrogate<T>();
            foreach (var change in tracker.ChangeList)
            {
                surrogate.ChangeList.Add(new Change
                {
                    Operation = change.Operation,
                    Index = change.Index,
                    NewValue = change.NewValue,
                });
            }
            return surrogate;
        }

        [ProtoConverter]
        public static TrackableListTracker<T> Convert(TrackableListTrackerSurrogate<T> surrogate)
        {
            if (surrogate == null)
                return null;

            var tracker = new TrackableListTracker<T>();
            foreach (var change in surrogate.ChangeList)
            {
                tracker.ChangeList.Add(new TrackableListTracker<T>.Change
                {
                    Operation = change.Operation,
                    Index = change.Index,
                    NewValue = change.NewValue,
                });
            }
            return tracker;
        }
    }
}
