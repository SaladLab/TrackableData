using System;

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
            if (typeof(ITrackablePoco).IsAssignableFrom(trackableType))
            {
                var pocoType = trackableType.BaseType;
                return typeof(TrackablePocoTracker<>).MakeGenericType(pocoType);
            }
            if (trackableType.IsGenericType)
            {
                var genericType = trackableType.GetGenericTypeDefinition();
                if (genericType == typeof (TrackableDictionary<,>))
                {
                    return typeof (TrackableDictionaryTracker<,>).MakeGenericType(
                        trackableType.GenericTypeArguments);
                }
                if (genericType == typeof (TrackableList<>))
                {
                    return typeof (TrackableListTracker<>).MakeGenericType(
                        trackableType.GenericTypeArguments);
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
