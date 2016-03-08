using System;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using TrackableData.Sql;

namespace TrackableData.MySql
{
    public class MySqlProvider : ISqlProvider
    {
        private static readonly Lazy<MySqlProvider> _instance = new Lazy<MySqlProvider>(() => new MySqlProvider());

        public static MySqlProvider Instance
        {
            get { return _instance.Value; }
        }

        public string GetSqlType(Type type, int length = 0)
        {
            var lengthStr = length > 0 ? length.ToString() : "10000"; // TODO: BLOB for MAX
            if (type == typeof(bool))
                return "BIT";
            if (type == typeof(byte))
                return "TINYINT";
            if (type == typeof(int))
                return "INT";
            if (type == typeof(long))
                return "BIGINT";
            if (type == typeof(short))
                return "SMALLINT";
            if (type == typeof(float))
                return "FLOAT";
            if (type == typeof(double))
                return "DOUBLE ";
            if (type == typeof(DateTime))
                return "[datetime2]";
            if (type == typeof(DateTimeOffset))
                return "[datetimeoffset]";
            if (type == typeof(string))
                return $"VARCHAR({lengthStr}) CHARACTER SET utf8";
            if (type == typeof(byte[]))
                return $"VARBINARY({lengthStr})";
            if (type == typeof(Guid))
                return "[uniqueidentifier]";
            return "";
        }

        public Func<object, string> GetSqlValueFunc(Type type)
        {
            if (type == typeof(DateTime))
            {
                return (o => GetSqlValue((DateTime)o));
            }
            else if (type == typeof(string))
            {
                return (o => o != null ? GetSqlValue((string)o) : "NULL");
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

        public static string GetSqlValue(string value)
        {
            return "'" + value.Replace("'", "''") + "'";
        }

        public static string GetSqlValue(DateTime value)
        {
            return "'" + value.ToString("yyyy-MM-dd HH:mm:ss") + "'";
        }

        public string EscapeName(string name)
        {
            return "`" + name + "`";
        }

        public string BuildCreateTableSql(string tableName,
                                          ColumnProperty[] columns,
                                          ColumnProperty[] primaryKeys,
                                          bool dropIfExists)
        {
            var columnDef = string.Join(
                ",\n",
                columns.Select(c =>
                {
                    var identity = c.IsIdentity ? " AUTO_INCREMENT" : "";
                    var notnull = c.Type.IsValueType ? " NOT NULL" : "";
                    return $"{c.EscapedName} {GetSqlType(c.Type, c.Length)}{identity}{notnull}";
                }));

            var primaryKeyDef = string.Join(
                ",",
                primaryKeys.Select(c => c.EscapedName));

            var sb = new StringBuilder();
            var tableEscapedName = EscapeName(tableName);
            if (dropIfExists)
                sb.AppendLine($"DROP TABLE IF EXISTS {tableEscapedName};");
            sb.AppendLine($"CREATE TABLE {tableEscapedName} (");
            sb.AppendLine(columnDef);
            sb.AppendLine($", PRIMARY KEY ({primaryKeyDef})");
            sb.AppendLine(");");
            return sb.ToString();
        }

        public string BuildInsertIntoSql(string tableName,
                                         string columns,
                                         string values,
                                         ColumnProperty identity)
        {
            var selectLastId = identity != null ? "SELECT LAST_INSERT_ID();" : "";
            return $"INSERT INTO {EscapeName(tableName)} ({columns}) VALUES ({values});\n{selectLastId}";
        }

        public DbCommand CreateDbCommand(string sql, DbConnection connection)
        {
            var sqlConnection = (MySqlConnection)connection;
            if (sqlConnection == null)
                throw new ArgumentNullException(nameof(connection));

            return new MySqlCommand(sql, sqlConnection);
        }
    }
}
