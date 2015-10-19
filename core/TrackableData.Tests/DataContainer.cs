﻿namespace TrackableData.Tests
{
    public interface IDataContainer : ITrackableContainer<IDataContainer>
    {
        TrackablePerson Person { get; set; }
        TrackableDictionary<int, string> Dictionary { get; set; }
        TrackableList<string> List { get; set; }
    }
}
