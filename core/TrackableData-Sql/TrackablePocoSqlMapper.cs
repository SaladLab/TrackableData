using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TrackableData
{
    public class TrackablePocoSqlMapper<T>
        where T : ITrackablePoco
    {
        private struct Column
        {
            public string Name;
            public Type Type;
            public PropertyInfo PropertyInfo;
            public Func<object, string> ConvertToSqlValue;
        }

        private readonly Type _trackableType;
        private readonly string _tableName;
        private readonly Column[] _allColumns;
        private readonly Column[] _primaryKeyColumns;
        private readonly Column[] _valueColumns;
        private readonly Dictionary<PropertyInfo, Column> _valueColumnMap;
        private readonly int _headKeyCount;
        private readonly string _allColumnString;
        private readonly string _allColumnStringExceptHead;

        public TrackablePocoSqlMapper(string tableName)
            : this(tableName, null, null)
        {
        }

        public TrackablePocoSqlMapper(string tableName, string headKeyColumnName, Type headKeyType)
        {
            var trackableTypeName = typeof (T).Namespace + "." + ("Trackable" + typeof (T).Name.Substring(1));
            _trackableType = typeof (T).Assembly.GetType(trackableTypeName);

            var allColumns = new List<Column>();
            var primaryKeyColumns = new List<Column>();
            var valueColumns = new List<Column>();

            _tableName = tableName;

            // add head key to primary keys

            if (string.IsNullOrEmpty(headKeyColumnName) == false)
            {
                var column = new Column
                {
                    Name = SqlMapperHelper.GetEscapedName(headKeyColumnName),
                    Type = headKeyType,
                    ConvertToSqlValue = SqlMapperHelper.GetSqlValueFunc(headKeyType)
                };
                primaryKeyColumns.Add(column);
                allColumns.Add(column);
                _headKeyCount = 1;
            }

            // while scan properties of T, construct primaryKey, value column information.

            var valueType = typeof(T);
            foreach (var property in valueType.GetProperties())
            {
                var columnName = property.Name;
                var primaryKey = false;

                var attr = property.GetCustomAttribute<TrackableFieldAttribute>();
                if (attr != null)
                {
                    if (attr["sql.ignore"] != null)
                        continue;
                    columnName = attr["sql.column:"] ?? columnName;
                    primaryKey = attr["sql.primary-key"] != null;
                }

                var column = new Column
                {
                    Name = SqlMapperHelper.GetEscapedName(columnName),
                    Type = property.PropertyType,
                    PropertyInfo = property,
                    ConvertToSqlValue = SqlMapperHelper.GetSqlValueFunc(property.PropertyType)
                };

                if (primaryKey)
                    primaryKeyColumns.Add(column);

                valueColumns.Add(column);
                allColumns.Add(column);
            }

            _allColumns = allColumns.ToArray();
            _primaryKeyColumns = primaryKeyColumns.ToArray();
            _valueColumns = valueColumns.ToArray();
            _valueColumnMap = _valueColumns.ToDictionary(x => x.PropertyInfo, y => y);
            _allColumnString = string.Join(",", _allColumns);
            _allColumnStringExceptHead = string.Join(",", _valueColumns.Select(c => c.Name));
        }

        // SQL Friendly Methods

        private void BuildWhereClauses(StringBuilder sb, params object[] keyValues)
        {
            if (keyValues.Length <= 0)
                return;

            sb.Append(" WHERE ");
            sb.Append(string.Join(
                " AND ",
                keyValues.Zip(_primaryKeyColumns, (v, c) => $"{c.Name} = {c.ConvertToSqlValue(v)}")));
        }

        public string GenerateCreateTableSql()
        {
            var columnDef = string.Join(
                ",\n",
                _allColumns.Select(c =>
                    $"{c.Name} {SqlMapperHelper.GetSqlType(c.Type)} NOT NULL"));

            var primaryKeyDef = string.Join(
                ",",
                _primaryKeyColumns.Select(c =>
                    $"{c.Name} ASC"));

            var sb = new StringBuilder();
            sb.AppendLine($"CREATE TABLE [dbo].[{_tableName}] (");
            sb.AppendLine(columnDef);
            sb.AppendLine($"  CONSTRAINT[PK_{_tableName}] PRIMARY KEY CLUSTERED({primaryKeyDef}) WITH (");
            sb.AppendLine("  PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON");
            sb.AppendLine("  ) ON[PRIMARY]");
            sb.AppendLine(") ON[PRIMARY]");
            return sb.ToString();
        }

        public string GenerateInsertSql(T poco, params object[] keyValues)
        {
            if (keyValues.Length != _headKeyCount)
                throw new ArgumentException("Head key value required");

            var sb = new StringBuilder();
            sb.Append($"INSERT INTO {_tableName} ({_allColumnString}) VALUES (");

            var concating = false;
            var keyIndex = 0;
            foreach (var column in _allColumns)
            {
                if (concating == false)
                    concating = true;
                else
                    sb.Append(",");

                if (column.PropertyInfo == null)
                {
                    sb.Append(column.ConvertToSqlValue(keyValues[keyIndex]));
                    keyIndex += 1;
                }
                else
                {
                    var value = column.PropertyInfo.GetValue(poco);
                    sb.Append(column.ConvertToSqlValue(value));
                }
            }

            sb.Append(")");
            return sb.ToString();
        }

        public string GenerateDeleteSql(params object[] keyValues)
        {
            var sb = new StringBuilder();
            sb.Append($"DELETE FROM {_tableName}");
            BuildWhereClauses(sb, keyValues);
            return sb.ToString();
        }

        public string GenerateSelectSql(params object[] keyValues)
        {
            var sb = new StringBuilder();
            sb.Append($"SELECT {_allColumnStringExceptHead} FROM {_tableName}");
            BuildWhereClauses(sb, keyValues);
            return sb.ToString();
        }

        public string GenerateUpdateSql(TrackablePocoTracker<T> tracker, params object[] keyValues)
        {
            if (tracker.HasChange == false)
                return string.Empty;

            var sb = new StringBuilder();
            sb.Append($"UPDATE {_tableName} SET ");

            var concating = false;
            foreach (var c in tracker.ChangeMap)
            {
                Column column;
                if (_valueColumnMap.TryGetValue(c.Key, out column) == false)
                    continue;

                if (concating == false)
                    concating = true;
                else
                    sb.Append(",");

                sb.Append(column.Name);
                sb.Append("=");
                sb.Append(column.ConvertToSqlValue(c.Value.NewValue));
            }

            if (concating == false)
                return string.Empty;

            BuildWhereClauses(sb, keyValues);
            return sb.ToString();
        }

        // POCO Friendly Methods

        public async Task<int> CreateAsync(SqlConnection connection, T value, params object[] keyValues)
        {
            var sql = GenerateInsertSql(value, keyValues);
            using (var command = new SqlCommand(sql, connection))
            {
                return await command.ExecuteNonQueryAsync();
            }
        }

        public async Task<int> RemoveAsync(SqlConnection connection, T value, params object[] keyValues)
        {
            var sql = GenerateDeleteSql(keyValues);
            using (var command = new SqlCommand(sql, connection))
            {
                return await command.ExecuteNonQueryAsync();
            }
        }

        public async Task<T> LoadAsync(SqlConnection connection, params object[] keys)
        {
            var sql = GenerateSelectSql(keys);
            using (var command = new SqlCommand(sql, connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (await reader.ReadAsync())
                    {
                        return ConvertToPoco(reader);
                    }
                }
            }
            return default(T);
        }

        public async Task<List<T>> LoadAllAsync(SqlConnection connection, params object[] keys)
        {
            var sql = GenerateSelectSql(keys);
            var list = new List<T>();
            using (var command = new SqlCommand(sql, connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (await reader.ReadAsync())
                    {
                        list.Add(ConvertToPoco(reader));
                    }
                }
            }
            return list;
        }

        private T ConvertToPoco(IDataRecord record)
        {
            var value = (T)Activator.CreateInstance(_trackableType);
            for (var i = 0; i < _valueColumns.Length; i++)
            {
                _valueColumns[i].PropertyInfo.SetValue(
                    value,
                    SqlMapperHelper.GetNetValue(record.GetValue(i), _valueColumns[i].Type));
            }
            return value;
        }

        public async Task<int> SaveAsync(SqlConnection connection, TrackablePocoTracker<T> tracker, params object[] keyValues)
        {
            if (tracker.HasChange == false)
                return 0;

            var sql = GenerateUpdateSql(tracker, keyValues);
            using (var command = new SqlCommand(sql, connection))
            {
                return await command.ExecuteNonQueryAsync();
            }
        }
    }
}
