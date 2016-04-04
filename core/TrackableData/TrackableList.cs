using System;
using System.Collections;
using System.Collections.Generic;

namespace TrackableData
{
    public class TrackableList<T> : IList<T>, ITrackable<IList<T>>
    {
        private readonly List<T> _list = new List<T>();

        // Specific tracker

        public IListTracker<T> Tracker { get; set; }

        // ITrackable

        public bool Changed
        {
            get { return Tracker != null && Tracker.HasChange; }
        }

        ITracker ITrackable.Tracker
        {
            get
            {
                return Tracker;
            }
            set
            {
                var tracker = (IListTracker<T>)value;
                Tracker = tracker;
            }
        }

        ITracker<IList<T>> ITrackable<IList<T>>.Tracker
        {
            get
            {
                return Tracker;
            }
            set
            {
                var tracker = (IListTracker<T>)value;
                Tracker = tracker;
            }
        }

        public ITrackable GetChildTrackable(object name)
        {
            int index = (name.GetType() == typeof(int))
                            ? (int)name
                            : (int)Convert.ChangeType(name, typeof(int));

            return index >= 0 && index < _list.Count
                       ? (ITrackable)_list[index]
                       : null;
        }

        public IEnumerable<KeyValuePair<object, ITrackable>> GetChildTrackables(bool changedOnly = false)
        {
            if (typeof(ITrackable).IsAssignableFrom(typeof(T)) == false)
                yield break;

            var count = _list.Count;
            for (var i = 0; i < count; i++)
            {
                var trackable = (ITrackable)_list[i];
                if (changedOnly == false || trackable.Changed)
                    yield return new KeyValuePair<object, ITrackable>(i, trackable);
            }
        }

        // IList<T>

        public T this[int index]
        {
            get
            {
                return _list[index];
            }
            set
            {
                if (Tracker != null)
                    Tracker.TrackModify(index, _list[index], value);
                _list[index] = value;
            }
        }

        public int IndexOf(T item)
        {
            return _list.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            if (Tracker != null)
            {
                if (index == _list.Count)
                    Tracker.TrackPushBack(item);
                else if (index == 0)
                    Tracker.TrackPushFront(item);
                else
                    Tracker.TrackInsert(index, item);
            }

            _list.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            if (Tracker != null)
            {
                if (index == _list.Count - 1)
                    Tracker.TrackPopBack(_list[index]);
                else if (index == 0)
                    Tracker.TrackPopFront(_list[index]);
                else
                    Tracker.TrackRemove(index, _list[index]);
            }

            _list.RemoveAt(index);
        }

        // ICollection<T>

        public void Add(T item)
        {
            Insert(_list.Count, item);
        }

        public void Clear()
        {
            if (Tracker != null)
            {
                for (int i = _list.Count - 1; i >= 0; i--)
                    Tracker.TrackPopBack(_list[i]);
            }

            _list.Clear();
        }

        public bool Contains(T item)
        {
            return _list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            int index = _list.IndexOf(item);
            if (index >= 0)
            {
                RemoveAt(index);
                return true;
            }

            return false;
        }

        public int Count
        {
            get { return _list.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        // IEnumerator<T>

        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        // IEnumerable

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_list).GetEnumerator();
        }
    }
}
