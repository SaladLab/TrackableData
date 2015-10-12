using System;
using System.Collections.Generic;

namespace TrackableData
{
    public static class TrackableExtensions
    {
        public static ITrackable GetTrackableByPath(
            this ITrackable trackable, string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return trackable;

            var si = path.IndexOf('/');
            if (si != -1)
            {
                var child = trackable.GetChildTrackable(path.Substring(0, si));
                return child?.GetTrackableByPath(path.Substring(si + 1));
            }
            else
            {
                return trackable.GetChildTrackable(path);
            }
        }

        public static IEnumerable<KeyValuePair<string, ITrackable>> GetChangedTrackablesWithPath(
            this ITrackable trackable, string parentPath = null)
        {
            if (trackable.Changed)
                yield return new KeyValuePair<string, ITrackable>(parentPath ?? "", trackable);

            foreach (var child in trackable.GetChildTrackables())
            {
                var subPath = parentPath != null ? (parentPath + "/" + child.Key) : child.Key.ToString();
                foreach (var subResult in child.Value.GetChangedTrackablesWithPath(subPath)) 
                    yield return subResult;
            }
        }

        public static IEnumerable<KeyValuePair<string, ITracker>> GetChangedTrackersWithPath(
            this ITrackable trackable, string parentPath = null)
        {
            foreach (var item in trackable.GetChangedTrackablesWithPath(parentPath))
            {
                var tracker = item.Value.Tracker;
                if (tracker != null)
                    yield return new KeyValuePair<string, ITracker>(item.Key, item.Value.Tracker);
            }
        }

        public static void ApplyTo(
            this IEnumerable<KeyValuePair<string, ITracker>> pathAndTrackers,
            ITrackable trackable)
        {
            foreach (var item in pathAndTrackers)
            {
                var targetTrackable = trackable.GetTrackableByPath(item.Key);
                item.Value.ApplyTo(targetTrackable);
            }
        }

        public static void SetDefaultTracker(this ITrackable trackable)
        {
            trackable.Tracker = TrackerResolver.CreateDefaultTracker(trackable.GetType());
        }

        public static void SetDefaultTrackerDeep(this ITrackable trackable)
        {
            trackable.SetDefaultTracker();
            foreach (var child in trackable.GetChildTrackables())
            {
                child.Value.SetDefaultTrackerDeep();
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
            foreach (var child in trackable.GetChildTrackables())
            {
                child.Value.RollbackDeep();
            }
        }
    }
}
