using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace TrackableData
{
    public class TrackablePocoTracker<T> : ITracker
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
            if ((obj is T) == false)
                throw new ArgumentException("obj");

            foreach (var item in ChangeMap)
            {
                var setter = item.Key.GetSetMethod().GetBaseDefinition();
                setter.Invoke(obj, new[] { item.Value.NewValue });
            }
        }

        public void ApplyTo(ITracker tracker)
        {
            var t = (TrackablePocoTracker<T>)tracker;
            if (t == null)
                throw new ArgumentException("tracker");

            ApplyTo(t);
        }

        public void ApplyTo(TrackablePocoTracker<T> tracker)
        {
            foreach (var item in ChangeMap)
                tracker.TrackSet(item.Key, item.Value.NewValue, item.Value.OldValue);
        }

        public void RollbackTo(object obj)
        {
            if ((obj is T) == false)
                throw new ArgumentException("obj");

            foreach (var item in ChangeMap)
            {
                var setter = item.Key.GetSetMethod().GetBaseDefinition();
                setter.Invoke(obj, new[] { item.Value.OldValue });
            }
        }

        // Object

        public override string ToString()
        {
            return "{ " + string.Join(", ", ChangeMap.Select(x => x.Key.Name + ":" + x.Value)) + " }";
        }
    }
}
