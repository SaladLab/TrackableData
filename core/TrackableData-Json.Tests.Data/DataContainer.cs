using System;
using System.Collections.Generic;

namespace TrackableData.Json.Tests.Data
{
    public class DataContainer : ITrackableContainer
    {
        public virtual Person Person { get; set; }
        public virtual IDictionary<int, string> Dictionary { get; set; }
        public virtual IList<string> List { get; set; } 
    }
}
