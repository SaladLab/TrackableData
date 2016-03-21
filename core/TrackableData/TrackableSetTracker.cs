using System;
using System.Collections.Generic;
using System.Linq;

namespace TrackableData
{
    public enum TrackableSetOperation : byte
    {
        None = 0,
        Add = 1,
        Remove = 2,
    }

    public class TrackableSetTracker<T> : ISetTracker<T>
    {
        public Dictionary<T, TrackableSetOperation> ChangeMap = new Dictionary<T, TrackableSetOperation>();

        public bool GetChange(T value, out TrackableSetOperation operation)
        {
            return ChangeMap.TryGetValue(value, out operation);
        }

        public void TrackAdd(T value)
        {
            TrackableSetOperation prevOperation;
            GetChange(value, out prevOperation);

            switch (prevOperation)
            {
                case TrackableSetOperation.None:
                    SetChange(value, TrackableSetOperation.Add);
                    break;

                case TrackableSetOperation.Add:
                    throw new InvalidOperationException("Add after add is impossbile.");

                case TrackableSetOperation.Remove:
                    ChangeMap.Remove(value);
                    break;
            }
        }

        public void TrackRemove(T value)
        {
            TrackableSetOperation prevOperation;
            GetChange(value, out prevOperation);

            switch (prevOperation)
            {
                case TrackableSetOperation.None:
                    SetChange(value, TrackableSetOperation.Remove);
                    break;

                case TrackableSetOperation.Add:
                    ChangeMap.Remove(value);
                    break;

                case TrackableSetOperation.Remove:
                    throw new InvalidOperationException("Remove after remove is impossbile.");
            }
        }

        private void SetChange(T value, TrackableSetOperation operation)
        {
            var hasChangedBefore = HasChange;

            ChangeMap[value] = operation;

            if (HasChangeSet != null && hasChangedBefore == false)
                HasChangeSet(this);
        }

        public IEnumerable<T> AddValues
        {
            get
            {
                return ChangeMap.Where(i => i.Value == TrackableSetOperation.Add)
                                .Select(i => i.Key);
            }
        }

        public IEnumerable<T> RemoveValues
        {
            get
            {
                return ChangeMap.Where(i => i.Value == TrackableSetOperation.Remove)
                                .Select(i => i.Key);
            }
        }

        // ITracker

        public bool HasChange
        {
            get { return ChangeMap.Any(); }
        }

        public event TrackerHasChangeSet HasChangeSet;

        public void Clear()
        {
            ChangeMap.Clear();
        }

        public void ApplyTo(object trackable)
        {
            ApplyTo((ICollection<T>)trackable);
        }

        public void ApplyTo(ICollection<T> trackable)
        {
            if (trackable == null)
                throw new ArgumentNullException("trackable");

            foreach (var item in ChangeMap)
            {
                switch (item.Value)
                {
                    case TrackableSetOperation.Add:
                        trackable.Add(item.Key);
                        break;

                    case TrackableSetOperation.Remove:
                        trackable.Remove(item.Key);
                        break;
                }
            }
        }

        public void ApplyTo(ITracker tracker)
        {
            ApplyTo((TrackableSetTracker<T>)tracker);
        }

        public void ApplyTo(ITracker<ICollection<T>> tracker)
        {
            ApplyTo((TrackableSetTracker<T>)tracker);
        }

        public void ApplyTo(TrackableSetTracker<T> tracker)
        {
            if (tracker == null)
                throw new ArgumentNullException("tracker");

            foreach (var item in ChangeMap)
            {
                switch (item.Value)
                {
                    case TrackableSetOperation.Add:
                        tracker.TrackAdd(item.Key);
                        break;

                    case TrackableSetOperation.Remove:
                        tracker.TrackRemove(item.Key);
                        break;
                }
            }
        }

        public void RollbackTo(object trackable)
        {
            RollbackTo((ICollection<T>)trackable);
        }

        public void RollbackTo(ICollection<T> trackable)
        {
            if (trackable == null)
                throw new ArgumentNullException("trackable");

            foreach (var item in ChangeMap)
            {
                switch (item.Value)
                {
                    case TrackableSetOperation.Add:
                        trackable.Remove(item.Key);
                        break;

                    case TrackableSetOperation.Remove:
                        trackable.Add(item.Key);
                        break;
                }
            }
        }

        public void RollbackTo(ITracker tracker)
        {
            RollbackTo((TrackableSetTracker<T>)tracker);
        }

        public void RollbackTo(ITracker<ICollection<T>> tracker)
        {
            RollbackTo((TrackableSetTracker<T>)tracker);
        }

        public void RollbackTo(TrackableSetTracker<T> tracker)
        {
            if (tracker == null)
                throw new ArgumentNullException("tracker");

            foreach (var item in ChangeMap)
            {
                switch (item.Value)
                {
                    case TrackableSetOperation.Add:
                        tracker.TrackRemove(item.Key);
                        break;

                    case TrackableSetOperation.Remove:
                        tracker.TrackAdd(item.Key);
                        break;
                }
            }
        }

        // Object

        public override string ToString()
        {
            return "{ " + string.Join(", ", ChangeMap.Select(x =>
            {
                switch (x.Value)
                {
                    case TrackableSetOperation.Add:
                        return "+" + x.Key;

                    case TrackableSetOperation.Remove:
                        return "-" + x.Key;

                    default:
                        return "";
                }
            }).ToArray()) + " }";
        }
    }
}
