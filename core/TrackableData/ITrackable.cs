using System.Collections.Generic;

namespace TrackableData
{
    public interface ITrackable
    {
        // changed or not = Tracker.HasChange
        bool Changed { get; }

        // return tracker that will track this
        ITracker Tracker { get; set; }

        // return child trackable which have specified name, otherwise null
        ITrackable GetChildTrackable(object name);

        // return all child trackables with name
        IEnumerable<KeyValuePair<object, ITrackable>> GetChildTrackables(bool changedOnly = false);
    }

    public interface ITrackable<T> : ITrackable
    {
        new ITracker<T> Tracker { get; set; }
    }

    // -----

    public interface ITrackablePoco
    {
    }

    public interface ITrackablePoco<T> : ITrackable<T>, ITrackablePoco
    {
    }

    public interface ITrackableContainer
    {
    }

    public interface ITrackableContainer<T> : ITrackable<T>, ITrackableContainer
    {
    }
}
