using System;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeGen
{
    public static class Utility
    {
        public static PropertyDeclarationSyntax[] GetTrackableProperties(PropertyDeclarationSyntax[] properties)
        {
            // NOTE: it's naive approach because we don't know semantic type information here.
            return properties.Where(p =>
            {
                var parts = p.Type.ToString().Split('.');
                var typeName = parts[parts.Length - 1];
                return typeName.StartsWith("Trackable");
            }).ToArray();
        }

        public static string GetTrackerClassName(TypeSyntax type)
        {
            // NOTE: it's naive approach because we don't know semantic type information here.
            var genericType = type as GenericNameSyntax;
            if (genericType == null)
            {
                if (type.ToString().StartsWith("Trackable"))
                {
                    return $"TrackablePocoTracker<I{type.ToString().Substring(9)}>";
                }
            }
            else if (CodeAnalaysisExtensions.CompareTypeName(genericType.Identifier.ToString(),
                "TrackableData.TrackableDictionary"))
            {
                return $"TrackableDictionaryTracker{genericType.TypeArgumentList}";
            }
            else if (CodeAnalaysisExtensions.CompareTypeName(genericType.Identifier.ToString(),
                "TrackableData.TrackableList"))
            {
                return $"TrackableListTracker{genericType.TypeArgumentList}";
            }

            throw new Exception("Cannot resolve tracker class of " + type);
        }
    }
}
