using System.Collections.Generic;
using System.Reflection;

namespace TrackableData
{
    public interface ITracker
    {
        // has change
        bool HasChange { get; }

        // clear all change (not affect trackable)
        void Clear();

        // affect all changes to target object
        void ApplyTo(object trackable);

        // affect all changes to target tracker
        void ApplyTo(ITracker tracker);

        // revert all changes to target object
        void RollbackTo(object trackable);

        // revert all changes to target tracker
        void RollbackTo(ITracker tracker);
    }

    public interface ITracker<T> : ITracker
    {
        void ApplyTo(T trackable);
        void ApplyTo(ITracker<T> tracker);
        void RollbackTo(T trackable);
        void RollbackTo(ITracker<T> tracker);
    }

    // -----

    public interface IPocoTracker
    {
    }

    public interface IPocoTracker<T> : ITracker<T>, IPocoTracker
    {
        void TrackSet(PropertyInfo pi, object oldValue, object newValue);
    }

    public interface IContainerTracker
    {
    }

    public interface IContainerTracker<T> : ITracker<T>, IContainerTracker
    {
    }

    public interface IDictionaryTracker
    {
    }

    public interface IDictionaryTracker<TKey, TValue> : ITracker<IDictionary<TKey, TValue>>, IDictionaryTracker
    {
        void TrackAdd(TKey key, TValue newValue);
        void TrackRemove(TKey key, TValue oldValue);
        void TrackModify(TKey key, TValue oldValue, TValue newValue);
    }

    public interface IListTracker
    {
    }

    public interface IListTracker<T> : ITracker<IList<T>>, IListTracker
    {
        void TrackInsert(int index, T newValue);
        void TrackRemove(int index, T oldValue);
        void TrackModify(int index, T oldValue, T newValue);
    }
}
