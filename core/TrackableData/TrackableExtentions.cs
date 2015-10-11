using System;

namespace TrackableData
{
    public static class TrackableExtensions
    {
        public static void SetDefaultTrackerDeep(this ITrackable trackable)
        {
            trackable.SetDefaultTracker();
            foreach (var childTrackable in trackable.ChildrenTrackables)
            {
                childTrackable.SetDefaultTracker();
            }
        }

        public static void Rollback(this ITrackable trackable)
        {
            var tracker = trackable.Tracker;
            if (tracker == null)
                throw new ArgumentException("trackable should have Tracker");

            if (trackable.Changed)
            {
                // To prevent tracker from saving rollback changes
                // detach tracker from trackable temporarily.
                trackable.Tracker = null;

                tracker.RollbackTo(trackable);
                tracker.Clear();

                // Attcah tracker to trackable to undo temporary touch.
                trackable.Tracker = tracker;
            }
        }

        public static void RollbackDeep(this ITrackable trackable)
        {
            trackable.Rollback();
            foreach (var childTrackable in trackable.ChildrenTrackables)
            {
                childTrackable.Rollback();
            }
        }
    }
}
