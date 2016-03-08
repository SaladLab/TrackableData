using System;
using System.Reflection;

namespace TrackableData.Sql
{
    internal static class SqlUtility
    {
        public static object ConvertValue(object o, Type t)
        {
            if (t.IsEnum)
            {
                return Enum.ToObject(t, o);
            }
            else
            {
                return Convert.ChangeType(o, t);
            }
        }

        public static Func<object, string> GetExtractToSqlValueFunc(this ISqlProvider sqlProvider, PropertyInfo pi)
        {
            var func = sqlProvider.GetSqlValueFunc(pi.PropertyType);
            return (o => func(pi.GetValue(o)));
        }
    }
}
