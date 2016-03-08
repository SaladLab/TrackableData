using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using TrackableData.Sql;

namespace TrackableData.MsSql
{
    public class MsSqlProvider : ISqlProvider
    {
        private static readonly Lazy<MsSqlProvider> _instance = new Lazy<MsSqlProvider>(() => new MsSqlProvider());

        public static MsSqlProvider Instance
        {
            get { return _instance.Value; }
        }

        public string GetSqlType(Type type, int length = 0)
        {
            var lengthStr = length > 0 ? length.ToString() : "MAX";
            if (type == typeof(bool))
                return "[bit]";
            if (type == typeof(byte))
                return "[tinyint]";
            if (type == typeof(int))
                return "[int]";
            if (type == typeof(long))
                return "[bigint]";
            if (type == typeof(short))
                return "[smallint]";
            if (type == typeof(float))
                return "[real]";
            if (type == typeof(double))
                return "[float]";
            if (type == typeof(DateTime))
                return "[datetime2]";
            if (type == typeof(DateTimeOffset))
                return "[datetimeoffset]";
            if (type == typeof(string))
                return $"[nvarchar]({lengthStr})";
            if (type == typeof(byte[]))
                return $"[varbinary]({lengthStr})";
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
                    var identity = c.IsIdentity ? " IDENTITY(1,1)" : "";
                    var notnull = c.Type.IsValueType ? " NOT NULL" : "";
                    return $"{c.EscapedName} {GetSqlType(c.Type, c.Length)}{identity}{notnull}";
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
