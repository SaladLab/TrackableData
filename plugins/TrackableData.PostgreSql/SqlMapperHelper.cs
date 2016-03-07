using System;
using System.Globalization;
using System.Reflection;

namespace TrackableData.PostgreSql
{
    public static class SqlMapperHelper
    {
        public static string GetEscapedName(string name)
        {
            return '"' + name + '"';
        }

        public static string GetEscapedValue(string value)
        {
            return "N'" + value.Replace("'", "''") + "'";
        }

        public static string GetEscapedValue(DateTime value)
        {
            return "'" + value.ToString("yyyy-MM-dd HH:mm:ss") + "'";
        }

        public static Func<object, string> GetSqlValueFunc(Type type)
        {
            if (type == typeof(DateTime))
            {
                return (o => GetEscapedValue((DateTime)o));
            }
            else if (type == typeof(string))
            {
                return (o => o != null ? GetEscapedValue((string)o) : "NULL");
            }
            else if (type == typeof(bool))
            {
                return (o => (bool)o ? "1" : "0");
            }
            else if (type.IsEnum)
            {
                return (o => Convert.ToInt32(o).ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                return (o => o.ToString());
            }
        }

        public static Func<object, string> GetSqlValueFunc(FieldInfo fi)
        {
            var func = GetSqlValueFunc(fi.FieldType);
            return (o => func(fi.GetValue(o)));
        }

        public static Func<object, string> GetSqlValueFunc(PropertyInfo pi)
        {
            var func = GetSqlValueFunc(pi.PropertyType);
            return (o => func(pi.GetValue(o)));
        }

        public static object GetNetValue(object o, Type t)
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

        public static string GetSqlType(Type type, int length = 0)
        {
            var lengthStr = length > 0 ? length.ToString() : "10000"; // TODO: BLOB for MAX
            if (type == typeof(bool))
                return "boolean";
            if (type == typeof(byte)) // PostgreSQL doesn't provide 1 byte integer.
                return "smallint";
            if (type == typeof(int))
                return "integer";
            if (type == typeof(long))
                return "bigint";
            if (type == typeof(short))
                return "smallint";
            if (type == typeof(float))
                return "float4";
            if (type == typeof(double))
                return "float8 ";
            if (type == typeof(DateTime))
                return "timestamp";
            if (type == typeof(DateTimeOffset))
                return "timestamptz";
            if (type == typeof(string))
                return $"VARCHAR({lengthStr})";
            if (type == typeof(byte[]))
                return $"VARBINARY({lengthStr})";
            if (type == typeof(Guid))
                return "[uniqueidentifier]";
            return "";
        }
    }
}
