using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TrackableData
{
    public enum TrackableListOperation : byte
    {
        None = 0,
        Insert = 1,
        Remove = 2,
        Modify = 3,
    }

    public class TrackableListTracker<T> : IListTracker<T>
    {
        public struct Change
        {
            public TrackableListOperation Operation;
            public int Index;
            public T OldValue;
            public T NewValue;
        }

        public List<Change> ChangeList = new List<Change>();

        public void TrackInsert(int index, T newValue)
        {
            ChangeList.Add(new Change
            {
                Operation = TrackableListOperation.Insert,
                Index = index,
                NewValue = newValue
            });
        }

        public void TrackRemove(int index, T oldValue)
        {
            ChangeList.Add(new Change
            {
                Operation = TrackableListOperation.Remove,
                Index = index,
                OldValue = oldValue,
            });
        }

        public void TrackModify(int index, T oldValue, T newValue)
        {
            ChangeList.Add(new Change
            {
                Operation = TrackableListOperation.Modify,
                Index = index,
                OldValue = oldValue,
                NewValue = newValue
            });
        }

        // ITracker

        public bool HasChange
        {
            get { return ChangeList.Count > 0; }
        }

        public void Clear()
        {
            ChangeList.Clear();
        }

        public void ApplyTo(object trackable)
        {
            ApplyTo((IList<T>)trackable);
        }

        public void ApplyTo(IList<T> trackable)
        {
            if (trackable == null)
                throw new ArgumentNullException("trackable");

            foreach (var item in ChangeList)
            {
                switch (item.Operation)
                {
                    case TrackableListOperation.Insert:
                        trackable.Insert(item.Index, item.NewValue);
                        break;

                    case TrackableListOperation.Remove:
                        trackable.RemoveAt(item.Index);
                        break;

                    case TrackableListOperation.Modify:
                        trackable[item.Index] = item.NewValue;
                        break;
                }
            }
        }

        public void ApplyTo(ITracker tracker)
        {
            ApplyTo((TrackableListTracker<T>)tracker);
        }

        public void ApplyTo(ITracker<IList<T>> tracker)
        {
            ApplyTo((TrackableListTracker<T>)tracker);
        }

        public void ApplyTo(TrackableListTracker<T> tracker)
        {
            if (tracker == null)
                throw new ArgumentNullException("tracker");

            foreach (var item in ChangeList)
            {
                switch (item.Operation)
                {
                    case TrackableListOperation.Insert:
                        tracker.TrackInsert(item.Index, item.NewValue);
                        break;

                    case TrackableListOperation.Remove:
                        tracker.TrackRemove(item.Index, item.OldValue);
                        break;

                    case TrackableListOperation.Modify:
                        tracker.TrackModify(item.Index, item.OldValue, item.NewValue);
                        break;
                }
            }
        }

        public void RollbackTo(object trackable)
        {
            RollbackTo((IList<T>)trackable);
        }

        public void RollbackTo(IList<T> trackable)
        {
            if (trackable == null)
                throw new ArgumentNullException("trackable");

            for (int i = ChangeList.Count - 1; i >= 0; i--)
            {
                var item = ChangeList[i];
                switch (item.Operation)
                {
                    case TrackableListOperation.Insert:
                        trackable.RemoveAt(item.Index);
                        break;

                    case TrackableListOperation.Remove:
                        trackable.Insert(item.Index, item.OldValue);
                        break;

                    case TrackableListOperation.Modify:
                        trackable[item.Index] = item.OldValue;
                        break;
                }
            }
        }

        public void RollbackTo(ITracker tracker)
        {
            RollbackTo((TrackableListTracker<T>)tracker);
        }

        public void RollbackTo(ITracker<IList<T>> tracker)
        {
            RollbackTo((TrackableListTracker<T>)tracker);
        }

        public void RollbackTo(TrackableListTracker<T> tracker)
        {
            if (tracker == null)
                throw new ArgumentNullException("tracker");

            for (int i = ChangeList.Count - 1; i >= 0; i--)
            {
                var item = ChangeList[i];
                switch (item.Operation)
                {
                    case TrackableListOperation.Insert:
                        tracker.TrackRemove(item.Index, item.NewValue);
                        break;

                    case TrackableListOperation.Remove:
                        tracker.TrackInsert(item.Index, item.OldValue);
                        break;

                    case TrackableListOperation.Modify:
                        tracker.TrackModify(item.Index, item.NewValue, item.OldValue);
                        break;
                }
            }
        }

        // Object

        public override string ToString()
        {
            return "[ " + string.Join(", ", ChangeList.Select(x =>
            {
                switch (x.Operation)
                {
                    case TrackableListOperation.Insert:
                        return "+" + x.Index + ":" + x.NewValue;

                    case TrackableListOperation.Remove:
                        return "-" + x.Index + ":" + x.OldValue;

                    case TrackableListOperation.Modify:
                        return "=" + x.Index + ":" + x.OldValue + "=>" + x.NewValue;

                    default:
                        return "";
                }
            })) + " ]";
        }
    }
}
