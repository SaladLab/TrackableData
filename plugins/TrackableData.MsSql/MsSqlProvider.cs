using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using TrackableData.Sql;

namespace TrackableData.MsSql
{
    public class MsSqlProvider : ISqlProvider
    {
        private static readonly Lazy<MsSqlProvider> s_instance =
            new Lazy<MsSqlProvider>(() => new MsSqlProvider());

        public static MsSqlProvider Instance
        {
            get { return s_instance.Value; }
        }

        private static readonly MethodInfo s_methodForGetConvertToSqlValueFuncForNullable =
            typeof(MsSqlProvider).GetMethod("GetConvertToSqlValueFuncForNullable",
                                            BindingFlags.Instance | BindingFlags.Public);

        public string GetSqlType(Type type, int length = 0)
        {
            if (type == typeof(bool))
                return "BIT";
            if (type == typeof(byte))
                return "TINYINT";
            if (type == typeof(short))
                return "SMALLINT";
            if (type == typeof(char))
                return "INT";
            if (type == typeof(int))
                return "INT";
            if (type == typeof(long))
                return "BIGINT";
            if (type == typeof(float))
                return "REAL";
            if (type == typeof(double))
                return "FLOAT";
            if (type == typeof(decimal))
                return "DECIMAL(18,2)";
            if (type == typeof(DateTime))
                return "DATETIME2";
            if (type == typeof(DateTimeOffset))
                return "DATETIMEOFFSET";
            if (type == typeof(TimeSpan))
                return "TIME";
            if (type == typeof(string))
                return $"NVARCHAR({GetSqlLength(length)})";
            if (type == typeof(byte[]))
                return $"VARBINARY({GetSqlLength(length)})";
            if (type == typeof(Guid))
                return "UNIQUEIDENTIFIER";
            if (type.IsEnum)
                return GetSqlType(Enum.GetUnderlyingType(type));
            return "";
        }

        private static string GetSqlLength(int length)
        {
            return length > 0 ? length.ToString() : "MAX";
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
            return "'" + value.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz") + "'";
        }

        public static string GetSqlValue(TimeSpan value)
        {
            return "'" + value.ToString() + "'";
        }

        public static string GetSqlValue(string value)
        {
            return "N'" + value.Replace("'", "''") + "'";
        }

        public static string GetSqlValue(byte[] value)
        {
            var sb = new StringBuilder((value.Length * 2) + 2);

            sb.Append("0x");
            foreach (var b in value)
                sb.Append(b.ToString("X2", CultureInfo.InvariantCulture));

            return sb.ToString();
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
            return "[" + name + "]";
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
                        var identity = c.IsIdentity ? " IDENTITY(1,1)" : "";
                        return $"{c.EscapedName} {GetSqlType(underlyingType, c.Length)}{identity}";
                    }
                    else
                    {
                        var identity = c.IsIdentity ? " IDENTITY(1,1)" : "";
                        var notnull = c.Type.IsValueType ? " NOT NULL" : "";
                        return $"{c.EscapedName} {GetSqlType(c.Type, c.Length)}{identity}{notnull}";
                    }
                }));

            var primaryKeyDef = string.Join(
                ",",
                primaryKeys.Select(c => $"{c.EscapedName} ASC"));

            var sb = new StringBuilder();
            var tableEscapedName = EscapeName(tableName);
            if (dropIfExists)
            {
                sb.AppendLine($"IF OBJECT_ID('dbo.{tableEscapedName}', 'U') IS NOT NULL");
                sb.AppendLine($"  DROP TABLE dbo.{tableEscapedName}");
            }
            sb.AppendLine($"CREATE TABLE [dbo].{tableEscapedName} (");
            sb.AppendLine(columnDef);
            sb.AppendLine($"  CONSTRAINT[PK_{tableName}] PRIMARY KEY CLUSTERED({primaryKeyDef}) WITH (");
            sb.AppendLine("  PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF,");
            sb.AppendLine("  IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON");
            sb.AppendLine("  ) ON[PRIMARY]");
            sb.AppendLine(") ON[PRIMARY]");
            return sb.ToString();
        }

        public string BuildInsertIntoSql(string tableName,
                                         string columns,
                                         string values,
                                         ColumnProperty identity)
        {
            var outputClause = identity != null ? "OUTPUT INSERTED." + identity.EscapedName : "";
            return $"INSERT INTO {EscapeName(tableName)} ({columns}) {outputClause} VALUES ({values});";
        }

        public DbCommand CreateDbCommand(string sql, DbConnection connection)
        {
            var sqlConnection = (SqlConnection)connection;
            if (sqlConnection == null)
                throw new ArgumentNullException(nameof(connection));

            return new SqlCommand(sql, sqlConnection);
        }
    }
}
