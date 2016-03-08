using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TrackableData.Sql
{
    public class TrackablePocoSqlMapper<T>
        where T : ITrackablePoco<T>
    {
        private readonly ISqlProvider _sqlProvider;
        private readonly Type _trackableType;
        private readonly string _tableName;
        private readonly string _tableEscapedName;
        private readonly ColumnProperty[] _allColumns;
        private readonly ColumnProperty[] _headKeyColumns;
        private readonly ColumnProperty[] _primaryKeyColumns;
        private readonly ColumnProperty[] _valueColumns;
        private readonly Dictionary<PropertyInfo, ColumnProperty> _valueColumnMap;
        private readonly ColumnProperty _identityColumn;
        private readonly string _allColumnStringExceptIdentity;
        private readonly string _allColumnStringExceptHead;

        public TrackablePocoSqlMapper(ISqlProvider sqlProvider,
                                      string tableName, ColumnDefinition[] headKeyColumnDefs = null)
        {
            _sqlProvider = sqlProvider;

            var trackableTypeName = typeof(T).Namespace + "." + ("Trackable" + typeof(T).Name.Substring(1));
            _trackableType = typeof(T).Assembly.GetType(trackableTypeName);
            if (_trackableType == null)
                throw new ArgumentException($"Cannot find type '{trackableTypeName}'");

            _tableName = tableName;
            _tableEscapedName = _sqlProvider.EscapeName(tableName);

            var allColumns = new List<ColumnProperty>();
            var headKeyColumns = new List<ColumnProperty>();
            var primaryKeyColumns = new List<ColumnProperty>();
            var valueColumns = new List<ColumnProperty>();

            // add head key columns

            if (headKeyColumnDefs != null)
            {
                foreach (var headKeyColumnDef in headKeyColumnDefs)
                {
                    var column = new ColumnProperty(
                        name: headKeyColumnDef.Name,
                        escapedName: _sqlProvider.EscapeName(headKeyColumnDef.Name),
                        type: headKeyColumnDef.Type,
                        length: headKeyColumnDef.Length,
                        convertToSqlValue: _sqlProvider.GetSqlValueFunc(headKeyColumnDef.Type));

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

                var column = new ColumnProperty(
                    name: columnName,
                    escapedName: _sqlProvider.EscapeName(columnName),
                    type: property.PropertyType,
                    isIdentity: isIdentity,
                    propertyInfo: property,
                    convertToSqlValue: _sqlProvider.GetSqlValueFunc(property.PropertyType),
                    extractToSqlValue: _sqlProvider.GetExtractToSqlValueFunc(property));

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

            _allColumnStringExceptIdentity = string.Join(
                ",", _allColumns.Where(c => c.IsIdentity == false).Select(c => c.EscapedName));
            _allColumnStringExceptHead = string.Join(
                ",", _valueColumns.Select(c => c.EscapedName));
        }

        private void BuildWhereClauses(StringBuilder sb, params object[] keyValues)
        {
            if (keyValues.Length <= 0)
                return;

            sb.Append(" WHERE ");
            sb.Append(string.Join(
                " AND ",
                keyValues.Zip(_primaryKeyColumns,
                              (v, c) => $"{c.EscapedName}={c.ConvertToSqlValue(v)}")));
        }

        public string BuildCreateTableSql(bool dropIfExists = false)
        {
            return _sqlProvider.BuildCreateTableSql(_tableName,
                                                    _allColumns,
                                                    _primaryKeyColumns,
                                                    dropIfExists);
        }

        public string BuildSqlForCreate(T poco, params object[] keyValues)
        {
            if (keyValues.Length != _headKeyColumns.Length)
                throw new ArgumentException("Head key value required");

            var sb = new StringBuilder();
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

                    sb.Append(column.ExtractToSqlValue(poco));
                }
            }

            return _sqlProvider.BuildInsertIntoSql(_tableName,
                                                   _allColumnStringExceptIdentity,
                                                   sb.ToString(),
                                                   _identityColumn);
        }

        public string BuildSqlForDelete(params object[] keyValues)
        {
            var sb = new StringBuilder();
            sb.Append($"DELETE FROM {_tableEscapedName}");
            BuildWhereClauses(sb, keyValues);
            sb.Append(";\n");
            return sb.ToString();
        }

        public string BuildSqlForLoad(params object[] keyValues)
        {
            var sb = new StringBuilder();
            sb.Append($"SELECT {_allColumnStringExceptHead} FROM {_tableEscapedName}");
            BuildWhereClauses(sb, keyValues);
            sb.Append(";\n");
            return sb.ToString();
        }

        public string BuildSqlForSave(TrackablePocoTracker<T> tracker, params object[] keyValues)
        {
            if (tracker.HasChange == false)
                return string.Empty;

            var sb = new StringBuilder();
            sb.Append($"UPDATE {_tableEscapedName} SET ");

            var concating = false;
            foreach (var c in tracker.ChangeMap)
            {
                ColumnProperty column;
                if (_valueColumnMap.TryGetValue(c.Key, out column) == false)
                    continue;

                if (concating == false)
                    concating = true;
                else
                    sb.Append(",");

                sb.Append(column.EscapedName);
                sb.Append("=");
                sb.Append(column.ConvertToSqlValue(c.Value.NewValue));
            }

            if (concating == false)
                return string.Empty;

            BuildWhereClauses(sb, keyValues);
            sb.Append(";\n");
            return sb.ToString();
        }

        public async Task<int> ResetTableAsync(DbConnection connection)
        {
            var sql = BuildCreateTableSql(true);
            using (var command = _sqlProvider.CreateDbCommand(sql, connection))
            {
                return await command.ExecuteNonQueryAsync();
            }
        }

        public async Task<int> CreateAsync(DbConnection connection, T value, params object[] keyValues)
        {
            var sql = BuildSqlForCreate(value, keyValues);
            using (var command = _sqlProvider.CreateDbCommand(sql, connection))
            {
                if (_identityColumn != null)
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            var identity = reader.GetValue(0);
                            _identityColumn.PropertyInfo.SetValue(
                                value,
                                Convert.ChangeType(identity, _identityColumn.PropertyInfo.PropertyType));
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

        public async Task<int> DeleteAsync(DbConnection connection, params object[] keyValues)
        {
            var sql = BuildSqlForDelete(keyValues);
            using (var command = _sqlProvider.CreateDbCommand(sql, connection))
            {
                return await command.ExecuteNonQueryAsync();
            }
        }

        public async Task<T> LoadAsync(DbConnection connection, params object[] keyValues)
        {
            var sql = BuildSqlForLoad(keyValues);
            using (var command = _sqlProvider.CreateDbCommand(sql, connection))
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

        public async Task<List<T>> LoadAllAsync(DbConnection connection, params object[] keyValues)
        {
            var sql = BuildSqlForLoad(keyValues);
            var list = new List<T>();
            using (var command = _sqlProvider.CreateDbCommand(sql, connection))
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
                    SqlUtility.ConvertValue(record.GetValue(i), _valueColumns[i].Type));
            }
            return value;
        }

        public Task<int> SaveAsync(DbConnection connection, IPocoTracker<T> tracker, params object[] keyValues)
        {
            return SaveAsync(connection, (TrackablePocoTracker<T>)tracker, keyValues);
        }

        public async Task<int> SaveAsync(DbConnection connection, TrackablePocoTracker<T> tracker,
                                         params object[] keyValues)
        {
            if (tracker.HasChange == false)
                return 0;

            var sql = BuildSqlForSave(tracker, keyValues);
            using (var command = _sqlProvider.CreateDbCommand(sql, connection))
            {
                return await command.ExecuteNonQueryAsync();
            }
        }
    }
}
