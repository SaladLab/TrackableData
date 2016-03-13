﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TrackableData.Sql
{
    public class TrackableDictionarySqlMapper<TKey, TValue>
    {
        private readonly ISqlProvider _sqlProvider;
        private readonly string _tableName;
        private readonly string _tableEscapedName;
        private readonly ColumnProperty[] _allColumns;
        private readonly ColumnProperty[] _headKeyColumns;
        private readonly ColumnProperty[] _primaryKeyColumns;
        private readonly ColumnProperty _keyColumn;
        private readonly ColumnProperty[] _valueColumns;
        private readonly Dictionary<PropertyInfo, ColumnProperty> _valueColumnMap;
        private readonly string _allColumnString;
        private readonly string _allColumnStringExceptHead;

        private bool IsSingleValueType => _valueColumns.Length == 1 && _valueColumns[0].PropertyInfo == null;

        public TrackableDictionarySqlMapper(ISqlProvider sqlProvider,
                                            string tableName,
                                            ColumnDefinition keyColumnDef,
                                            ColumnDefinition[] headKeyColumnDefs = null)
            : this(sqlProvider, tableName, keyColumnDef, null, headKeyColumnDefs)
        {
        }

        public TrackableDictionarySqlMapper(ISqlProvider sqlProvider,
                                            string tableName,
                                            ColumnDefinition keyColumnDef,
                                            ColumnDefinition singleValueColumnDef,
                                            ColumnDefinition[] headKeyColumnDefs)
        {
            _sqlProvider = sqlProvider;

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
                        convertToSqlValue: _sqlProvider.GetConvertToSqlValueFunc(headKeyColumnDef.Type));

                    headKeyColumns.Add(column);
                    primaryKeyColumns.Add(column);
                    allColumns.Add(column);
                }
            }

            // add key column

            _keyColumn = new ColumnProperty(
                name: keyColumnDef.Name,
                escapedName: _sqlProvider.EscapeName(keyColumnDef.Name),
                type: typeof(TKey),
                length: keyColumnDef.Length,
                convertToSqlValue: _sqlProvider.GetConvertToSqlValueFunc(typeof(TKey)),
                convertFromDbValue: _sqlProvider.GetConvertFromDbValueFunc(typeof(TKey)));

            primaryKeyColumns.Add(_keyColumn);
            allColumns.Add(_keyColumn);

            // while scan properties of T, construct value column information.

            if (singleValueColumnDef != null)
            {
                var column = new ColumnProperty(
                    name: singleValueColumnDef.Name,
                    escapedName: _sqlProvider.EscapeName(singleValueColumnDef.Name),
                    type: typeof(TValue),
                    length: singleValueColumnDef.Length,
                    convertToSqlValue: _sqlProvider.GetConvertToSqlValueFunc(typeof(TValue)),
                    convertFromDbValue: _sqlProvider.GetConvertFromDbValueFunc(typeof(TValue)),
                    extractToSqlValue: _sqlProvider.GetConvertToSqlValueFunc(typeof(TValue)));

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

                    var column = new ColumnProperty(
                        name: columnName,
                        escapedName: _sqlProvider.EscapeName(columnName),
                        type: property.PropertyType,
                        propertyInfo: property,
                        convertToSqlValue: _sqlProvider.GetConvertToSqlValueFunc(property.PropertyType),
                        convertFromDbValue: _sqlProvider.GetConvertFromDbValueFunc(property.PropertyType),
                        extractToSqlValue: _sqlProvider.GetExtractToSqlValueFunc(property),
                        installFromDbValue: _sqlProvider.GetInstallFromDbValueFunc(property));

                    valueColumns.Add(column);
                    allColumns.Add(column);
                }
            }

            _allColumns = allColumns.ToArray();
            _headKeyColumns = headKeyColumns.ToArray();
            _primaryKeyColumns = primaryKeyColumns.ToArray();
            _valueColumns = valueColumns.ToArray();
            _valueColumnMap = _valueColumns.Where(x => x.PropertyInfo != null)
                                           .ToDictionary(x => x.PropertyInfo, y => y);

            _allColumnString = string.Join(",", _allColumns.Select(c => c.EscapedName));
            _allColumnStringExceptHead = _keyColumn.EscapedName + "," +
                                         string.Join(",", _valueColumns.Select(c => c.EscapedName));
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
                                                    _headKeyColumns.Concat(new[] { _keyColumn }).ToArray(),
                                                    dropIfExists);
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
                    sql.Append(_headKeyColumns[k].EscapedName).Append("=");
                    sql.Append(_headKeyColumns[k].ConvertToSqlValue(keyValues[k]));
                    sql.Append(" AND ");
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

        public async Task<int> ResetTableAsync(DbConnection connection)
        {
            var sql = BuildCreateTableSql(true);
            using (var command = _sqlProvider.CreateDbCommand(sql, connection))
            {
                return await command.ExecuteNonQueryAsync();
            }
        }

        public async Task<int> CreateAsync(DbConnection connection, IDictionary<TKey, TValue> dictionary,
                                           params object[] keyValues)
        {
            var sql = BuildSqlForCreate(dictionary, keyValues);
            using (var command = _sqlProvider.CreateDbCommand(sql, connection))
            {
                return await command.ExecuteNonQueryAsync();
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

        public Task<TrackableDictionary<TKey, TValue>> LoadAsync(DbConnection connection,
                                                                 params object[] keyValues)
        {
            var sql = BuildSqlForLoad(keyValues);
            using (var command = _sqlProvider.CreateDbCommand(sql, connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    return LoadAsync(reader);
                }
            }
        }

        public async Task<TrackableDictionary<TKey, TValue>> LoadAsync(DbDataReader reader)
        {
            var dictionary = new TrackableDictionary<TKey, TValue>();
            while (await reader.ReadAsync())
            {
                dictionary.Add(ConvertToKeyAndValue(reader));
            }
            return dictionary;
        }

        private KeyValuePair<TKey, TValue> ConvertToKeyAndValue(IDataRecord record)
        {
            var key = (TKey)_keyColumn.ConvertFromDbValue(record.GetValue(0));
            TValue value;

            if (IsSingleValueType)
            {
                var columnValue = record.GetValue(1);
                value = (TValue)_valueColumns[0].ConvertFromDbValue(columnValue);
            }
            else
            {
                value = (TValue)Activator.CreateInstance(typeof(TValue));
                for (var i = 0; i < _valueColumns.Length; i++)
                {
                    var columnValue = record.GetValue(i + 1);
                    _valueColumns[i].InstallFromDbValue(value, columnValue);
                }
            }

            return new KeyValuePair<TKey, TValue>(key, value);
        }

        public Task<int> SaveAsync(DbConnection connection, IDictionaryTracker<TKey, TValue> tracker,
                                   params object[] keyValues)
        {
            return SaveAsync(connection, (TrackableDictionaryTracker<TKey, TValue>)tracker, keyValues);
        }

        public async Task<int> SaveAsync(DbConnection connection, TrackableDictionaryTracker<TKey, TValue> tracker,
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
