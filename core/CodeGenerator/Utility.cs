using System;
using System.Linq;

namespace CodeGen
{
    public static class Utility
    {
        public static string GetTypeName(Type type)
        {
            if (type.IsGenericType)
            {
                var genericParams = String.Join(", ", type.GenericTypeArguments.Select(t => GetTypeName(t)));
                var delimiterPos = type.Name.IndexOf('`');
                return type.Namespace + "." + type.Name.Substring(0, delimiterPos) + "<" + genericParams + ">";
            }
            else
            {
                return type.FullName;
            }
        }

        public static bool IsTrackable(Type type)
        {
            return type.IsClass &&
                   type.GetInterfaces().Any(i => i.FullName.StartsWith("TrackableData.ITrackable"));
        }

        public static bool IsTrackablePoco(Type type)
        {
            return type.IsClass &&
                   type.GetInterfaces().Any(i => i.FullName == "TrackableData.ITrackablePoco");
        }
    }
}
