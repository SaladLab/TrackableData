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

    [Serializable]
    public struct TrackableDictionaryOperationItem<TKey, TValue>
    {
        private readonly TKey _key;
        private readonly TrackableDictionaryOperation _operation;
        private readonly TValue _oldValue;
        private readonly TValue _newValue;

        public TKey Key
        {
            get { return _key; }
        }

        public TrackableDictionaryOperation Operation
        {
            get { return _operation; }
        }

        public TValue OldValue
        {
            get { return _oldValue; }
        }

        public TValue NewValue
        {
            get { return _newValue; }
        }

        public TrackableDictionaryOperationItem(TKey key, TrackableDictionaryOperation operation, TValue oldValue, TValue newValue)
        {
            _key = key;
            _operation = operation;
            _oldValue = oldValue;
            _newValue = newValue;
        }
    }

    public class TrackableDictionaryTracker<TKey, TValue> : ITracker
        where TValue : new()
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

        public TrackableDictionaryOperationItem<TKey, TValue>[] GetChanges()
        {
            if (ChangeMap.Count == 0)
                return null;

            var changes = new TrackableDictionaryOperationItem<TKey, TValue>[ChangeMap.Count];
            var cur = 0;
            foreach (var i in ChangeMap)
            {
                changes[cur] = new TrackableDictionaryOperationItem<TKey, TValue>(
                    i.Key, i.Value.Operation, i.Value.OldValue, i.Value.NewValue);
                cur += 1;
            }
            return changes;
        }

        public void SetChanges(TrackableDictionaryOperationItem<TKey, TValue>[] changes)
        {
            ChangeMap.Clear();
            if (changes == null || changes.Length == 0)
                return;

            foreach (var i in changes)
                ChangeMap[i.Key] = new Change { Operation = i.Operation, OldValue = i.OldValue, NewValue = i.NewValue };
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

        public void ApplyTo(object obj)
        {
            var dictionary = (IDictionary<TKey, TValue>)obj;
            ApplyTo(dictionary);
        }

        public void ApplyTo(IDictionary<TKey, TValue> dictionary, bool strict = false)
        {
            foreach (var item in ChangeMap)
            {
                switch (item.Value.Operation)
                {
                    case TrackableDictionaryOperation.Add:
                        if (strict)
                            dictionary.Add(item.Key, item.Value.NewValue);
                        else
                            dictionary[item.Key] = item.Value.NewValue;
                        break;

                    case TrackableDictionaryOperation.Remove:
                        dictionary.Remove(item.Key);
                        break;

                    case TrackableDictionaryOperation.Modify:
                        dictionary[item.Key] = item.Value.NewValue;
                        break;
                }
            }
        }

        public void ApplyTo(ITracker tracker)
        {
            var t = (TrackableDictionaryTracker<TKey, TValue>)tracker;
            if (t == null)
                throw new ArgumentException("tracker");

            ApplyTo(t);
        }

        public void ApplyTo(TrackableDictionaryTracker<TKey, TValue> tracker)
        {
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

        public void RollbackTo(object obj)
        {
            var dictionary = (IDictionary<TKey, TValue>)obj;
            ApplyTo(dictionary);
        }

        public void RollbackTo(IDictionary<TKey, TValue> dictionary)
        {
            foreach (var item in ChangeMap)
            {
                switch (item.Value.Operation)
                {
                    case TrackableDictionaryOperation.Add:
                        dictionary.Remove(item.Key);
                        break;

                    case TrackableDictionaryOperation.Remove:
                        dictionary[item.Key] = item.Value.OldValue;
                        break;

                    case TrackableDictionaryOperation.Modify:
                        dictionary[item.Key] = item.Value.OldValue;
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

                    case TrackableDictionaryOperation.Modify:
                        return "=" + x.Key + ":" + x.Value.OldValue + "=>" + x.Value.NewValue;

                    case TrackableDictionaryOperation.Remove:
                        return "-" + x.Key + ":" + x.Value.OldValue;

                    default:
                        return "";
                }
            })) + " }";
        }
    }
}
