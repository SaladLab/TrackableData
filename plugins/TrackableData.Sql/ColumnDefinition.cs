using System;

namespace TrackableData.Sql
{
    public class ColumnDefinition
    {
        public string Name { get; private set; }
        public Type Type { get; private set; }
        public int Length { get; private set; }

        public ColumnDefinition(string name,
                                Type type = null,
                                int length = 0)
        {
            Name = name;
            Type = type;
            Length = length;
        }
    }
}
