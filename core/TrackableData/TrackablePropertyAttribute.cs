﻿using System;
using System.Linq;
using System.Reflection;

namespace TrackableData
{
    public sealed class TrackablePropertyAttribute : Attribute
    {
        public string[] Parameters;

        public TrackablePropertyAttribute(params string[] parameters)
        {
            Parameters = parameters;
        }

        public string this[string parameter]
        {
            get
            {
                if (parameter.EndsWith(":"))
                {
                    // as property
                    var p = Parameters.FirstOrDefault(x => x.StartsWith(parameter));
                    return p?.Substring(parameter.Length);
                }
                else
                {
                    // as flag
                    return Parameters.Any(p => p == parameter) ? "true" : null;
                }
            }
        }

        public static string GetParameter(ICustomAttributeProvider provider, string parameter)
        {
            if (parameter.EndsWith(":"))
            {
                // as property
                foreach (var property in provider.GetCustomAttributes(false).OfType<TrackablePropertyAttribute>())
                {
                    var p = property.Parameters.FirstOrDefault(x => x.StartsWith(parameter));
                    if (p != null)
                        return p.Substring(parameter.Length);
                }
            }
            else
            {
                // as flag
                foreach (var property in provider.GetCustomAttributes(false).OfType<TrackablePropertyAttribute>())
                {
                    if (property.Parameters.Any(p => p == parameter))
                        return "true";
                }
            }
            return null;
        }
    }
}
