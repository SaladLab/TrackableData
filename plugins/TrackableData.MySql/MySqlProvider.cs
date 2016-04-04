using System;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using MySql.Data.MySqlClient;
using TrackableData.Sql;

namespace TrackableData.MySql
{
    public class MySqlProvider : ISqlProvider
    {
        private static readonly Lazy<MySqlProvider> s_instance =
            new Lazy<MySqlProvider>(() => new MySqlProvider());

        public static MySqlProvider Instance
        {
            get { return s_instance.Value; }
        }

        private static readonly MethodInfo s_methodForGetConvertToSqlValueFuncForNullable =
            typeof(MySqlProvider).GetMethod("GetConvertToSqlValueFuncForNullable",
                                            BindingFlags.Instance | BindingFlags.Public);

        public string GetSqlType(Type type, int length = 0)
        {
            var lengthStr = length > 0 ? length.ToString() : "10000"; // TODO: BLOB for MAX
            if (type == typeof(bool))
                return "BIT";
            if (type == typeof(byte))
                return "TINYINT UNSIGNED";
            if (type == typeof(short))
                return "SMALLINT";
            if (type == typeof(char))
                return "INT";
            if (type == typeof(int))
                return "INT";
            if (type == typeof(long))
                return "BIGINT";
            if (type == typeof(float))
                return "FLOAT";
            if (type == typeof(double))
                return "DOUBLE";
            if (type == typeof(decimal))
                return "DECIMAL(18,2)";
            if (type == typeof(DateTime))
                return "DATETIME(6)";
            if (type == typeof(DateTimeOffset))
                return "DATETIME(6)";
            if (type == typeof(TimeSpan))
                return "TIME(6)";
            if (type == typeof(string))
                return $"VARCHAR({lengthStr}) CHARACTER SET utf8";
            if (type == typeof(byte[]))
                return $"VARBINARY({lengthStr})";
            if (type == typeof(Guid))
                return "CHAR(36)";
            if (type.IsEnum)
                return GetSqlType(Enum.GetUnderlyingType(type));
            return "";
        }

        public Func<object, string> GetConvertToSqlValueFunc(Type type)
        {
            if (type == typeof(bool))
                return (o => (bool)o ? "1" : "0");
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
            return "'" + value.ToString("yyyy-MM-ddTHH:mm:ss.fffK") + "'";
        }

        public static string GetSqlValue(DateTimeOffset value)
        {
            // MySQL lacks a type for date with timezone
            return GetSqlValue(value.ToUniversalTime().DateTime);
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
            var stringBuilder = new StringBuilder("0x");

            foreach (var b in value)
                stringBuilder.Append(b.ToString("X2", CultureInfo.InvariantCulture));

            return stringBuilder.ToString();
        }

        public static string GetSqlValue(Guid value)
        {
            return "N'" + value.ToString() + "'";
        }

        public Func<object, object> GetConvertFromDbValueFunc(Type type)
        {
            if (type == typeof(string))
                return (o => o != DBNull.Value ? Convert.ChangeType(o, type) : null);
            if (type == typeof(byte[]))
                return (o => o != DBNull.Value ? Convert.ChangeType(o, type) : null);
            // MySQL doesn't provide DateTime with TimeZone
            if (type == typeof(DateTimeOffset))
                return (o => new DateTimeOffset((DateTime)o, TimeSpan.Zero));
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
                    if (Nullable.GetUnderlyingType(c.Type) != null)
                    {
                        var underlyingType = Nullable.GetUnderlyingType(c.Type);
                        var identity = c.IsIdentity ? " AUTO_INCREMENT" : "";
                        return $"{c.EscapedName} {GetSqlType(underlyingType, c.Length)}{identity}";
                    }
                    else
                    {
                        var identity = c.IsIdentity ? " AUTO_INCREMENT" : "";
                        var notnull = c.Type.IsValueType ? " NOT NULL" : "";
                        return $"{c.EscapedName} {GetSqlType(c.Type, c.Length)}{identity}{notnull}";
                    }
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
