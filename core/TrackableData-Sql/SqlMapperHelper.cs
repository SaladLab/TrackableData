using System;
using System.Globalization;
using System.Reflection;

namespace TrackableData
{
    public static class SqlMapperHelper
    {
        public static string GetEscapedName(string name)
        {
            return "[" + name + "]";
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
                return (o => GetEscapedValue((string)o));
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

        public static string GetSqlType(Type type)
        {
            if (type == typeof (int))
                return "[int]";
            if (type == typeof(long))
                return "[bigint]";
            if (type == typeof(string))
                return "[nvarchar](MAX)";
            return "";
        }
    }
}
