// ------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Akka.Interfaced CodeGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using TrackableData;

#region IItem

namespace TrackableData.Sql.Tests
{
    public class TrackableItem : IItem, ITrackable<IItem>
    {
        [IgnoreDataMember]
        public IPocoTracker<IItem> Tracker { get; set; }

        public bool Changed { get { return Tracker != null && Tracker.HasChange; } }

        ITracker ITrackable.Tracker
        {
            get
            {
                return Tracker;
            }
            set
            {
                var t = (IPocoTracker<IItem>)value;
                Tracker = t;
            }
        }

        ITracker<IItem> ITrackable<IItem>.Tracker
        {
            get
            {
                return Tracker;
            }
            set
            {
                var t = (IPocoTracker<IItem>)value;
                Tracker = t;
            }
        }

        public ITrackable GetChildTrackable(object name)
        {
            switch ((string)name)
            {
                default:
                    return null;
            }
        }

        public IEnumerable<KeyValuePair<object, ITrackable>> GetChildTrackables(bool changedOnly = false)
        {
            yield break;
        }

        public static class PropertyTable
        {
            public static readonly PropertyInfo Kind = typeof(IItem).GetProperty("Kind");
            public static readonly PropertyInfo Count = typeof(IItem).GetProperty("Count");
            public static readonly PropertyInfo Note = typeof(IItem).GetProperty("Note");
        }

        private short _Kind;

        public short Kind
        {
            get
            {
                return _Kind;
            }
            set
            {
                if (Tracker != null && Kind != value)
                    Tracker.TrackSet(PropertyTable.Kind, _Kind, value);
                _Kind = value;
            }
        }

        private int _Count;

        public int Count
        {
            get
            {
                return _Count;
            }
            set
            {
                if (Tracker != null && Count != value)
                    Tracker.TrackSet(PropertyTable.Count, _Count, value);
                _Count = value;
            }
        }

        private string _Note;

        public string Note
        {
            get
            {
                return _Note;
            }
            set
            {
                if (Tracker != null && Note != value)
                    Tracker.TrackSet(PropertyTable.Note, _Note, value);
                _Note = value;
            }
        }
    }
}

#endregion

#region IPerson

namespace TrackableData.Sql.Tests
{
    public class TrackablePerson : IPerson, ITrackable<IPerson>
    {
        [IgnoreDataMember]
        public IPocoTracker<IPerson> Tracker { get; set; }

        public bool Changed { get { return Tracker != null && Tracker.HasChange; } }

        ITracker ITrackable.Tracker
        {
            get
            {
                return Tracker;
            }
            set
            {
                var t = (IPocoTracker<IPerson>)value;
                Tracker = t;
            }
        }

        ITracker<IPerson> ITrackable<IPerson>.Tracker
        {
            get
            {
                return Tracker;
            }
            set
            {
                var t = (IPocoTracker<IPerson>)value;
                Tracker = t;
            }
        }

        public ITrackable GetChildTrackable(object name)
        {
            switch ((string)name)
            {
                default:
                    return null;
            }
        }

        public IEnumerable<KeyValuePair<object, ITrackable>> GetChildTrackables(bool changedOnly = false)
        {
            yield break;
        }

        public static class PropertyTable
        {
            public static readonly PropertyInfo Id = typeof(IPerson).GetProperty("Id");
            public static readonly PropertyInfo Name = typeof(IPerson).GetProperty("Name");
            public static readonly PropertyInfo Age = typeof(IPerson).GetProperty("Age");
        }

        private int _Id;

        public int Id
        {
            get
            {
                return _Id;
            }
            set
            {
                if (Tracker != null && Id != value)
                    Tracker.TrackSet(PropertyTable.Id, _Id, value);
                _Id = value;
            }
        }

        private string _Name;

        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                if (Tracker != null && Name != value)
                    Tracker.TrackSet(PropertyTable.Name, _Name, value);
                _Name = value;
            }
        }

        private int _Age;

        public int Age
        {
            get
            {
                return _Age;
            }
            set
            {
                if (Tracker != null && Age != value)
                    Tracker.TrackSet(PropertyTable.Age, _Age, value);
                _Age = value;
            }
        }
    }
}

#endregion

#region IPersonWithIdentity

namespace TrackableData.Sql.Tests
{
    public class TrackablePersonWithIdentity : IPersonWithIdentity, ITrackable<IPersonWithIdentity>
    {
        [IgnoreDataMember]
        public IPocoTracker<IPersonWithIdentity> Tracker { get; set; }

        public bool Changed { get { return Tracker != null && Tracker.HasChange; } }

        ITracker ITrackable.Tracker
        {
            get
            {
                return Tracker;
            }
            set
            {
                var t = (IPocoTracker<IPersonWithIdentity>)value;
                Tracker = t;
            }
        }

        ITracker<IPersonWithIdentity> ITrackable<IPersonWithIdentity>.Tracker
        {
            get
            {
                return Tracker;
            }
            set
            {
                var t = (IPocoTracker<IPersonWithIdentity>)value;
                Tracker = t;
            }
        }

        public ITrackable GetChildTrackable(object name)
        {
            switch ((string)name)
            {
                default:
                    return null;
            }
        }

        public IEnumerable<KeyValuePair<object, ITrackable>> GetChildTrackables(bool changedOnly = false)
        {
            yield break;
        }

        public static class PropertyTable
        {
            public static readonly PropertyInfo Id = typeof(IPersonWithIdentity).GetProperty("Id");
            public static readonly PropertyInfo Name = typeof(IPersonWithIdentity).GetProperty("Name");
            public static readonly PropertyInfo Age = typeof(IPersonWithIdentity).GetProperty("Age");
        }

        private int _Id;

        public int Id
        {
            get
            {
                return _Id;
            }
            set
            {
                if (Tracker != null && Id != value)
                    Tracker.TrackSet(PropertyTable.Id, _Id, value);
                _Id = value;
            }
        }

        private string _Name;

        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                if (Tracker != null && Name != value)
                    Tracker.TrackSet(PropertyTable.Name, _Name, value);
                _Name = value;
            }
        }

        private int _Age;

        public int Age
        {
            get
            {
                return _Age;
            }
            set
            {
                if (Tracker != null && Age != value)
                    Tracker.TrackSet(PropertyTable.Age, _Age, value);
                _Age = value;
            }
        }
    }
}

#endregion
