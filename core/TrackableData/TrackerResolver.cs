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
            if (typeof(ITrackablePoco).IsAssignableFrom(trackableType))
            {
                var trackerType =
                    trackableType.GetInterfaces()
                                 .FirstOrDefault(t => t.IsGenericType &&
                                                      t.GetGenericTypeDefinition() == typeof(ITrackable<>));
                if (trackerType != null)
                    return typeof(TrackablePocoTracker<>).MakeGenericType(trackerType.GetGenericArguments()[0]);
            }
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
