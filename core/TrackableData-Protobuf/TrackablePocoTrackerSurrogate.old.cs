using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using ProtoBuf;
using ProtoBuf.Meta;

namespace TrackableData.Protobuf
{
    [ProtoContract]
    public class TrackablePocoTrackerSurrogate<T>
    {
        [ProtoMember(1)] public byte[] Buffer;

        public static void ConvertToSurrogate(
            TrackablePocoTracker<T> tracker,
            TrackablePocoTrackerSurrogate<T> surrogate,
            TypeModel typeModel)
        {
            if (tracker.ChangeMap.Count == 0)
                return;

            var stream = new MemoryStream();
            var writer = new ProtoWriter(stream, typeModel, null);
            
            foreach (var changeItem in tracker.ChangeMap)
            {
                var tag = changeItem.Key.GetCustomAttribute<ProtoMemberAttribute>().Tag;

                // ProtoWriter.WriteObject(changeItem.Value.NewValue,  )
                // typeModel.Serialize(stream, changeItem.Value.NewValue);
                surrogate.ChangeItems[cursor++] = new ChangeItem
                {
                    Tag = tag,
                    Buffer = stream.GetBuffer()
                };
                stream.Dispose();
            }

/*
            surrogate.ChangeItems = new ChangeItem[tracker.ChangeMap.Count];
            var cursor = 0;
            foreach (var changeItem in tracker.ChangeMap)
            {
                var tag = changeItem.Key.GetCustomAttribute<ProtoMemberAttribute>().Tag;
                var stream = new MemoryStream();
                typeModel.Serialize(stream, changeItem.Value.NewValue);
                surrogate.ChangeItems[cursor++] = new ChangeItem
                {
                    Tag = tag,
                    Buffer = stream.GetBuffer()
                };
                stream.Dispose();
            }*/
        }

        public static void ConvertToTracker(
            TrackablePocoTrackerSurrogate<T> surrogate,
            TrackablePocoTracker<T> tracker,
            TypeModel typeModel)
        {
            if (surrogate.Buffer == null)
                return;

            // TODO:
        }

        // Serializer

        private TrackablePocoTracker<T> _currentTracker;

        [ProtoBeforeSerialization]
        public void OnSerializing(SerializationContext context)
        {
            var typeModel = (context.Context == null)
                ? (TypeModel)context.Context
                : RuntimeTypeModel.Default;

            ConvertToSurrogate(_currentTracker, this, typeModel);
            _currentTracker = null;
        }

        [ProtoAfterDeserialization]
        public void OnDeserialized(SerializationContext context)
        {
            var typeModel = (context.Context == null)
                ? (TypeModel)context.Context
                : RuntimeTypeModel.Default;

            _currentTracker = new TrackablePocoTracker<T>();
            ConvertToTracker(this, _currentTracker, typeModel);
        }

        public static implicit operator TrackablePocoTrackerSurrogate<T>(TrackablePocoTracker<T> tracker)
        {
            if (tracker == null)
                return null;

            var surrogate = new TrackablePocoTrackerSurrogate<T>();
            surrogate._currentTracker = tracker;
            return surrogate;
        }

        public static implicit operator TrackablePocoTracker<T>(TrackablePocoTrackerSurrogate<T> surrogate)
        {
            if (surrogate == null)
                return null;

            var tracker = surrogate._currentTracker;
            surrogate._currentTracker = null;
            return tracker;
        }
    }
}
