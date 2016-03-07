using System;

namespace TrackableData.Sql
{
    internal class ColumnDefinition
    {
        public string Name;
        public Type Type;
        public int Length;

        public ColumnDefinition(string name, Type type = null, int length = 0)
        {
            Name = name;
            Type = type;
            Length = length;
        }
    }
}
