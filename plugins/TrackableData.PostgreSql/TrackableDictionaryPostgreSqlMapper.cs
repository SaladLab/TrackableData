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
    public class TrackableDictionaryPostgreSqlMapper<TKey, TValue>
    {
        private class Column
        {
            public string Name;
            public string EscapedName;
            public Type Type;
            public int Length;
            public PropertyInfo PropertyInfo;
            public Func<object, string> ConvertToSqlValue;
            public Func<object, string> ExtractToSqlValue;
        }

        private readonly string _tableName;
        private readonly string _tableEscapedName;
        private readonly Column[] _allColumns;
        private readonly Column[] _headKeyColumns;
        private readonly Column[] _primaryKeyColumns;
        private readonly Column _keyColumn;
        private readonly Column[] _valueColumns;
        private readonly Dictionary<PropertyInfo, Column> _valueColumnMap;
        private readonly string _allColumnString;
        private readonly string _allColumnStringExceptHead;

        private bool IsSingleValueType => _valueColumns.Length == 1 && _valueColumns[0].PropertyInfo == null;

        public TrackableDictionaryPostgreSqlMapper(string tableName,
                                                   ColumnDefinition keyColumnDef,
                                                   ColumnDefinition[] headKeyColumnDefs = null)
            : this(tableName, keyColumnDef, null, headKeyColumnDefs)
        {
        }

        public TrackableDictionaryPostgreSqlMapper(string tableName,
                                                   ColumnDefinition keyColumnDef,
                                                   ColumnDefinition singleValueColumnDef,
                                                   ColumnDefinition[] headKeyColumnDefs)
        {
            _tableName = tableName;
            _tableEscapedName = SqlMapperHelper.GetEscapedName(tableName);

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
                        Name = headKeyColumnDef.Name,
                        EscapedName = SqlMapperHelper.GetEscapedName(headKeyColumnDef.Name),
                        Type = headKeyColumnDef.Type,
                        Length = headKeyColumnDef.Length,
                        ConvertToSqlValue = SqlMapperHelper.GetSqlValueFunc(headKeyColumnDef.Type)
                    };
                    headKeyColumns.Add(column);
                    primaryKeyColumns.Add(column);
                    allColumns.Add(column);
                }
            }

            // add key column

            _keyColumn = new Column
            {
                Name = keyColumnDef.Name,
                EscapedName = SqlMapperHelper.GetEscapedName(keyColumnDef.Name),
                Type = typeof(TKey),
                Length = keyColumnDef.Length,
                ConvertToSqlValue = SqlMapperHelper.GetSqlValueFunc(typeof(TKey))
            };
            primaryKeyColumns.Add(_keyColumn);
            allColumns.Add(_keyColumn);

            // while scan properties of T, construct value column information.

            if (singleValueColumnDef != null)
            {
                var column = new Column
                {
                    Name = singleValueColumnDef.Name,
                    EscapedName = SqlMapperHelper.GetEscapedName(singleValueColumnDef.Name),
                    Type = typeof(TValue),
                    Length = singleValueColumnDef.Length,
                    ConvertToSqlValue = SqlMapperHelper.GetSqlValueFunc(typeof(TValue)),
                    ExtractToSqlValue = SqlMapperHelper.GetSqlValueFunc(typeof(TValue))
                };
                valueColumns.Add(column);
                allColumns.Add(column);
            }
            else
            {
                var valueType = typeof(TValue);
                foreach (var property in valueType.GetProperties())
                {
                    var columnName = property.Name;

                    var attr = property.GetCustomAttribute<TrackablePropertyAttribute>();
                    if (attr != null)
                    {
                        if (attr["sql.ignore"] != null)
                            continue;
                        columnName = attr["sql.column:"] ?? columnName;
                    }

                    var column = new Column
                    {
                        Name = columnName,
                        EscapedName = SqlMapperHelper.GetEscapedName(columnName),
                        Type = property.PropertyType,
                        PropertyInfo = property,
                        ConvertToSqlValue = SqlMapperHelper.GetSqlValueFunc(property.PropertyType),
                        ExtractToSqlValue = SqlMapperHelper.GetSqlValueFunc(property)
                    };

                    valueColumns.Add(column);
                    allColumns.Add(column);
                }
            }

            _allColumns = allColumns.ToArray();
            _headKeyColumns = headKeyColumns.ToArray();
            _primaryKeyColumns = primaryKeyColumns.ToArray();
            _valueColumns = valueColumns.ToArray();
            _valueColumnMap = _valueColumns.Where(x => x.PropertyInfo != null).ToDictionary(x => x.PropertyInfo, y => y);
            _allColumnString = string.Join(",", _allColumns.Select(c => c.EscapedName));
            _allColumnStringExceptHead = _keyColumn.EscapedName + "," +
                                         string.Join(",", _valueColumns.Select(c => c.EscapedName));
        }

        #region PostgreSQL SQL Builder

        private void BuildWhereClauses(StringBuilder sb, params object[] keyValues)
        {
            if (keyValues.Length <= 0)
                return;

            sb.Append(" WHERE ");
            sb.Append(string.Join(
                " AND ",
                keyValues.Zip(_primaryKeyColumns, (v, c) => $"{c.EscapedName} = {c.ConvertToSqlValue(v)}")));
        }

        public string BuildCreateTableSql(bool includeDropIfExists = false)
        {
            var columnDef = string.Join(
                ",\n",
                _allColumns.Select(c =>
                {
                    var notnull = c.Type.IsValueType ? "NOT NULL" : "";
                    return $"{c.EscapedName} {SqlMapperHelper.GetSqlType(c.Type, c.Length)} {notnull}";
                }));

            var primaryKeyDef = string.Join(
                ",",
                _headKeyColumns.Concat(new[] { _keyColumn }).Select(c => $"{c.EscapedName}"));

            var sb = new StringBuilder();
            if (includeDropIfExists)
                sb.AppendLine($"DROP TABLE IF EXISTS {_tableEscapedName};");
            sb.AppendLine($"CREATE TABLE {_tableEscapedName} (");
            sb.AppendLine(columnDef);
            sb.AppendLine($", PRIMARY KEY ({primaryKeyDef})");
            sb.AppendLine(");");
            return sb.ToString();
        }

        public string BuildSqlForCreate(IDictionary<TKey, TValue> dictionary, params object[] keyValues)
        {
            if (keyValues.Length != _headKeyColumns.Length)
                throw new ArgumentException("Number of keyValues should be same with the number of head columns");

            var sql = new StringBuilder();

            // generate sql command for each rows

            var insertCount = 0;
            foreach (var i in dictionary)
            {
                if (insertCount == 0)
                {
                    sql.Append("INSERT INTO ").Append(_tableEscapedName);
                    sql.Append(" (").Append(_allColumnString).Append(") VALUES\n");
                }
                else
                {
                    sql.Append(",\n");
                }

                sql.Append(" (");
                for (var k = 0; k < _headKeyColumns.Length; k++)
                {
                    sql.Append(_headKeyColumns[k].ConvertToSqlValue(keyValues[k]));
                    sql.Append(",");
                }
                sql.Append(_keyColumn.ConvertToSqlValue(i.Key));

                foreach (var col in _valueColumns)
                    sql.Append(",").Append(col.ExtractToSqlValue(i.Value));

                sql.Append(")");

                insertCount += 1;
                if (insertCount >= 1000)
                {
                    sql.Append(";\n");
                    insertCount = 0;
                }
            }
            if (insertCount > 0)
                sql.Append(";\n");

            return sql.ToString();
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

        public string BuildSqlForSave(TrackableDictionaryTracker<TKey, TValue> tracker,
                                      params object[] keyValues)
        {
            if (keyValues.Length != _headKeyColumns.Length)
                throw new ArgumentException("Number of keyValues should be same with the number of head columns");

            var sqlAdd = new StringBuilder();
            var sqlModify = new StringBuilder();
            var removeIds = new List<TKey>();

            // generate sql command for each changes

            var insertCount = 0;
            foreach (var i in tracker.ChangeMap)
            {
                var v = i.Value.NewValue;
                switch (i.Value.Operation)
                {
                    case TrackableDictionaryOperation.Add:
                        if (insertCount == 0)
                        {
                            sqlAdd.Append("INSERT INTO ").Append(_tableEscapedName);
                            sqlAdd.Append(" (").Append(_allColumnString).Append(") VALUES\n");
                        }
                        else
                        {
                            sqlAdd.Append(",\n");
                        }

                        sqlAdd.Append(" (");
                        for (var k = 0; k < _headKeyColumns.Length; k++)
                        {
                            sqlAdd.Append(_headKeyColumns[k].ConvertToSqlValue(keyValues[k]));
                            sqlAdd.Append(",");
                        }
                        sqlAdd.Append(_keyColumn.ConvertToSqlValue(i.Key));

                        foreach (var col in _valueColumns)
                            sqlAdd.Append(",").Append(col.ExtractToSqlValue(v));

                        sqlAdd.Append(")");

                        insertCount += 1;
                        if (insertCount >= 1000)
                        {
                            sqlAdd.Append(";\n");
                            insertCount = 0;
                        }
                        break;

                    case TrackableDictionaryOperation.Modify:
                        sqlModify.Append("UPDATE ").Append(_tableEscapedName);
                        sqlModify.Append(" SET ");
                        var concating = false;
                        foreach (var col in _valueColumns)
                        {
                            if (concating == false)
                                concating = true;
                            else
                                sqlModify.Append(",");
                            sqlModify.Append(col.EscapedName).Append("=").Append(col.ExtractToSqlValue(v));
                        }

                        sqlModify.Append(" WHERE ");
                        for (var k = 0; k < _headKeyColumns.Length; k++)
                        {
                            sqlModify.Append(_headKeyColumns[k].EscapedName).Append("=");
                            sqlModify.Append(_headKeyColumns[k].ConvertToSqlValue(keyValues[k]));
                            sqlModify.Append(" AND ");
                        }

                        sqlModify.Append(_keyColumn.EscapedName).Append("=");
                        sqlModify.Append(_keyColumn.ConvertToSqlValue(i.Key)).Append(";\n");
                        break;

                    case TrackableDictionaryOperation.Remove:
                        removeIds.Add(i.Key);
                        break;
                }
            }
            if (insertCount > 0)
                sqlAdd.Append(";\n");

            // merge insert, update and delete sql into one sql

            var sql = new StringBuilder();
            sql.Append(sqlAdd);
            sql.Append(sqlModify);
            if (removeIds.Any())
            {
                sql.Append("DELETE FROM ").Append(_tableEscapedName).Append(" WHERE ");
                for (var k = 0; k < _headKeyColumns.Length; k++)
                {
                    sqlModify.Append(_headKeyColumns[k].EscapedName).Append("=");
                    sqlModify.Append(_headKeyColumns[k].ConvertToSqlValue(keyValues[k]));
                    sqlModify.Append(" AND ");
                }
                sql.Append(_keyColumn.EscapedName).Append(" IN (");
                var concating = false;
                foreach (var id in removeIds)
                {
                    if (concating == false)
                        concating = true;
                    else
                        sql.Append(",");
                    sql.Append(_keyColumn.ConvertToSqlValue(id));
                }
                sql.Append(");\n");
            }

            return sql.ToString();
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

        public async Task<int> CreateAsync(NpgsqlConnection connection, IDictionary<TKey, TValue> dictionary,
                                           params object[] keyValues)
        {
            var sql = BuildSqlForCreate(dictionary, keyValues);
            using (var command = new NpgsqlCommand(sql, connection))
            {
                return await command.ExecuteNonQueryAsync();
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

        public async Task<TrackableDictionary<TKey, TValue>> LoadAsync(NpgsqlConnection connection,
                                                                       params object[] keyValues)
        {
            var sql = BuildSqlForLoad(keyValues);
            var dictionary = new TrackableDictionary<TKey, TValue>();
            using (var command = new NpgsqlCommand(sql, connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (await reader.ReadAsync())
                    {
                        dictionary.Add(ConvertToKeyAndValue(reader));
                    }
                }
            }
            return dictionary;
        }

        private KeyValuePair<TKey, TValue> ConvertToKeyAndValue(IDataRecord record)
        {
            var key = (TKey)SqlMapperHelper.GetNetValue(record.GetValue(0), typeof(TKey));
            TValue value;

            if (IsSingleValueType)
            {
                value = (TValue)SqlMapperHelper.GetNetValue(record.GetValue(1), typeof(TValue));
            }
            else
            {
                value = (TValue)Activator.CreateInstance(typeof(TValue));
                for (var i = 0; i < _valueColumns.Length; i++)
                {
                    _valueColumns[i].PropertyInfo.SetValue(
                        value,
                        SqlMapperHelper.GetNetValue(record.GetValue(i + 1), _valueColumns[i].Type));
                }
            }

            return new KeyValuePair<TKey, TValue>(key, value);
        }

        public Task<int> SaveAsync(NpgsqlConnection connection, IDictionaryTracker<TKey, TValue> tracker,
                                   params object[] keyValues)
        {
            return SaveAsync(connection, (TrackableDictionaryTracker<TKey, TValue>)tracker, keyValues);
        }

        public async Task<int> SaveAsync(NpgsqlConnection connection, TrackableDictionaryTracker<TKey, TValue> tracker,
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
