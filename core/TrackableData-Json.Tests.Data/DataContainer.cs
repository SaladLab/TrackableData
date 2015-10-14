﻿using System;
using System.Collections.Generic;

namespace TrackableData.Json.Tests.Data
{
    public interface IDataContainer : ITrackableContainer
    {
        TrackablePerson Person { get; set; }
        TrackableDictionary<int, string> Dictionary { get; set; }
        TrackableList<string> List { get; set; }
    }
}
