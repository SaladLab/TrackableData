using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TrackableData
{
    public class TrackablePocoSqlMapper<T, TTrackable>
        where T : class, new()
        where TTrackable : class, new()
    {
        private struct Column
        {
            public string Name;
            public Type Type;
            public PropertyInfo PropertyInfo;
            public Func<object, string> ConvertToSqlValue;
        }

        private readonly string _tableName;
        private readonly Column? _headKeyColumn;
        private readonly Column[] _valueColumns;
        private readonly Dictionary<PropertyInfo, Column> _valueColumnMap;
        private readonly string _allColumnStringExceptHead;

        public TrackablePocoSqlMapper(string tableName, string headKeyColumnName, Type headKeyType)
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

            // 값 컬럼 구성

            var valueColumns = new List<Column>();
            var valueType = typeof(TTrackable);
            foreach (var property in valueType.GetProperties())
            {
                var attr = property.GetCustomAttribute<TrackableFieldAttribute>();
                if (attr != null && attr.IsColumnIgnored == false)
                {
                    valueColumns.Add(new Column
                    {
                        Name = SqlMapperHelper.GetEscapedName(attr.ColumnName ?? property.Name),
                        Type = property.PropertyType,
                        PropertyInfo = property,
                        ConvertToSqlValue = SqlMapperHelper.GetSqlValueFunc(property.PropertyType)
                    });
                }
            }
            _valueColumns = valueColumns.ToArray();
            _valueColumnMap = new Dictionary<PropertyInfo, Column>();
            foreach (var column in _valueColumns)
                _valueColumnMap[column.PropertyInfo] = column;

            // 컬럼 문자열 구축

            var columnNames = new List<string>();
            columnNames.AddRange(_valueColumns.Select(c => c.Name));
            _allColumnStringExceptHead = string.Join(",", columnNames);
        }

        /*
        public async Task<TTrackable> LoadAsync(DbSession session, object headKey)
        {
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

            // 쿼리를 실행해서 Rowset 으로 부터 Value 구성

            using (var rowset = await session.ExecuteReaderAsync(sb.ToString()))
            {
                while (await rowset.ReadAsync())
                {
                    var value = new TTrackable();
                    for (var i = 0; i < _valueColumns.Length; i++)
                    {
                        _valueColumns[i].PropertyInfo.SetValue(
                            value,
                            SqlMapperHelper.GetNetValue(rowset.GetValue(i), _valueColumns[i].Type));
                    }
                    return value;
                }
            }
            return null;
        }

        public async Task SaveAsync(DbSession session, object headKey, TrackablePocoTracker<T> tracker)
        {
            var sql = GenerateSaveSql(headKey, tracker);
            if (string.IsNullOrEmpty(sql) == false)
                await session.ExecuteNonQueryAsync(sql);
        }
        */

        public string GenerateSaveSql(object headKey, TrackablePocoTracker<T> tracker)
        {
            if (tracker.ChangeMap.Any() == false)
                return string.Empty;

            var sb = new StringBuilder();
            sb.Append("UPDATE ");
            sb.Append(_tableName);

            sb.Append(" SET ");
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
                sb.Append(column.ConvertToSqlValue(c.Value));
            }

            if (concating == false)
                return string.Empty;

            if (_headKeyColumn != null)
            {
                sb.Append(" WHERE ");
                sb.Append(_headKeyColumn.Value.Name);
                sb.Append("=");
                sb.Append(_headKeyColumn.Value.ConvertToSqlValue(headKey));
            }
            sb.Append(";\n");

            return sb.ToString();
        }
    }
}
