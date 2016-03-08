using Npgsql;
using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using TrackableData.Sql;

namespace TrackableData.PostgreSql
{
    public class PostgreSqlProvider : ISqlProvider
    {
        private static readonly Lazy<PostgreSqlProvider> _instance = new Lazy<PostgreSqlProvider>(() => new PostgreSqlProvider());

        public static PostgreSqlProvider Instance { get { return _instance.Value; } }

        public string GetSqlType(Type type, int length = 0)
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
            return "N'" + value.Replace("'", "''") + "'";
        }

        public static string GetSqlValue(DateTime value)
        {
            return "'" + value.ToString("yyyy-MM-dd HH:mm:ss") + "'";
        }

        public string EscapeName(string name)
        {
            return "\"" + name + "\"";
        }

        public string BuildCreateTableSql(string tableName,
                                          ColumnProperty[] columns,
                                          ColumnProperty[] primaryKeys,
                                          bool dropIfExists)
        {
            var identityColumn = columns.FirstOrDefault(c => c.IsIdentity);
            var columnDef = string.Join(
                ",\n",
                columns.Select(c =>
                {
                    var identity = c.IsIdentity ? $" DEFAULT nextval('\"{tableName}_{c.Name}_seq\"')" : "";
                    var notnull = c.Type.IsValueType ? " NOT NULL" : "";
                    return $"{c.EscapedName} {GetSqlType(c.Type, c.Length)}{identity}{notnull}";
                }));

            var primaryKeyDef = string.Join(
                ",",
                primaryKeys.Select(c => $"{c.EscapedName}"));

            var sb = new StringBuilder();
            var tableEscapedName = EscapeName(tableName);

            if (dropIfExists)
                sb.AppendLine($"DROP TABLE IF EXISTS {tableEscapedName};");

            if (identityColumn != null)
                sb.AppendLine($"CREATE SEQUENCE \"{tableName}_{identityColumn.Name}_seq\";");

            sb.AppendLine($"CREATE TABLE {tableEscapedName} (");
            sb.AppendLine(columnDef);
            sb.AppendLine($", PRIMARY KEY ({primaryKeyDef})");
            sb.AppendLine(");");

            if (identityColumn != null)
            {
                sb.AppendLine($"ALTER SEQUENCE \"{tableName}_{identityColumn.Name}_seq\" " +
                              $"OWNED BY {tableEscapedName}.{identityColumn.EscapedName};");
            }
            return sb.ToString();
        }

        public string BuildInsertIntoSql(string tableName,
                                         string columns,
                                         string values,
                                         ColumnProperty identity)
        {
            var returnClause = identity != null ? $"RETURNING {identity.EscapedName}" : "";
            return $"INSERT INTO {EscapeName(tableName)} ({columns}) VALUES ({values}) {returnClause};";
        }

        public DbCommand CreateDbCommand(string sql, DbConnection connection)
        {
            var sqlConnection = (NpgsqlConnection)connection;
            if (sqlConnection == null)
                throw new ArgumentNullException(nameof(connection));

            return new NpgsqlCommand(sql, sqlConnection);
        }
    }
}
