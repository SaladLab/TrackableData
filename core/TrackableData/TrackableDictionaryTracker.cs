using System;
using System.Collections.Generic;
using System.Linq;

namespace TrackableData
{
    public enum TrackableDictionaryOperation : byte
    {
        None = 0,
        Add = 1,
        Remove = 2,
        Modify = 3,
    }

    public class TrackableDictionaryTracker<TKey, TValue> : ITracker<IDictionary<TKey, TValue>>
    {
        public struct Change
        {
            public TrackableDictionaryOperation Operation;
            public TValue OldValue;
            public TValue NewValue;
        }

        public Dictionary<TKey, Change> ChangeMap = new Dictionary<TKey, Change>();

        public bool GetChange(TKey key, out Change change)
        {
            return ChangeMap.TryGetValue(key, out change);
        }

        public void TrackAdd(TKey key, TValue newValue)
        {
            Change prevChange;
            GetChange(key, out prevChange);

            switch (prevChange.Operation)
            {
                case TrackableDictionaryOperation.None:
                    ChangeMap[key] = new Change
                    {
                        Operation = TrackableDictionaryOperation.Add,
                        NewValue = newValue
                    };
                    break;

                case TrackableDictionaryOperation.Add:
                    throw new InvalidOperationException("Add after add is impossbile.");

                case TrackableDictionaryOperation.Modify:
                    prevChange.NewValue = newValue;
                    ChangeMap[key] = prevChange;
                    break;

                case TrackableDictionaryOperation.Remove:
                    ChangeMap[key] = new Change
                    {
                        Operation = TrackableDictionaryOperation.Modify,
                        OldValue = prevChange.OldValue,
                        NewValue = newValue
                    };
                    break;
            }
        }

        public void TrackRemove(TKey key, TValue oldValue)
        {
            Change prevChange;
            GetChange(key, out prevChange);

            switch (prevChange.Operation)
            {
                case TrackableDictionaryOperation.None:
                    ChangeMap[key] = new Change
                    {
                        Operation = TrackableDictionaryOperation.Remove,
                        OldValue = oldValue
                    };
                    break;

                case TrackableDictionaryOperation.Add:
                    ChangeMap.Remove(key);
                    break;

                case TrackableDictionaryOperation.Modify:
                    ChangeMap[key] = new Change
                    {
                        Operation = TrackableDictionaryOperation.Remove,
                        OldValue = prevChange.OldValue
                    };
                    break;

                case TrackableDictionaryOperation.Remove:
                    break;
            }
        }

        public void TrackModify(TKey key, TValue oldValue, TValue newValue)
        {
            Change prevChange;
            GetChange(key, out prevChange);

            switch (prevChange.Operation)
            {
                case TrackableDictionaryOperation.None:
                    ChangeMap[key] = new Change
                    {
                        Operation = TrackableDictionaryOperation.Modify,
                        OldValue = oldValue,
                        NewValue = newValue
                    };
                    break;

                case TrackableDictionaryOperation.Add:
                    prevChange.NewValue = newValue;
                    ChangeMap[key] = prevChange;
                    break;

                case TrackableDictionaryOperation.Remove:
                    throw new InvalidOperationException("Modify after remove is impossible.");

                case TrackableDictionaryOperation.Modify:
                    ChangeMap[key] = new Change
                    {
                        Operation = TrackableDictionaryOperation.Modify,
                        OldValue = prevChange.OldValue,
                        NewValue = newValue
                    };
                    break;
            }
        }

        public IEnumerable<KeyValuePair<TKey, TValue>> AddItems
        {
            get
            {
                return ChangeMap.Where(i => i.Value.Operation == TrackableDictionaryOperation.Add)
                                .Select(i => new KeyValuePair<TKey, TValue>(i.Key, i.Value.NewValue));
            }
        }

        public IEnumerable<KeyValuePair<TKey, TValue>> ModifyItems
        {
            get
            {
                return ChangeMap.Where(i => i.Value.Operation == TrackableDictionaryOperation.Modify)
                                .Select(i => new KeyValuePair<TKey, TValue>(i.Key, i.Value.NewValue));
            }
        }

        public IEnumerable<KeyValuePair<TKey, TValue>> RemoveItems
        {
            get
            {
                return ChangeMap.Where(i => i.Value.Operation == TrackableDictionaryOperation.Remove)
                                .Select(i => new KeyValuePair<TKey, TValue>(i.Key, i.Value.OldValue));
            }
        }

        public IEnumerable<TKey> RemoveKeys
        {
            get
            {
                return ChangeMap.Where(i => i.Value.Operation == TrackableDictionaryOperation.Remove)
                                .Select(i => i.Key);
            }
        }

        // ITracker

        public bool HasChange
        {
            get { return ChangeMap.Any(); }
        }

        public void Clear()
        {
            ChangeMap.Clear();
        }

        public void ApplyTo(object trackable)
        {
            ApplyTo((IDictionary<TKey, TValue>)trackable);
        }

        public void ApplyTo(IDictionary<TKey, TValue> trackable)
        {
            ApplyTo(trackable, false);
        }

        public void ApplyTo(IDictionary<TKey, TValue> trackable, bool strict)
        {
            if (trackable == null)
                throw new ArgumentNullException("trackable");

            foreach (var item in ChangeMap)
            {
                switch (item.Value.Operation)
                {
                    case TrackableDictionaryOperation.Add:
                        if (strict)
                            trackable.Add(item.Key, item.Value.NewValue);
                        else
                            trackable[item.Key] = item.Value.NewValue;
                        break;

                    case TrackableDictionaryOperation.Remove:
                        trackable.Remove(item.Key);
                        break;

                    case TrackableDictionaryOperation.Modify:
                        trackable[item.Key] = item.Value.NewValue;
                        break;
                }
            }
        }

        public void ApplyTo(ITracker tracker)
        {
            ApplyTo((TrackableDictionaryTracker<TKey, TValue>)tracker);
        }

        public void ApplyTo(ITracker<IDictionary<TKey, TValue>> tracker)
        {
            ApplyTo((TrackableDictionaryTracker<TKey, TValue>)tracker);
        }

        public void ApplyTo(TrackableDictionaryTracker<TKey, TValue> tracker)
        {
            if (tracker == null)
                throw new ArgumentNullException("tracker");

            foreach (var item in ChangeMap)
            {
                switch (item.Value.Operation)
                {
                    case TrackableDictionaryOperation.Add:
                        tracker.TrackAdd(item.Key, item.Value.NewValue);
                        break;

                    case TrackableDictionaryOperation.Remove:
                        tracker.TrackRemove(item.Key, item.Value.OldValue);
                        break;

                    case TrackableDictionaryOperation.Modify:
                        tracker.TrackModify(item.Key, item.Value.OldValue, item.Value.NewValue);
                        break;
                }
            }
        }

        public void RollbackTo(object trackable)
        {
            RollbackTo((IDictionary<TKey, TValue>)trackable);
        }

        public void RollbackTo(IDictionary<TKey, TValue> trackable)
        {
            if (trackable == null)
                throw new ArgumentNullException("trackable");

            foreach (var item in ChangeMap)
            {
                switch (item.Value.Operation)
                {
                    case TrackableDictionaryOperation.Add:
                        trackable.Remove(item.Key);
                        break;

                    case TrackableDictionaryOperation.Remove:
                        trackable[item.Key] = item.Value.OldValue;
                        break;

                    case TrackableDictionaryOperation.Modify:
                        trackable[item.Key] = item.Value.OldValue;
                        break;
                }
            }
        }

        public void RollbackTo(ITracker tracker)
        {
            RollbackTo((TrackableDictionaryTracker<TKey, TValue>)tracker);
        }

        public void RollbackTo(ITracker<IDictionary<TKey, TValue>> tracker)
        {
            RollbackTo((TrackableDictionaryTracker<TKey, TValue>)tracker);
        }

        public void RollbackTo(TrackableDictionaryTracker<TKey, TValue> tracker)
        {
            if (tracker == null)
                throw new ArgumentNullException("tracker");

            foreach (var item in ChangeMap)
            {
                switch (item.Value.Operation)
                {
                    case TrackableDictionaryOperation.Add:
                        tracker.TrackRemove(item.Key, item.Value.NewValue);
                        break;

                    case TrackableDictionaryOperation.Remove:
                        tracker.TrackAdd(item.Key, item.Value.OldValue);
                        break;

                    case TrackableDictionaryOperation.Modify:
                        tracker.TrackModify(item.Key, item.Value.NewValue, item.Value.OldValue);
                        break;
                }
            }
        }

        // Object

        public override string ToString()
        {
            return "{ " + string.Join(", ", ChangeMap.Select(x =>
            {
                switch (x.Value.Operation)
                {
                    case TrackableDictionaryOperation.Add:
                        return "+" + x.Key + ":" + x.Value.NewValue;

                    case TrackableDictionaryOperation.Remove:
                        return "-" + x.Key + ":" + x.Value.OldValue;

                    case TrackableDictionaryOperation.Modify:
                        return "=" + x.Key + ":" + x.Value.OldValue + "=>" + x.Value.NewValue;

                    default:
                        return "";
                }
            })) + " }";
        }
    }
}
