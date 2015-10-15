using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TrackableData
{
    public class TrackablePocoTracker<T> : IPocoTracker<T>
    {
        public struct Change
        {
            public object OldValue;
            public object NewValue;
        }

        public Dictionary<PropertyInfo, Change> ChangeMap = new Dictionary<PropertyInfo, Change>();

        public void TrackSet(PropertyInfo pi, object oldValue, object newValue)
        {
            Change change;
            if (ChangeMap.TryGetValue(pi, out change))
            {
                ChangeMap[pi] = new Change { OldValue = change.OldValue, NewValue = newValue };
            }
            else
            {
                ChangeMap[pi] = new Change { OldValue = oldValue, NewValue = newValue };
            }
        }

        // ITracker

        public bool HasChange => ChangeMap.Any();

        public void Clear()
        {
            ChangeMap.Clear();
        }

        public void ApplyTo(object trackable)
        {
            var poco = (T)trackable;
            ApplyTo(poco);
        }

        public void ApplyTo(T trackable)
        {
            if (trackable == null)
                throw new ArgumentNullException(nameof(trackable));

            foreach (var item in ChangeMap)
            {
                var setter = item.Key.GetSetMethod().GetBaseDefinition();
                setter.Invoke(trackable, new[] { item.Value.NewValue });
            }
        }

        public void ApplyTo(ITracker tracker)
        {
            ApplyTo((TrackablePocoTracker<T>)tracker);
        }

        public void ApplyTo(ITracker<T> tracker)
        {
            ApplyTo((TrackablePocoTracker<T>)tracker);
        }

        public void ApplyTo(TrackablePocoTracker<T> tracker)
        {
            if (tracker == null)
                throw new ArgumentNullException(nameof(tracker));

            foreach (var item in ChangeMap)
                tracker.TrackSet(item.Key, item.Value.OldValue, item.Value.NewValue);
        }

        public void RollbackTo(object trackable)
        {
            RollbackTo((T)trackable);
        }

        public void RollbackTo(T trackable)
        {
            if (trackable == null)
                throw new ArgumentNullException(nameof(trackable));

            foreach (var item in ChangeMap)
            {
                var setter = item.Key.GetSetMethod().GetBaseDefinition();
                setter.Invoke(trackable, new[] { item.Value.OldValue });
            }
        }

        public void RollbackTo(ITracker tracker)
        {
            RollbackTo((TrackablePocoTracker<T>)tracker);
        }

        public void RollbackTo(ITracker<T> tracker)
        {
            RollbackTo((TrackablePocoTracker<T>)tracker);
        }

        public void RollbackTo(TrackablePocoTracker<T> tracker)
        {
            if (tracker == null)
                throw new ArgumentNullException(nameof(tracker));

            if (this == tracker)
            {
                ChangeMap.Clear();
            }
            else
            {
                foreach (var item in ChangeMap)
                    tracker.TrackSet(item.Key, item.Value.NewValue, item.Value.OldValue);
            }
        }

        // Object

        public override string ToString()
        {
            return "{ " + string.Join(", ", ChangeMap.Select(x =>
            {
                return $"{x.Key.Name}:{x.Value.OldValue}->{x.Value.NewValue}";
            }).ToArray()) + " }";
        }
    }
}
