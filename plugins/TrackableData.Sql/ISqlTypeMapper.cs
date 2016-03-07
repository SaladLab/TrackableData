using System;

namespace TrackableData.Sql
{
    public interface ISqlTypeMapper
    {
        // ex: string(10) -> varchar(10)
        string GetSqlType(Type type, int length = 0);

        // ex: type(string) -> (v -> "N'" + v.Replace("'", "''") + "'")
        Func<object, string> GetSqlValueFunc(Type type);
    }
}
