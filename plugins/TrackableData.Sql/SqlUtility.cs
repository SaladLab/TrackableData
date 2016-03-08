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
            if (o == DBNull.Value)
            {
                return null;
            }
            var underlyingType = Nullable.GetUnderlyingType(t);
            if (underlyingType != null)
            {
                return ConvertValue(o, underlyingType);
            }
            if (t == typeof(DateTimeOffset) && o != null && o.GetType() == typeof(DateTime))
            {
                var dt = (DateTime)o;
                if (dt.Kind == DateTimeKind.Unspecified)
                    return new DateTimeOffset((DateTime)o, TimeSpan.Zero); // MySQL
                else
                    return new DateTimeOffset(((DateTime)o).ToUniversalTime(), TimeSpan.Zero); // PostgreSQL
            }

            return Convert.ChangeType(o, t);
        }

        public static Func<object, string> GetExtractToSqlValueFunc(this ISqlProvider sqlProvider, PropertyInfo pi)
        {
            var func = sqlProvider.GetSqlValueFunc(pi.PropertyType);
            return (o => func(pi.GetValue(o)));
        }
    }
}
