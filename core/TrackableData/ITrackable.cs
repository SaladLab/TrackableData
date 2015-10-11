using System.Collections.Generic;

namespace TrackableData
{
    public interface ITrackable
    {
        // changed or not = Tracker.HasChange
        bool Changed { get; }

        // return tracker that will track this
        ITracker Tracker { get; set; }

        // Tracker = new DefaultTracker();
        void SetDefaultTracker();

        // return tracker's children trackables
        IEnumerable<ITrackable> ChildrenTrackables { get; }
    }

    public interface ITrackable<T> : ITrackable
    {
        new ITracker<T> Tracker { get; set; }
    }
}
