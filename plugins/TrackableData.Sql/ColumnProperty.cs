using System;
using System.Reflection;

namespace TrackableData.Sql
{
    public class ColumnProperty
    {
        public string Name { get; private set; }
        public string EscapedName { get; private set; }
        public Type Type { get; private set; }
        public int Length { get; private set; }
        public bool IsIdentity { get; private set; }
        public PropertyInfo PropertyInfo { get; private set; }
        public Func<object, string> ConvertToSqlValue { get; private set; }
        public Func<object, string> ExtractToSqlValue { get; private set; }

        public ColumnProperty(string name,
                              string escapedName,
                              Type type = null,
                              int length = 0,
                              bool isIdentity = false,
                              PropertyInfo propertyInfo = null,
                              Func<object, string> convertToSqlValue = null,
                              Func<object, string> extractToSqlValue = null)
        {
            Name = name;
            EscapedName = escapedName;
            Type = type;
            Length = length;
            IsIdentity = isIdentity;
            PropertyInfo = propertyInfo;
            ConvertToSqlValue = convertToSqlValue;
            ExtractToSqlValue = extractToSqlValue;
        }
    }
}
