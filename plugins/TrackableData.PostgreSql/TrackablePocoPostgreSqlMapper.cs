using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace TrackableData.PostgreSql
{
    public class TrackablePocoPostgreSqlMapper<T>
        where T : ITrackablePoco<T>
    {
        private class Column
        {
            public string Name;
            public Type Type;
            public int Length;
            public bool IsIdentity;
            public PropertyInfo PropertyInfo;
            public Func<object, string> ConvertToSqlValue;
        }

        private readonly Type _trackableType;
        private readonly string _tableName;
        private readonly Column[] _allColumns;
        private readonly Column[] _headKeyColumns;
        private readonly Column[] _primaryKeyColumns;
        private readonly Column[] _valueColumns;
        private readonly Dictionary<PropertyInfo, Column> _valueColumnMap;
        private readonly Column _identityColumn;
        private readonly string _allColumnStringExceptIdentity;
        private readonly string _allColumnStringExceptHead;

        public TrackablePocoPostgreSqlMapper(string tableName, ColumnDefinition[] headKeyColumnDefs = null)
        {
            var trackableTypeName = typeof(T).Namespace + "." + ("Trackable" + typeof(T).Name.Substring(1));
            _trackableType = typeof(T).Assembly.GetType(trackableTypeName);

            _tableName = tableName;

            var allColumns = new List<Column>();
            var headKeyColumns = new List<Column>();
            var primaryKeyColumns = new List<Column>();
            var valueColumns = new List<Column>();

            // add head key columns

            if (headKeyColumnDefs != null)
            {
                foreach (var headKeyColumnDef in headKeyColumnDefs)
                {
                    var column = new Column
                    {
                        Name = SqlMapperHelper.GetEscapedName(headKeyColumnDef.Name),
                        Type = headKeyColumnDef.Type,
                        Length = headKeyColumnDef.Length,
                        ConvertToSqlValue = SqlMapperHelper.GetSqlValueFunc(headKeyColumnDef.Type)
                    };
                    headKeyColumns.Add(column);
                    primaryKeyColumns.Add(column);
                    allColumns.Add(column);
                }
            }

            // while scan properties of T, construct primaryKey, value column information.

            var valueType = typeof(T);
            foreach (var property in valueType.GetProperties())
            {
                var columnName = property.Name;
                var primaryKey = false;
                var isIdentity = false;

                var attr = property.GetCustomAttribute<TrackablePropertyAttribute>();
                if (attr != null)
                {
                    if (attr["sql.ignore"] != null)
                        continue;
                    columnName = attr["sql.column:"] ?? columnName;
                    primaryKey = attr["sql.primary-key"] != null;
                    isIdentity = attr["sql.identity"] != null;
                }

                var column = new Column
                {
                    Name = SqlMapperHelper.GetEscapedName(columnName),
                    Type = property.PropertyType,
                    IsIdentity = isIdentity,
                    PropertyInfo = property,
                    ConvertToSqlValue = SqlMapperHelper.GetSqlValueFunc(property.PropertyType)
                };

                if (primaryKey)
                    primaryKeyColumns.Add(column);

                valueColumns.Add(column);
                allColumns.Add(column);

                if (isIdentity)
                    _identityColumn = column;
            }

            _allColumns = allColumns.ToArray();
            _headKeyColumns = headKeyColumns.ToArray();
            _primaryKeyColumns = primaryKeyColumns.ToArray();
            _valueColumns = valueColumns.ToArray();
            _valueColumnMap = _valueColumns.ToDictionary(x => x.PropertyInfo, y => y);
            _allColumnStringExceptIdentity = string.Join(",",
                                                         _allColumns.Where(c => c.IsIdentity == false)
                                                                    .Select(c => c.Name));
            _allColumnStringExceptHead = string.Join(",", _valueColumns.Select(c => c.Name));
        }

        #region PostgreSQL SQL Builder

        private void BuildWhereClauses(StringBuilder sb, params object[] keyValues)
        {
            if (keyValues.Length <= 0)
                return;

            sb.Append(" WHERE ");
            sb.Append(string.Join(
                " AND ",
                keyValues.Zip(_primaryKeyColumns, (v, c) => $"{c.Name} = {c.ConvertToSqlValue(v)}")));
        }

        public string BuildCreateTableSql(bool includeDropIfExists = false)
        {
            var columnDef = string.Join(
                ",\n",
                _allColumns.Select(c =>
                {
                    var identity = c.IsIdentity ? "AUTO_INCREMENT" : "";
                    var notnull = c.Type.IsValueType ? "NOT NULL" : "";
                    return $"{c.Name} {SqlMapperHelper.GetSqlType(c.Type, c.Length)} {identity} {notnull}";
                }));

            var primaryKeyDef = string.Join(
                ",",
                _primaryKeyColumns.Select(c => $"{c.Name}"));

            var sb = new StringBuilder();
            if (includeDropIfExists)
                sb.AppendLine($"DROP TABLE IF EXISTS \"{_tableName}\";");
            sb.AppendLine($"CREATE TABLE \"{_tableName}\" (");
            sb.AppendLine(columnDef);
            sb.AppendLine($", PRIMARY KEY ({primaryKeyDef})");
            sb.AppendLine(");");
            return sb.ToString();
        }

        public string BuildSqlForCreate(T poco, params object[] keyValues)
        {
            if (keyValues.Length != _headKeyColumns.Length)
                throw new ArgumentException("Head key value required");

            var sb = new StringBuilder();
            sb.Append($"INSERT INTO {_tableName} ({_allColumnStringExceptIdentity}) VALUES (");

            var concating = false;
            var keyIndex = 0;
            foreach (var column in _allColumns)
            {
                if (column.IsIdentity)
                    continue;

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

            sb.Append(");\n");
            sb.Append("SELECT LAST_INSERT_ID();");

            return sb.ToString();
        }

        public string BuildSqlForDelete(params object[] keyValues)
        {
            var sb = new StringBuilder();
            sb.Append($"DELETE FROM \"{_tableName}\"");
            BuildWhereClauses(sb, keyValues);
            sb.Append(";\n");
            return sb.ToString();
        }

        public string BuildSqlForLoad(params object[] keyValues)
        {
            var sb = new StringBuilder();
            sb.Append($"SELECT {_allColumnStringExceptHead} FROM \"{_tableName}\"");
            BuildWhereClauses(sb, keyValues);
            sb.Append(";\n");
            return sb.ToString();
        }

        public string BuildSqlForSave(TrackablePocoTracker<T> tracker, params object[] keyValues)
        {
            if (tracker.HasChange == false)
                return string.Empty;

            var sb = new StringBuilder();
            sb.Append($"UPDATE \"{_tableName}\" SET ");

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
            sb.Append(";\n");
            return sb.ToString();
        }

        #endregion

        #region Helpers

        public async Task<int> ResetTableAsync(NpgsqlConnection connection)
        {
            var sql = BuildCreateTableSql(true);
            using (var command = new NpgsqlCommand(sql, connection))
            {
                return await command.ExecuteNonQueryAsync();
            }
        }

        public async Task<int> CreateAsync(NpgsqlConnection connection, T value, params object[] keyValues)
        {
            var sql = BuildSqlForCreate(value, keyValues);
            using (var command = new NpgsqlCommand(sql, connection))
            {
                if (_identityColumn != null)
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            var id = reader.GetValue(0);
                            _identityColumn.PropertyInfo.SetValue(
                                value,
                                Convert.ChangeType(id, _identityColumn.PropertyInfo.PropertyType));
                            return 1;
                        }
                    }
                    return 0;
                }
                else
                {
                    return await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<int> DeleteAsync(NpgsqlConnection connection, params object[] keyValues)
        {
            var sql = BuildSqlForDelete(keyValues);
            using (var command = new NpgsqlCommand(sql, connection))
            {
                return await command.ExecuteNonQueryAsync();
            }
        }

        public async Task<T> LoadAsync(NpgsqlConnection connection, params object[] keyValues)
        {
            var sql = BuildSqlForLoad(keyValues);
            using (var command = new NpgsqlCommand(sql, connection))
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

        public async Task<List<T>> LoadAllAsync(NpgsqlConnection connection, params object[] keyValues)
        {
            var sql = BuildSqlForLoad(keyValues);
            var list = new List<T>();
            using (var command = new NpgsqlCommand(sql, connection))
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

        public Task<int> SaveAsync(NpgsqlConnection connection, IPocoTracker<T> tracker, params object[] keyValues)
        {
            return SaveAsync(connection, (TrackablePocoTracker<T>)tracker, keyValues);
        }

        public async Task<int> SaveAsync(NpgsqlConnection connection, TrackablePocoTracker<T> tracker,
                                         params object[] keyValues)
        {
            if (tracker.HasChange == false)
                return 0;

            var sql = BuildSqlForSave(tracker, keyValues);
            using (var command = new NpgsqlCommand(sql, connection))
            {
                return await command.ExecuteNonQueryAsync();
            }
        }

        #endregion
    }
}
