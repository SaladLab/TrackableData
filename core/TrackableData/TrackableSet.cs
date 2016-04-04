using System;
using System.Collections;
using System.Collections.Generic;

namespace TrackableData
{
    public class TrackableSet<T> :
#if !NET35
        ISet<T>,
#endif
        ICollection<T>,
        ITrackable<ICollection<T>>
    {
        private readonly HashSet<T> _set = new HashSet<T>();

        // Specific tracker

        public ISetTracker<T> Tracker { get; set; }

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
                var tracker = (ISetTracker<T>)value;
                Tracker = tracker;
            }
        }

        ITracker<ICollection<T>> ITrackable<ICollection<T>>.Tracker
        {
            get
            {
                return Tracker;
            }
            set
            {
                var tracker = (ISetTracker<T>)value;
                Tracker = tracker;
            }
        }

        public ITrackable GetChildTrackable(object name)
        {
            return null;
        }

        public IEnumerable<KeyValuePair<object, ITrackable>> GetChildTrackables(bool changedOnly = false)
        {
            yield break;
        }

        // ISet<T>

        public bool Add(T item)
        {
            if (_set.Add(item))
            {
                if (Tracker != null)
                    Tracker.TrackAdd(item);
                return true;
            }
            return false;
        }

        public void UnionWith(IEnumerable<T> other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            foreach (var item in other)
                Add(item);
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            if (_set.Count == 0 || other == this)
                return;

            var removes = new HashSet<T>(this);
            removes.ExceptWith(other);

            foreach (var item in removes)
                Remove(item);
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            if (_set.Count == 0)
                return;

            if (other == this)
            {
                Clear();
                return;
            }

            foreach (var element in other)
                Remove(element);
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            if (_set.Count == 0)
            {
                UnionWith(other);
                return;
            }

            if (other == this)
            {
                Clear();
                return;
            }

            foreach (var item in other)
            {
                if (_set.Contains(item))
                    Remove(item);
                else
                    Add(item);
            }
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            return _set.IsSubsetOf(other);
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            return _set.IsSupersetOf(other);
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            return _set.IsProperSupersetOf(other);
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            return _set.IsProperSubsetOf(other);
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            return _set.Overlaps(other);
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            return _set.SetEquals(other);
        }

        // ICollection<T>

        void ICollection<T>.Add(T item)
        {
            if (_set.Add(item))
            {
                if (Tracker != null)
                    Tracker.TrackAdd(item);
            }
        }

        public void Clear()
        {
            if (Tracker != null)
            {
                foreach (var i in _set)
                    Tracker.TrackRemove(i);
            }

            _set.Clear();
        }

        public bool Contains(T item)
        {
            return _set.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            ((ICollection<T>)_set).CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            if (_set.Remove(item))
            {
                if (Tracker != null)
                    Tracker.TrackRemove(item);
                return true;
            }
            return false;
        }

        public int Count
        {
            get { return _set.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        // IEnumerator<T>

        public IEnumerator<T> GetEnumerator()
        {
            return _set.GetEnumerator();
        }

        // IEnumerable

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_set).GetEnumerator();
        }
    }
}
