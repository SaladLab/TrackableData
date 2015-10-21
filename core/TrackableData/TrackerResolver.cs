using System;
using System.Linq;

namespace TrackableData
{
    public static class TrackerResolver
    {
        public static Type GetDefaultTracker<T>()
        {
            return GetDefaultTracker(typeof(T));
        }

        public static Type GetDefaultTracker(Type trackableType)
        {
            // ITrackablePoco -> TrackablePocoTracker
            var pocoType = TrackableResolver.GetPocoType(trackableType);
            if (pocoType != null)
                return typeof(TrackablePocoTracker<>).MakeGenericType(pocoType);

            // ITrackableContainer -> TrackableContainerTracker
            var containerType = TrackableResolver.GetContainerType(trackableType);
            if (containerType != null)
            {
                var trackerTypeName = containerType.Namespace + "." +
                                      "Trackable" + containerType.Name.Substring(1) + "Tracker";
                return containerType?.Assembly.GetType(trackerTypeName);
            }

            // TrackableDictionary -> TrackableDictionaryTracker
            // TrackableList -> TrackableListTracker
            if (trackableType.IsGenericType)
            {
                var genericType = trackableType.GetGenericTypeDefinition();
                if (genericType == typeof(TrackableDictionary<,>))
                {
                    return typeof(TrackableDictionaryTracker<,>).MakeGenericType(
                        trackableType.GetGenericArguments());
                }
                if (genericType == typeof(TrackableList<>))
                {
                    return typeof(TrackableListTracker<>).MakeGenericType(
                        trackableType.GetGenericArguments());
                }
            }

            return null;
        }

        public static ITracker CreateDefaultTracker<T>()
        {
            return CreateDefaultTracker(typeof(T));
        }

        public static ITracker CreateDefaultTracker(Type trackableType)
        {
            var trackerType = GetDefaultTracker(trackableType);
            return (trackerType != null)
                       ? (ITracker)Activator.CreateInstance(trackerType)
                       : null;
        }
    }
}
