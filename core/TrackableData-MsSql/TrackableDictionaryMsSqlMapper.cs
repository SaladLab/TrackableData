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
    public class TrackableDictionaryMsSqlMapper<TKey, TValue> where TValue : new()
    {
        private class Column
        {
            public string Name;
            public Type Type;
            public FieldInfo FieldInfo;
            public Func<object, string> ConvertToSqlValue;
        }

        private readonly string _tableName;
        private readonly Column _headKeyColumn;
        private readonly Column _keyColumn;
        private readonly Column[] _valueColumns;
        private readonly string _allColumnString;
        private readonly string _allColumnStringExceptHead;

        public TrackableDictionaryMsSqlMapper(string tableName)
        {
        }

        public TrackableDictionaryMsSqlMapper(string tableName,
                                              string headKeyColumnName, Type headKeyType,
                                              string keyColumnName, string valueColumnName = null)
        {
            _tableName = tableName;

            // 키 컬럼 구성

            if (string.IsNullOrEmpty(headKeyColumnName) == false)
            {
                _headKeyColumn = new Column
                {
                    Name = SqlMapperHelper.GetEscapedName(headKeyColumnName),
                    Type = headKeyType,
                    ConvertToSqlValue = SqlMapperHelper.GetSqlValueFunc(headKeyType)
                };
            }

            _keyColumn = new Column
            {
                Name = SqlMapperHelper.GetEscapedName(keyColumnName),
                Type = typeof(TKey),
                ConvertToSqlValue = SqlMapperHelper.GetSqlValueFunc(typeof(TKey))
            };

            // 값 컬럼 구성

            if (valueColumnName != null)
            {
                _valueColumns = new[]
                {
                    new Column
                    {
                        Name = SqlMapperHelper.GetEscapedName(valueColumnName),
                        Type = typeof(TValue),
                        ConvertToSqlValue = SqlMapperHelper.GetSqlValueFunc(typeof(TValue))
                    }
                };
            }
            else
            {
                //var valueColumns = new List<Column>();
                //var valueType = typeof(TValue);
                //foreach (var field in valueType.GetFields())
                //{
                //    var attr = field.GetCustomAttribute<TrackableFieldAttribute>();
                //    if (attr != null && attr.IsColumnIgnored == false)
                //    {
                //        valueColumns.Add(new Column
                //        {
                //            Name = SqlMapperHelper.GetEscapedName(attr.ColumnName ?? field.Name),
                //            Type = field.FieldType,
                //            FieldInfo = field,
                //            ConvertToSqlValue = SqlMapperHelper.GetSqlValueFunc(field)
                //        });
                //    }
                //}
                // _valueColumns = valueColumns.ToArray();
            }

            // 컬럼 문자열 구축

            var columnNames = new List<string>();
            columnNames.Add(_keyColumn.Name);
            columnNames.AddRange(_valueColumns.Select(c => c.Name));
            _allColumnStringExceptHead = string.Join(",", columnNames);
            _allColumnString = _headKeyColumn != null
                                   ? _headKeyColumn.Name + "," + _allColumnStringExceptHead
                                   : _allColumnStringExceptHead;
        }

        // SQL Friendly Methods

        public string GenerateCreateTableSql(bool includeDropIfExists = false)
        {
            var sb = new StringBuilder();
            return sb.ToString();
        }

        public string GenerateSaveSql(object headKey, TrackableDictionaryTracker<TKey, TValue> tracker)
        {
            if (tracker.ChangeMap.Any() == false)
                return string.Empty;

            var sqlAdd = new StringBuilder();
            var sqlModify = new StringBuilder();
            var removeIds = new List<TKey>();

            // 변경사항을 돌면서 변경 타입별 SQL 구문 생성

            var insertCount = 0;
            foreach (var i in tracker.ChangeMap)
            {
                var v = i.Value.NewValue;
                switch (i.Value.Operation)
                {
                    case TrackableDictionaryOperation.Add:
                        if (insertCount == 0)
                        {
                            sqlAdd.Append("INSERT INTO ").Append(_tableName);
                            sqlAdd.Append(" (").Append(_allColumnString).Append(") VALUES\n");
                        }
                        else
                        {
                            sqlAdd.Append(",\n");
                        }

                        sqlAdd.Append(" (");

                        if (_headKeyColumn != null)
                        {
                            sqlAdd.Append(_headKeyColumn.ConvertToSqlValue(headKey));
                            sqlAdd.Append(",");
                            sqlAdd.Append(_keyColumn.ConvertToSqlValue(i.Key));
                        }
                        else
                        {
                            sqlAdd.Append(_keyColumn.ConvertToSqlValue(i.Key));
                        }

                        foreach (var col in _valueColumns)
                            sqlAdd.Append(",").Append(col.ConvertToSqlValue(v));

                        sqlAdd.Append(")");

                        insertCount += 1;
                        if (insertCount >= 1000)
                        {
                            sqlAdd.Append(";\n");
                            insertCount = 0;
                        }
                        break;

                    case TrackableDictionaryOperation.Modify:
                        sqlModify.Append("UPDATE ").Append(_tableName);
                        sqlModify.Append(" SET ");
                        var concating = false;
                        foreach (var col in _valueColumns)
                        {
                            if (concating == false)
                                concating = true;
                            else
                                sqlModify.Append(",");
                            sqlModify.Append(col.Name).Append("=").Append(col.ConvertToSqlValue(v));
                        }

                        sqlModify.Append(" WHERE ");
                        if (_headKeyColumn != null)
                        {
                            sqlModify.Append(_headKeyColumn.Name).Append("=");
                            sqlModify.Append(_headKeyColumn.ConvertToSqlValue(headKey));
                            sqlModify.Append(" AND ");
                        }
                        sqlModify.Append(_keyColumn.Name).Append("=");
                        sqlModify.Append(_keyColumn.ConvertToSqlValue(i.Key)).Append(";\n");
                        break;

                    case TrackableDictionaryOperation.Remove:
                        removeIds.Add(i.Key);
                        break;
                }
            }
            if (insertCount > 0)
                sqlAdd.Append(";\n");

            // INSERT, UPDATE, DELETE SQL 문을 합쳐서 하나로 만들기

            var sql = new StringBuilder();
            sql.Append(sqlAdd);
            sql.Append(sqlModify);
            if (removeIds.Any())
            {
                sql.Append("DELETE ").Append(_tableName).Append(" WHERE ");
                if (_headKeyColumn != null)
                {
                    sql.Append(_headKeyColumn.Name).Append("=");
                    sql.Append(_headKeyColumn.ConvertToSqlValue(headKey));
                    sql.Append(" AND ");
                }
                sql.Append(_keyColumn.Name).Append(" IN (");
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

        // POCO Friendly Methods

        public async Task<int> ResetTableAsync(SqlConnection connection)
        {
            var sql = GenerateCreateTableSql(true);
            using (var command = new SqlCommand(sql, connection))
            {
                return await command.ExecuteNonQueryAsync();
            }
        }

        public async Task<TrackableDictionary<TKey, TValue>> LoadAsync(SqlConnection connection, params object[] keys)
        {
            /*
            // SELECT 쿼리문 구축

            var sb = new StringBuilder();
            sb.Append("SELECT ");
            sb.Append(_allColumnStringExceptHead);
            sb.Append(" FROM ");
            sb.Append(_tableName);
            if (_headKeyColumn != null)
            {
                sb.Append(" WHERE ");
                sb.Append(_headKeyColumn.Value.Name);
                sb.Append("=");
                sb.Append(_headKeyColumn.Value.ConvertToSqlValue(headKey));
            }

            // 쿼리를 실행해서 Rowset 으로 부터 Dictionary 구성

            var items = new TrackableDictionary<TKey, TValue>();
            using (var rowset = await session.ExecuteReaderAsync(sb.ToString()))
            {
                while (await rowset.ReadAsync())
                {
                    var key = (TKey)SqlMapperHelper.GetNetValue(rowset.GetValue(0), typeof(TKey));
                    var value = new TValue();

                    if (_valueColumns.Length == 1 && _valueColumns[0].FieldInfo == null)
                    {
                        value = (TValue)SqlMapperHelper.GetNetValue(rowset.GetValue(1), typeof(TValue));
                    }
                    else
                    {
                        for (var i = 0; i < _valueColumns.Length; i++)
                        {
                            _valueColumns[i].FieldInfo.SetValue(
                                value,
                                SqlMapperHelper.GetNetValue(rowset.GetValue(i + 1), _valueColumns[i].Type));
                        }
                    }
                    items.Add(key, value);
                }
            }
            */
            var items = new TrackableDictionary<TKey, TValue>();
            return items;
        }

        private KeyValuePair<TKey, TValue> ConvertToPoco(IDataRecord record)
        {
            return new KeyValuePair<TKey, TValue>();
        }

        public async Task SaveAsync(SqlConnection connection, TrackableDictionaryTracker<TKey, TValue> tracker, params object[] keyValues)
        {
            /*
            var sql = GenerateSaveSql(headKey, tracker);
            if (string.IsNullOrEmpty(sql) == false)
                await session.ExecuteNonQueryAsync(sql);
            */
        }
    }
}
