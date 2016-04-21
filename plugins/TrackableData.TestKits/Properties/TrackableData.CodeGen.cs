﻿// ------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by TrackableData.CodeGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Linq;
using System.Text;
using TrackableData;
using System.Threading.Tasks;
using Xunit;

#region ITypeNullableTestPoco

namespace TrackableData.TestKits
{
    public partial class TrackableTypeNullableTestPoco : ITypeNullableTestPoco
    {
        [IgnoreDataMember]
        public IPocoTracker<ITypeNullableTestPoco> Tracker { get; set; }

        [IgnoreDataMember]
        public bool Changed { get { return Tracker != null && Tracker.HasChange; } }

        ITracker ITrackable.Tracker
        {
            get
            {
                return Tracker;
            }
            set
            {
                var t = (IPocoTracker<ITypeNullableTestPoco>)value;
                Tracker = t;
            }
        }

        ITracker<ITypeNullableTestPoco> ITrackable<ITypeNullableTestPoco>.Tracker
        {
            get
            {
                return Tracker;
            }
            set
            {
                var t = (IPocoTracker<ITypeNullableTestPoco>)value;
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
            public static readonly PropertyInfo Id = typeof(ITypeNullableTestPoco).GetProperty("Id");
            public static readonly PropertyInfo ValBool = typeof(ITypeNullableTestPoco).GetProperty("ValBool");
            public static readonly PropertyInfo ValByte = typeof(ITypeNullableTestPoco).GetProperty("ValByte");
            public static readonly PropertyInfo ValShort = typeof(ITypeNullableTestPoco).GetProperty("ValShort");
            public static readonly PropertyInfo ValChar = typeof(ITypeNullableTestPoco).GetProperty("ValChar");
            public static readonly PropertyInfo ValInt = typeof(ITypeNullableTestPoco).GetProperty("ValInt");
            public static readonly PropertyInfo ValLong = typeof(ITypeNullableTestPoco).GetProperty("ValLong");
            public static readonly PropertyInfo ValFloat = typeof(ITypeNullableTestPoco).GetProperty("ValFloat");
            public static readonly PropertyInfo ValDouble = typeof(ITypeNullableTestPoco).GetProperty("ValDouble");
            public static readonly PropertyInfo ValDecimal = typeof(ITypeNullableTestPoco).GetProperty("ValDecimal");
            public static readonly PropertyInfo ValDateTime = typeof(ITypeNullableTestPoco).GetProperty("ValDateTime");
            public static readonly PropertyInfo ValDateTimeOffset = typeof(ITypeNullableTestPoco).GetProperty("ValDateTimeOffset");
            public static readonly PropertyInfo ValTimeSpan = typeof(ITypeNullableTestPoco).GetProperty("ValTimeSpan");
            public static readonly PropertyInfo ValString = typeof(ITypeNullableTestPoco).GetProperty("ValString");
            public static readonly PropertyInfo ValBytes = typeof(ITypeNullableTestPoco).GetProperty("ValBytes");
            public static readonly PropertyInfo ValGuid = typeof(ITypeNullableTestPoco).GetProperty("ValGuid");
            public static readonly PropertyInfo ValEnum = typeof(ITypeNullableTestPoco).GetProperty("ValEnum");
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

        private bool? _ValBool;

        public bool? ValBool
        {
            get
            {
                return _ValBool;
            }
            set
            {
                if (Tracker != null && ValBool != value)
                    Tracker.TrackSet(PropertyTable.ValBool, _ValBool, value);
                _ValBool = value;
            }
        }

        private byte? _ValByte;

        public byte? ValByte
        {
            get
            {
                return _ValByte;
            }
            set
            {
                if (Tracker != null && ValByte != value)
                    Tracker.TrackSet(PropertyTable.ValByte, _ValByte, value);
                _ValByte = value;
            }
        }

        private short? _ValShort;

        public short? ValShort
        {
            get
            {
                return _ValShort;
            }
            set
            {
                if (Tracker != null && ValShort != value)
                    Tracker.TrackSet(PropertyTable.ValShort, _ValShort, value);
                _ValShort = value;
            }
        }

        private char? _ValChar;

        public char? ValChar
        {
            get
            {
                return _ValChar;
            }
            set
            {
                if (Tracker != null && ValChar != value)
                    Tracker.TrackSet(PropertyTable.ValChar, _ValChar, value);
                _ValChar = value;
            }
        }

        private int? _ValInt;

        public int? ValInt
        {
            get
            {
                return _ValInt;
            }
            set
            {
                if (Tracker != null && ValInt != value)
                    Tracker.TrackSet(PropertyTable.ValInt, _ValInt, value);
                _ValInt = value;
            }
        }

        private long? _ValLong;

        public long? ValLong
        {
            get
            {
                return _ValLong;
            }
            set
            {
                if (Tracker != null && ValLong != value)
                    Tracker.TrackSet(PropertyTable.ValLong, _ValLong, value);
                _ValLong = value;
            }
        }

        private float? _ValFloat;

        public float? ValFloat
        {
            get
            {
                return _ValFloat;
            }
            set
            {
                if (Tracker != null && ValFloat != value)
                    Tracker.TrackSet(PropertyTable.ValFloat, _ValFloat, value);
                _ValFloat = value;
            }
        }

        private double? _ValDouble;

        public double? ValDouble
        {
            get
            {
                return _ValDouble;
            }
            set
            {
                if (Tracker != null && ValDouble != value)
                    Tracker.TrackSet(PropertyTable.ValDouble, _ValDouble, value);
                _ValDouble = value;
            }
        }

        private decimal? _ValDecimal;

        public decimal? ValDecimal
        {
            get
            {
                return _ValDecimal;
            }
            set
            {
                if (Tracker != null && ValDecimal != value)
                    Tracker.TrackSet(PropertyTable.ValDecimal, _ValDecimal, value);
                _ValDecimal = value;
            }
        }

        private DateTime? _ValDateTime;

        public DateTime? ValDateTime
        {
            get
            {
                return _ValDateTime;
            }
            set
            {
                if (Tracker != null && ValDateTime != value)
                    Tracker.TrackSet(PropertyTable.ValDateTime, _ValDateTime, value);
                _ValDateTime = value;
            }
        }

        private DateTimeOffset? _ValDateTimeOffset;

        public DateTimeOffset? ValDateTimeOffset
        {
            get
            {
                return _ValDateTimeOffset;
            }
            set
            {
                if (Tracker != null && ValDateTimeOffset != value)
                    Tracker.TrackSet(PropertyTable.ValDateTimeOffset, _ValDateTimeOffset, value);
                _ValDateTimeOffset = value;
            }
        }

        private TimeSpan? _ValTimeSpan;

        public TimeSpan? ValTimeSpan
        {
            get
            {
                return _ValTimeSpan;
            }
            set
            {
                if (Tracker != null && ValTimeSpan != value)
                    Tracker.TrackSet(PropertyTable.ValTimeSpan, _ValTimeSpan, value);
                _ValTimeSpan = value;
            }
        }

        private string _ValString;

        public string ValString
        {
            get
            {
                return _ValString;
            }
            set
            {
                if (Tracker != null && ValString != value)
                    Tracker.TrackSet(PropertyTable.ValString, _ValString, value);
                _ValString = value;
            }
        }

        private byte[] _ValBytes;

        public byte[] ValBytes
        {
            get
            {
                return _ValBytes;
            }
            set
            {
                if (Tracker != null && ValBytes != value)
                    Tracker.TrackSet(PropertyTable.ValBytes, _ValBytes, value);
                _ValBytes = value;
            }
        }

        private Guid? _ValGuid;

        public Guid? ValGuid
        {
            get
            {
                return _ValGuid;
            }
            set
            {
                if (Tracker != null && ValGuid != value)
                    Tracker.TrackSet(PropertyTable.ValGuid, _ValGuid, value);
                _ValGuid = value;
            }
        }

        private TestEnum? _ValEnum;

        public TestEnum? ValEnum
        {
            get
            {
                return _ValEnum;
            }
            set
            {
                if (Tracker != null && ValEnum != value)
                    Tracker.TrackSet(PropertyTable.ValEnum, _ValEnum, value);
                _ValEnum = value;
            }
        }
    }
}

#endregion
#region ITypeTestPoco

namespace TrackableData.TestKits
{
    public partial class TrackableTypeTestPoco : ITypeTestPoco
    {
        [IgnoreDataMember]
        public IPocoTracker<ITypeTestPoco> Tracker { get; set; }

        [IgnoreDataMember]
        public bool Changed { get { return Tracker != null && Tracker.HasChange; } }

        ITracker ITrackable.Tracker
        {
            get
            {
                return Tracker;
            }
            set
            {
                var t = (IPocoTracker<ITypeTestPoco>)value;
                Tracker = t;
            }
        }

        ITracker<ITypeTestPoco> ITrackable<ITypeTestPoco>.Tracker
        {
            get
            {
                return Tracker;
            }
            set
            {
                var t = (IPocoTracker<ITypeTestPoco>)value;
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
            public static readonly PropertyInfo Id = typeof(ITypeTestPoco).GetProperty("Id");
            public static readonly PropertyInfo ValBool = typeof(ITypeTestPoco).GetProperty("ValBool");
            public static readonly PropertyInfo ValByte = typeof(ITypeTestPoco).GetProperty("ValByte");
            public static readonly PropertyInfo ValShort = typeof(ITypeTestPoco).GetProperty("ValShort");
            public static readonly PropertyInfo ValChar = typeof(ITypeTestPoco).GetProperty("ValChar");
            public static readonly PropertyInfo ValInt = typeof(ITypeTestPoco).GetProperty("ValInt");
            public static readonly PropertyInfo ValLong = typeof(ITypeTestPoco).GetProperty("ValLong");
            public static readonly PropertyInfo ValFloat = typeof(ITypeTestPoco).GetProperty("ValFloat");
            public static readonly PropertyInfo ValDouble = typeof(ITypeTestPoco).GetProperty("ValDouble");
            public static readonly PropertyInfo ValDecimal = typeof(ITypeTestPoco).GetProperty("ValDecimal");
            public static readonly PropertyInfo ValDateTime = typeof(ITypeTestPoco).GetProperty("ValDateTime");
            public static readonly PropertyInfo ValDateTimeOffset = typeof(ITypeTestPoco).GetProperty("ValDateTimeOffset");
            public static readonly PropertyInfo ValTimeSpan = typeof(ITypeTestPoco).GetProperty("ValTimeSpan");
            public static readonly PropertyInfo ValString = typeof(ITypeTestPoco).GetProperty("ValString");
            public static readonly PropertyInfo ValBytes = typeof(ITypeTestPoco).GetProperty("ValBytes");
            public static readonly PropertyInfo ValGuid = typeof(ITypeTestPoco).GetProperty("ValGuid");
            public static readonly PropertyInfo ValEnum = typeof(ITypeTestPoco).GetProperty("ValEnum");
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

        private bool _ValBool;

        public bool ValBool
        {
            get
            {
                return _ValBool;
            }
            set
            {
                if (Tracker != null && ValBool != value)
                    Tracker.TrackSet(PropertyTable.ValBool, _ValBool, value);
                _ValBool = value;
            }
        }

        private byte _ValByte;

        public byte ValByte
        {
            get
            {
                return _ValByte;
            }
            set
            {
                if (Tracker != null && ValByte != value)
                    Tracker.TrackSet(PropertyTable.ValByte, _ValByte, value);
                _ValByte = value;
            }
        }

        private short _ValShort;

        public short ValShort
        {
            get
            {
                return _ValShort;
            }
            set
            {
                if (Tracker != null && ValShort != value)
                    Tracker.TrackSet(PropertyTable.ValShort, _ValShort, value);
                _ValShort = value;
            }
        }

        private char _ValChar;

        public char ValChar
        {
            get
            {
                return _ValChar;
            }
            set
            {
                if (Tracker != null && ValChar != value)
                    Tracker.TrackSet(PropertyTable.ValChar, _ValChar, value);
                _ValChar = value;
            }
        }

        private int _ValInt;

        public int ValInt
        {
            get
            {
                return _ValInt;
            }
            set
            {
                if (Tracker != null && ValInt != value)
                    Tracker.TrackSet(PropertyTable.ValInt, _ValInt, value);
                _ValInt = value;
            }
        }

        private long _ValLong;

        public long ValLong
        {
            get
            {
                return _ValLong;
            }
            set
            {
                if (Tracker != null && ValLong != value)
                    Tracker.TrackSet(PropertyTable.ValLong, _ValLong, value);
                _ValLong = value;
            }
        }

        private float _ValFloat;

        public float ValFloat
        {
            get
            {
                return _ValFloat;
            }
            set
            {
                if (Tracker != null && ValFloat != value)
                    Tracker.TrackSet(PropertyTable.ValFloat, _ValFloat, value);
                _ValFloat = value;
            }
        }

        private double _ValDouble;

        public double ValDouble
        {
            get
            {
                return _ValDouble;
            }
            set
            {
                if (Tracker != null && ValDouble != value)
                    Tracker.TrackSet(PropertyTable.ValDouble, _ValDouble, value);
                _ValDouble = value;
            }
        }

        private decimal _ValDecimal;

        public decimal ValDecimal
        {
            get
            {
                return _ValDecimal;
            }
            set
            {
                if (Tracker != null && ValDecimal != value)
                    Tracker.TrackSet(PropertyTable.ValDecimal, _ValDecimal, value);
                _ValDecimal = value;
            }
        }

        private DateTime _ValDateTime;

        public DateTime ValDateTime
        {
            get
            {
                return _ValDateTime;
            }
            set
            {
                if (Tracker != null && ValDateTime != value)
                    Tracker.TrackSet(PropertyTable.ValDateTime, _ValDateTime, value);
                _ValDateTime = value;
            }
        }

        private DateTimeOffset _ValDateTimeOffset;

        public DateTimeOffset ValDateTimeOffset
        {
            get
            {
                return _ValDateTimeOffset;
            }
            set
            {
                if (Tracker != null && ValDateTimeOffset != value)
                    Tracker.TrackSet(PropertyTable.ValDateTimeOffset, _ValDateTimeOffset, value);
                _ValDateTimeOffset = value;
            }
        }

        private TimeSpan _ValTimeSpan;

        public TimeSpan ValTimeSpan
        {
            get
            {
                return _ValTimeSpan;
            }
            set
            {
                if (Tracker != null && ValTimeSpan != value)
                    Tracker.TrackSet(PropertyTable.ValTimeSpan, _ValTimeSpan, value);
                _ValTimeSpan = value;
            }
        }

        private string _ValString;

        public string ValString
        {
            get
            {
                return _ValString;
            }
            set
            {
                if (Tracker != null && ValString != value)
                    Tracker.TrackSet(PropertyTable.ValString, _ValString, value);
                _ValString = value;
            }
        }

        private byte[] _ValBytes;

        public byte[] ValBytes
        {
            get
            {
                return _ValBytes;
            }
            set
            {
                if (Tracker != null && ValBytes != value)
                    Tracker.TrackSet(PropertyTable.ValBytes, _ValBytes, value);
                _ValBytes = value;
            }
        }

        private Guid _ValGuid;

        public Guid ValGuid
        {
            get
            {
                return _ValGuid;
            }
            set
            {
                if (Tracker != null && ValGuid != value)
                    Tracker.TrackSet(PropertyTable.ValGuid, _ValGuid, value);
                _ValGuid = value;
            }
        }

        private TestEnum _ValEnum;

        public TestEnum ValEnum
        {
            get
            {
                return _ValEnum;
            }
            set
            {
                if (Tracker != null && ValEnum != value)
                    Tracker.TrackSet(PropertyTable.ValEnum, _ValEnum, value);
                _ValEnum = value;
            }
        }
    }
}

#endregion