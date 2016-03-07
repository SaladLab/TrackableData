using System;
using System.Reflection;

namespace TrackableData.Sql
{
    internal class ColumnProperty
    {
        public string Name;
        public string EscapedName;
        public Type Type;
        public int Length;
        public bool IsIdentity;
        public PropertyInfo PropertyInfo;
        public Func<object, string> ConvertToSqlValue;
    }
}
