using System;
using System.Data.Common;

namespace TrackableData.Sql
{
    public interface ISqlProvider
    {
        // ex: type(string) -> (v -> "N'" + v.Replace("'", "''") + "'")
        Func<object, string> GetConvertToSqlValueFunc(Type type);

        // ex: type(string) -> (o -> o != DbNull ? o : null)
        Func<object, object> GetConvertFromDbValueFunc(Type type);

        // ex: name -> [name]
        string EscapeName(string name);

        // CREATE TABLE [tableEscapedName] { columns } WITH primaryKeys
        string BuildCreateTableSql(string tableName,
                                   ColumnProperty[] columns,
                                   ColumnProperty[] primaryKeys,
                                   bool dropIfExists);

        // INSERT INTO [tableEscapedName] (columns) OUTPUT INSERTED.identity VALUES (values)
        string BuildInsertIntoSql(string tableName,
                                  string columns,
                                  string values,
                                  ColumnProperty identity);

        DbCommand CreateDbCommand(string sql, DbConnection connection);
    }
}
