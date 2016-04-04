using System;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using Npgsql;
using TrackableData.Sql;

namespace TrackableData.PostgreSql
{
    public class PostgreSqlProvider : ISqlProvider
    {
        private static readonly Lazy<PostgreSqlProvider> s_instance =
            new Lazy<PostgreSqlProvider>(() => new PostgreSqlProvider());

        public static PostgreSqlProvider Instance
        {
            get { return s_instance.Value; }
        }

        private static readonly MethodInfo s_methodForGetConvertToSqlValueFuncForNullable =
            typeof(PostgreSqlProvider).GetMethod("GetConvertToSqlValueFuncForNullable",
                                                 BindingFlags.Instance | BindingFlags.Public);

        public string GetSqlType(Type type, int length = 0)
        {
            var lengthStr = length > 0 ? length.ToString() : "10000"; // TODO: BLOB for MAX
            if (type == typeof(bool))
                return "BOOLEAN";
            if (type == typeof(byte)) // PostgreSQL doesn't provide 1 byte integer.
                return "SMALLINT";
            if (type == typeof(short))
                return "SMALLINT";
            if (type == typeof(char))
                return "INTEGER";
            if (type == typeof(int))
                return "INTEGER";
            if (type == typeof(long))
                return "BIGINT";
            if (type == typeof(float))
                return "FLOAT4";
            if (type == typeof(double))
                return "FLOAT8";
            if (type == typeof(decimal))
                return "NUMERIC";
            if (type == typeof(DateTime))
                return "TIMESTAMP";
            if (type == typeof(DateTimeOffset))
                return "TIMESTAMPTZ";
            if (type == typeof(TimeSpan))
                return "TIME";
            if (type == typeof(string))
                return $"VARCHAR({lengthStr})";
            if (type == typeof(byte[]))
                return $"BYTEA";
            if (type == typeof(Guid))
                return "UUID";
            if (type.IsEnum)
                return GetSqlType(Enum.GetUnderlyingType(type));
            return "";
        }

        public Func<object, string> GetConvertToSqlValueFunc(Type type)
        {
            if (type == typeof(bool))
                return (o => (bool)o ? "TRUE" : "FALSE");
            if (type == typeof(char))
                return (o => ((int)(char)o).ToString());
            if (type == typeof(DateTime))
                return (o => GetSqlValue((DateTime)o));
            if (type == typeof(DateTimeOffset))
                return (o => GetSqlValue((DateTimeOffset)o));
            if (type == typeof(TimeSpan))
                return (o => GetSqlValue((TimeSpan)o));
            if (type == typeof(string))
                return (o => o != null ? GetSqlValue((string)o) : "NULL");
            if (type == typeof(byte[]))
                return (o => o != null ? GetSqlValue((byte[])o) : "NULL");
            if (type == typeof(Guid))
                return (o => GetSqlValue((Guid)o));
            if (type.IsEnum)
                return (o => Convert.ToInt64(o).ToString(CultureInfo.InvariantCulture));
            if (Nullable.GetUnderlyingType(type) != null)
            {
                return (Func<object, string>)s_methodForGetConvertToSqlValueFuncForNullable
                           .MakeGenericMethod(Nullable.GetUnderlyingType(type))
                           .Invoke(this, new object[] { });
            }
            return (o => o.ToString());
        }

        public Func<object, string> GetConvertToSqlValueFuncForNullable<T>()
            where T : struct
        {
            var func = GetConvertToSqlValueFunc(typeof(T));
            return (o =>
            {
                var v = (T?)o;
                return v.HasValue ? func(v.Value) : "NULL";
            });
        }

        public static string GetSqlValue(DateTime value)
        {
            return "'" + value.ToString(@"yyyy-MM-dd HH\:mm\:ss.fffffff") + "'";
        }

        public static string GetSqlValue(DateTimeOffset value)
        {
            return "'" + value.ToString(@"yyyy-MM-dd HH\:mm\:ss.fffffffzzz") + "'";
        }

        public static string GetSqlValue(TimeSpan value)
        {
            return "'" + value.ToString() + "'";
        }

        public static string GetSqlValue(string value)
        {
            return "'" + value.Replace("'", "''") + "'";
        }

        public static string GetSqlValue(byte[] value)
        {
            var sb = new StringBuilder((value.Length * 2) + 6);

            sb.Append("E'\\\\x");
            foreach (var b in value)
                sb.Append(b.ToString("X2", CultureInfo.InvariantCulture));
            sb.Append("'");

            return sb.ToString();
        }

        public static string GetSqlValue(Guid value)
        {
            return "'" + value.ToString() + "'";
        }

        public Func<object, object> GetConvertFromDbValueFunc(Type type)
        {
            if (type == typeof(string))
                return (o => o != DBNull.Value ? Convert.ChangeType(o, type) : null);
            if (type == typeof(byte[]))
                return (o => o != DBNull.Value ? Convert.ChangeType(o, type) : null);
            // PostgreSQL treats DateTimeOffset as DateTime at UTC
            if (type == typeof(DateTimeOffset))
                return (o => new DateTimeOffset(((DateTime)o).ToUniversalTime(), TimeSpan.Zero));
            if (type.IsEnum)
                return (o => Enum.ToObject(type, o));
            if (Nullable.GetUnderlyingType(type) != null)
            {
                var underlyingFunc = GetConvertFromDbValueFunc(Nullable.GetUnderlyingType(type));
                return (o => o != DBNull.Value ? underlyingFunc(o) : null);
            }
            return (o => Convert.ChangeType(o, type));
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
                    if (Nullable.GetUnderlyingType(c.Type) != null)
                    {
                        var underlyingType = Nullable.GetUnderlyingType(c.Type);
                        var identity = c.IsIdentity ? $" DEFAULT nextval('\"{tableName}_{c.Name}_seq\"')" : "";
                        return $"{c.EscapedName} {GetSqlType(underlyingType, c.Length)}{identity}";
                    }
                    else
                    {
                        var identity = c.IsIdentity ? $" DEFAULT nextval('\"{tableName}_{c.Name}_seq\"')" : "";
                        var notnull = c.Type.IsValueType ? " NOT NULL" : "";
                        return $"{c.EscapedName} {GetSqlType(c.Type, c.Length)}{identity}{notnull}";
                    }
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
