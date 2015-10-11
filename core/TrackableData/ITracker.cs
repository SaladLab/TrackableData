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
}
