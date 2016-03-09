using System;
using System.Reflection;

namespace TrackableData.Sql
{
    internal static class SqlUtility
    {
        public static Func<object, string> GetExtractToSqlValueFunc(this ISqlProvider sqlProvider, PropertyInfo pi)
        {
            var func = sqlProvider.GetConvertToSqlValueFunc(pi.PropertyType);
            return (o => func(pi.GetValue(o)));
        }

        public static Action<object, object> GetInstallFromDbValueFunc(this ISqlProvider sqlProvider, PropertyInfo pi)
        {
            var func = sqlProvider.GetConvertFromDbValueFunc(pi.PropertyType);
            return ((o, v) => pi.SetValue(o, func(v)));
        }
    }
}
