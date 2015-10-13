using System;
using System.Collections.Generic;

namespace TrackableData.Tests.Data
{
    public interface IDataContainer : ITrackableContainer
    {
        IPerson Person { get; set; }
        IDictionary<int, string> Dictionary { get; set; }
        IList<string> List { get; set; } 
    }
}
