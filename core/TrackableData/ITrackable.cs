using System;
using System.Collections.Generic;
using System.Reflection;

namespace TrackableData
{
    public interface ITrackable
    {
        bool Changed { get; }

        ITracker Tracker { get; set; }

        IEnumerable<ITrackable> ChildrenTrackables { get; }
    }

    public interface ITracker
    {
        bool HasChange { get; }
        void Clear();
        void ApplyTo(object obj);
        void ApplyTo(ITracker tracker);
        void RollbackTo(object obj);
    }
}
