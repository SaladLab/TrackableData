using System;
using System.Linq;

namespace TrackableData
{
    public static class TrackableResolver
    {
        public static bool IsTrackablePoco(Type type)
        {
            return typeof(ITrackablePoco).IsAssignableFrom(type);
        }

        public static bool IsTrackableContainer(Type type)
        {
            return typeof(ITrackableContainer).IsAssignableFrom(type);
        }

        public static bool IsTrackableDictionary(Type type)
        {
            return type.IsGenericType &&
                   type.GetGenericTypeDefinition() == typeof(TrackableDictionary<,>);
        }

        public static bool IsTrackableSet(Type type)
        {
            return type.IsGenericType &&
                   type.GetGenericTypeDefinition() == typeof(TrackableSet<>);
        }

        public static bool IsTrackableList(Type type)
        {
            return type.IsGenericType &&
                   type.GetGenericTypeDefinition() == typeof(TrackableList<>);
        }

        public static Type GetPocoType(Type trackableType)
        {
            if (typeof(ITrackablePoco).IsAssignableFrom(trackableType))
            {
                var trackerType = trackableType.GetInterfaces()
                                               .FirstOrDefault(t => t.IsGenericType &&
                                                                    t.GetGenericTypeDefinition() == typeof(ITrackable<>));
                return trackerType?.GetGenericArguments()[0];
            }

            return null;
        }

        public static Type GetPocoTrackerType(Type pocoType)
        {
            if (typeof(ITrackablePoco).IsAssignableFrom(pocoType))
            {
                var trackableTypeName = pocoType.Namespace + "." + ("Trackable" + pocoType.Name.Substring(1));
                return pocoType.Assembly.GetType(trackableTypeName);
            }

            return null;
        }

        public static Type GetContainerType(Type trackableType)
        {
            if (typeof(ITrackableContainer).IsAssignableFrom(trackableType))
            {
                var trackerType = trackableType.GetInterfaces()
                                               .FirstOrDefault(t => t.IsGenericType &&
                                                                    t.GetGenericTypeDefinition() == typeof(ITrackable<>));
                return trackerType?.GetGenericArguments()[0];
            }

            return null;
        }

        public static Type GetContainerTrackerType(Type containerType)
        {
            if (typeof(ITrackableContainer).IsAssignableFrom(containerType))
            {
                var trackableTypeName = containerType.Namespace + "." + ("Trackable" + containerType.Name.Substring(1));
                return containerType.Assembly.GetType(trackableTypeName);
            }

            return null;
        }
    }
}
