using System;

namespace TrackableData
{
    public sealed class TrackableFieldAttribute : Attribute
    {
        public string ColumnName;

        public TrackableFieldAttribute(string columnName = null)
        {
            ColumnName = columnName;
        }

        public bool IsColumnIgnored
        {
            get { return ColumnName == "^"; }
        }
    }
}
