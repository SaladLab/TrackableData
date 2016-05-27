using System.Collections.Generic;
using ProtoBuf;

namespace TrackableData.Protobuf
{
    [ProtoContract]
    public class TrackableDictionaryTrackerSurrogate<TKey, TValue>
    {
        [ProtoContract]
        public struct Change
        {
            [ProtoMember(1, IsRequired = true)] public TrackableDictionaryOperation Operation;
            [ProtoMember(2, IsRequired = true)] public TKey Key;
            [ProtoMember(3)] public TValue NewValue;
        }

        [ProtoMember(1)] public List<Change> ChangeList = new List<Change>();

        [ProtoConverter]
        public static TrackableDictionaryTrackerSurrogate<TKey, TValue> Convert(
            TrackableDictionaryTracker<TKey, TValue> tracker)
        {
            if (tracker == null)
                return null;

            var surrogate = new TrackableDictionaryTrackerSurrogate<TKey, TValue>();
            foreach (var item in tracker.ChangeMap)
            {
                surrogate.ChangeList.Add(new Change
                {
                    Operation = item.Value.Operation,
                    Key = item.Key,
                    NewValue = item.Value.NewValue,
                });
            }
            return surrogate;
        }

        [ProtoConverter]
        public static TrackableDictionaryTracker<TKey, TValue> Convert(
            TrackableDictionaryTrackerSurrogate<TKey, TValue> surrogate)
        {
            if (surrogate == null)
                return null;

            var tracker = new TrackableDictionaryTracker<TKey, TValue>();
            foreach (var change in surrogate.ChangeList)
            {
                tracker.ChangeMap.Add(
                    change.Key,
                    new TrackableDictionaryTracker<TKey, TValue>.Change
                    {
                        Operation = change.Operation,
                        NewValue = change.NewValue
                    });
            }
            return tracker;
        }
    }
}
