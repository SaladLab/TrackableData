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
    public class TrackableSetSqlMapper<T>
    {
        private readonly ISqlProvider _sqlProvider;
        private readonly string _tableName;
        private readonly string _tableEscapedName;
        private readonly ColumnProperty[] _allColumns;
        private readonly ColumnProperty[] _headKeyColumns;
        private readonly ColumnProperty _valueColumn;
        private readonly string _allColumnString;

        public TrackableSetSqlMapper(ISqlProvider sqlProvider,
                                     string tableName,
                                     ColumnDefinition valueColumnDef,
                                     ColumnDefinition[] headKeyColumnDefs = null)
        {
            _sqlProvider = sqlProvider;

            _tableName = tableName;
            _tableEscapedName = _sqlProvider.EscapeName(tableName);

            var allColumns = new List<ColumnProperty>();
            var headKeyColumns = new List<ColumnProperty>();
            var primaryKeyColumns = new List<ColumnProperty>();

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

            // add value column

            _valueColumn = new ColumnProperty(
                name: valueColumnDef.Name,
                escapedName: _sqlProvider.EscapeName(valueColumnDef.Name),
                type: typeof(T),
                length: valueColumnDef.Length,
                convertToSqlValue: _sqlProvider.GetConvertToSqlValueFunc(typeof(T)),
                convertFromDbValue: _sqlProvider.GetConvertFromDbValueFunc(typeof(T)));

            primaryKeyColumns.Add(_valueColumn);
            allColumns.Add(_valueColumn);

            _allColumns = allColumns.ToArray();
            _headKeyColumns = headKeyColumns.ToArray();

            _allColumnString = string.Join(",", _allColumns.Select(c => c.EscapedName));
        }

        private void BuildWhereClauses(StringBuilder sb, params object[] keyValues)
        {
            if (keyValues.Length <= 0)
                return;

            sb.Append(" WHERE ");
            sb.Append(string.Join(
                " AND ",
                keyValues.Zip(_allColumns,
                              (v, c) => $"{c.EscapedName}={c.ConvertToSqlValue(v)}")));
        }

        public string BuildCreateTableSql(bool dropIfExists = false)
        {
            return _sqlProvider.BuildCreateTableSql(_tableName,
                                                    _allColumns,
                                                    _headKeyColumns.Concat(new[] { _valueColumn }).ToArray(),
                                                    dropIfExists);
        }

        public string BuildSqlForCreate(ICollection<T> set, params object[] keyValues)
        {
            if (keyValues.Length != _headKeyColumns.Length)
                throw new ArgumentException("Number of keyValues should be same with the number of head columns");

            var sql = new StringBuilder();

            // generate sql command for each rows

            var insertCount = 0;
            foreach (var i in set)
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
                sql.Append(_valueColumn.ConvertToSqlValue(i));
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
            sb.Append($"SELECT {_valueColumn.EscapedName} FROM {_tableEscapedName}");
            BuildWhereClauses(sb, keyValues);
            sb.Append(";\n");
            return sb.ToString();
        }

        public string BuildSqlForSave(TrackableSetTracker<T> tracker,
                                      params object[] keyValues)
        {
            if (keyValues.Length != _headKeyColumns.Length)
                throw new ArgumentException("Number of keyValues should be same with the number of head columns");

            var sqlAdd = new StringBuilder();
            var removeValues = new List<T>();

            // generate sql command for each changes

            var insertCount = 0;
            foreach (var i in tracker.ChangeMap)
            {
                switch (i.Value)
                {
                    case TrackableSetOperation.Add:
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
                        sqlAdd.Append(_valueColumn.ConvertToSqlValue(i.Key));
                        sqlAdd.Append(")");

                        insertCount += 1;
                        if (insertCount >= 1000)
                        {
                            sqlAdd.Append(";\n");
                            insertCount = 0;
                        }
                        break;

                    case TrackableSetOperation.Remove:
                        removeValues.Add(i.Key);
                        break;
                }
            }
            if (insertCount > 0)
                sqlAdd.Append(";\n");

            // merge insert, update and delete sql into one sql

            var sql = new StringBuilder();
            sql.Append(sqlAdd);
            if (removeValues.Any())
            {
                sql.Append("DELETE FROM ").Append(_tableEscapedName).Append(" WHERE ");
                for (var k = 0; k < _headKeyColumns.Length; k++)
                {
                    sql.Append(_headKeyColumns[k].EscapedName).Append("=");
                    sql.Append(_headKeyColumns[k].ConvertToSqlValue(keyValues[k]));
                    sql.Append(" AND ");
                }
                sql.Append(_valueColumn.EscapedName).Append(" IN (");
                var concating = false;
                foreach (var id in removeValues)
                {
                    if (concating == false)
                        concating = true;
                    else
                        sql.Append(",");
                    sql.Append(_valueColumn.ConvertToSqlValue(id));
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

        public async Task<int> CreateAsync(DbConnection connection, ICollection<T> set,
                                           params object[] keyValues)
        {
            var sql = BuildSqlForCreate(set, keyValues);
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

        public Task<TrackableSet<T>> LoadAsync(DbConnection connection,
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

        public async Task<TrackableSet<T>> LoadAsync(DbDataReader reader)
        {
            var set = new TrackableSet<T>();
            while (await reader.ReadAsync())
            {
                var value = (T)_valueColumn.ConvertFromDbValue(reader.GetValue(0));
                set.Add(value);
            }
            return set;
        }

        public Task<int> SaveAsync(DbConnection connection, ISetTracker<T> tracker,
                                   params object[] keyValues)
        {
            return SaveAsync(connection, (TrackableSetTracker<T>)tracker, keyValues);
        }

        public async Task<int> SaveAsync(DbConnection connection, TrackableSetTracker<T> tracker,
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
