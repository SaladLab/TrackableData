using System;
using System.Collections;
using System.Collections.Generic;

namespace TrackableData
{
    public class TrackableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ITrackable<IDictionary<TKey, TValue>>
    {
        private readonly Dictionary<TKey, TValue> _dictionary = new Dictionary<TKey, TValue>();

        // Specific tracker

        public IDictionaryTracker<TKey, TValue> Tracker { get; set; }

        public bool Update(TKey key, Func<TKey, TValue, TValue> updateValueFactory)
        {
            TValue value;
            if (_dictionary.TryGetValue(key, out value) == false)
                return false;

            TValue newValue = updateValueFactory(key, value);
            _dictionary[key] = newValue;

            if (Tracker != null)
                Tracker.TrackModify(key, value, newValue);

            return true;
        }

        // It doens't provide atomic operation like ConcurrentDictionary.
        public TValue AddOrUpdate(TKey key,
                                  Func<TKey, TValue> addValueFactory,
                                  Func<TKey, TValue, TValue> updateValueFactory)
        {
            TValue value;
            if (_dictionary.TryGetValue(key, out value) == false)
            {
                TValue addValue = addValueFactory(key);
                Add(key, addValue);
                return addValue;
            }

            TValue newValue = updateValueFactory(key, value);
            _dictionary[key] = newValue;

            if (Tracker != null)
                Tracker.TrackModify(key, value, newValue);

            return newValue;
        }

        // ITrackable

        public bool Changed
        {
            get { return Tracker != null && Tracker.HasChange; }
        }

        ITracker ITrackable.Tracker
        {
            get { return Tracker; }

            set
            {
                var tracker = (IDictionaryTracker<TKey, TValue>)value;
                Tracker = tracker;
            }
        }

        ITracker<IDictionary<TKey, TValue>> ITrackable<IDictionary<TKey, TValue>>.Tracker
        {
            get { return Tracker; }

            set
            {
                var tracker = (IDictionaryTracker<TKey, TValue>)value;
                Tracker = tracker;
            }
        }

        public ITrackable GetChildTrackable(object name)
        {
            TKey key = (name.GetType() == typeof(TKey))
                           ? (TKey)name
                           : (TKey)Convert.ChangeType(name, typeof(TKey));

            TValue value;
            return _dictionary.TryGetValue(key, out value)
                       ? (ITrackable)value
                       : null;
        }

        public IEnumerable<KeyValuePair<object, ITrackable>> GetChildTrackables(bool changedOnly = false)
        {
            if (typeof(ITrackable).IsAssignableFrom(typeof(TValue)) == false)
                yield break;

            foreach (var item in _dictionary)
            {
                var trackable = (ITrackable)item.Value;
                if (changedOnly == false || trackable.Changed)
                    yield return new KeyValuePair<object, ITrackable>(item.Key, trackable);
            }
        }

        // IDictionary<TKey, TValue>

        public bool ContainsKey(TKey key)
        {
            return _dictionary.ContainsKey(key);
        }

        public void Add(TKey key, TValue value)
        {
            _dictionary.Add(key, value);

            if (Tracker != null)
                Tracker.TrackAdd(key, value);
        }

        public bool Remove(TKey key)
        {
            TValue value;
            if (_dictionary.TryGetValue(key, out value))
            {
                _dictionary.Remove(key);
                if (Tracker != null)
                    Tracker.TrackRemove(key, value);
                return true;
            }
            return false;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return _dictionary.TryGetValue(key, out value);
        }

        public TValue this[TKey key]
        {
            get { return _dictionary[key]; }
            set
            {
                TValue oldValue;
                if (_dictionary.TryGetValue(key, out oldValue))
                {
                    _dictionary[key] = value;

                    if (Tracker != null)
                        Tracker.TrackModify(key, oldValue, value);
                }
                else
                {
                    _dictionary.Add(key, value);

                    if (Tracker != null)
                        Tracker.TrackAdd(key, value);
                }
            }
        }

        public ICollection<TKey> Keys
        {
            get { return _dictionary.Keys; }
        }

        public ICollection<TValue> Values
        {
            get { return _dictionary.Values; }
        }

        // ICollection<KeyValuePair<TKey, TValue>>

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).Add(item);

            if (Tracker != null)
            {
                if (Tracker != null)
                    Tracker.TrackAdd(item.Key, item.Value);
            }
        }

        public void Clear()
        {
            if (Tracker != null)
            {
                foreach (var i in _dictionary)
                    Tracker.TrackRemove(i.Key, i.Value);
            }

            _dictionary.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).Remove(item))
            {
                if (Tracker != null)
                    Tracker.TrackRemove(item.Key, item.Value);
                return true;
            }
            return false;
        }

        public int Count
        {
            get { return _dictionary.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        // IEnumerator<T>

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        // IEnumerable

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_dictionary).GetEnumerator();
        }
    }
}
