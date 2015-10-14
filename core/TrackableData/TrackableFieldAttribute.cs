using System;
using System.Linq;

namespace TrackableData
{
    public sealed class TrackableFieldAttribute : Attribute
    {
        public string[] Parameters;

        public TrackableFieldAttribute(params string[] parameters)
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
    }
}
