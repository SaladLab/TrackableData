using System;
using System.Linq;
using System.Reflection;

namespace CodeGen
{
    public static class Utility
    {
        public static string GetTypeName(Type type)
        {
            if (type.IsGenericType)
            {
                var genericParams = String.Join(", ", type.GenericTypeArguments.Select(t => GetTypeFullName(t)));
                var delimiterPos = type.Name.IndexOf('`');
                return type.Name.Substring(0, delimiterPos) + "<" + genericParams + ">";
            }
            else
            {
                return type.Name;
            }
        }

        public static string GetTypeFullName(Type type)
        {
            return type.Namespace + "." + GetTypeName(type);
        }

        public static bool IsTrackable(Type type)
        {
            return type.GetInterfaces().Any(i => i.FullName.StartsWith("TrackableData.ITrackable"));
        }

        public static bool IsTrackablePoco(Type type)
        {
            return type.GetInterfaces().Any(i => i.FullName == "TrackableData.ITrackablePoco");
        }

        public static bool IsTrackableContainer(Type type)
        {
            return type.GetInterfaces().Any(i => i.FullName == "TrackableData.ITrackableContainer");
        }

        public static string GetTrackableClassName(Type type)
        {
            if (Utility.IsTrackablePoco(type))
            {
                return $"Trackable{type.Name.Substring(1)}";
            }
            else
            {
                var genericParams = String.Join(", ", type.GenericTypeArguments.Select(t => GetTypeFullName(t)));
                var delimiterPos = type.Name.IndexOf('`');
                return "Trackable" + type.Name.Substring(1, delimiterPos - 1) +
                       "<" + genericParams + ">";
            }
        }

        public static string GetTrackerClassName(Type type)
        {
            if (Utility.IsTrackablePoco(type))
            {
                return $"TrackablePocoTracker<{Utility.GetTypeFullName(type)}>";
            }
            else
            {
                var genericParams = String.Join(", ", type.GenericTypeArguments.Select(t => GetTypeFullName(t)));
                var delimiterPos = type.Name.IndexOf('`');
                return "Trackable" + type.Name.Substring(1, delimiterPos - 1) + "Tracker" +
                       "<" + genericParams + ">";
            }
        }
    }
}
