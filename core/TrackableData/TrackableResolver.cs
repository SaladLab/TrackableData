using System;
using System.Linq;

namespace TrackableData
{
    public static class TrackableResolver
    {
        public static Type GetPocoType(Type trackableType)
        {
            if (typeof(ITrackablePoco).IsAssignableFrom(trackableType))
            {
                var trackerType = trackableType.GetInterfaces()
                                               .FirstOrDefault(t => t.IsGenericType &&
                                                                    t.GetGenericTypeDefinition() == typeof(ITrackable<>));
                if (trackerType != null)
                    return trackerType.GetGenericArguments()[0];
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
    }
}
