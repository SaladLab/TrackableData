using System.Collections.Generic;
using ProtoBuf;

namespace TrackableData.Protobuf
{
    [ProtoContract]
    public class TrackableSetTrackerSurrogate<T>
    {
        [ProtoMember(1)] public List<T> AddValues = new List<T>();
        [ProtoMember(2)] public List<T> RemoveValues = new List<T>();

        public static implicit operator TrackableSetTrackerSurrogate<T>(TrackableSetTracker<T> tracker)
        {
            if (tracker == null)
                return null;

            var surrogate = new TrackableSetTrackerSurrogate<T>();
            foreach (var change in tracker.ChangeMap)
            {
                if (change.Value == TrackableSetOperation.Add)
                    surrogate.AddValues.Add(change.Key);
                else
                    surrogate.RemoveValues.Add(change.Key);
            }
            return surrogate;
        }

        public static implicit operator TrackableSetTracker<T>(TrackableSetTrackerSurrogate<T> surrogate)
        {
            if (surrogate == null)
                return null;

            var tracker = new TrackableSetTracker<T>();
            foreach (var value in surrogate.AddValues)
                tracker.TrackAdd(value);
            foreach (var value in surrogate.RemoveValues)
                tracker.TrackRemove(value);
            return tracker;
        }
    }
}
