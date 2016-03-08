using System;
using System.Data.Common;

namespace TrackableData.Sql
{
    public interface ISqlProvider
    {
        // ex: string(10) -> varchar(10)
        string GetSqlType(Type type, int length = 0);

        // ex: type(string) -> (v -> "N'" + v.Replace("'", "''") + "'")
        Func<object, string> GetSqlValueFunc(Type type);

        // ex: name -> [name]
        string EscapeName(string name);

        // CREATE TABLE [tableEscapedName] { columns } WITH primaryKeys
        string BuildCreateTableSql(string tableEscapedName,
                                   ColumnProperty[] columns,
                                   ColumnProperty[] primaryKeys,
                                   bool dropIfExists);

        // INSERT INTO [tableEscapedName] (columns) OUTPUT INSERTED.identity VALUES (values)
        string BuildInsertIntoSql(string tableEscapedName,
                                  string columns,
                                  string values,
                                  ColumnProperty identity);

        DbCommand CreateDbCommand(string sql, DbConnection connection);
    }
}
