using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TrackableData.Sql
{
    public class TrackableContainerSqlMapper<T>
        where T : ITrackableContainer<T>
    {
        private readonly ISqlProvider _sqlProvider;
        private readonly Type _trackableType;

        private class PropertyItem
        {
            public string Name;
            public PropertyInfo PropertyInfo;
            public PropertyInfo TrackerPropertyInfo;
            public object Mapper;
            public Func<bool, string> BuildCreateTableSql;
            public Func<T, object[], string> BuildSqlForCreate;
            public Func<object[], string> BuildSqlForDelete;
            public Func<object[], string> BuildSqlForLoad;
            public Func<DbDataReader, T, Task<bool>> LoadAndSetAsync;
            public Func<IContainerTracker<T>, object[], string> BuildSqlForSave;
        }

        private readonly PropertyItem[] _items;

        public TrackableContainerSqlMapper(ISqlProvider sqlProvider, Tuple<string, object[]>[] mapperParameters)
        {
            _sqlProvider = sqlProvider;

            _trackableType = TrackableResolver.GetContainerTrackerType(typeof(T));
            if (_trackableType == null)
                throw new ArgumentException($"Cannot find tracker type of '{nameof(T)}'");

            _items = ConstructPropertyItems(sqlProvider, mapperParameters);
        }

        private static PropertyItem[] ConstructPropertyItems(ISqlProvider sqlProvider,
                                                             Tuple<string, object[]>[] mapperParameters)
        {
            var trackerType = TrackerResolver.GetDefaultTracker(typeof(T));
            var mapperParameterMap = mapperParameters.ToDictionary(x => x.Item1, x => x.Item2);

            var items = new List<PropertyItem>();
            foreach (var property in typeof(T).GetProperties())
            {
                var attr = property.GetCustomAttribute<TrackablePropertyAttribute>();
                if (attr != null)
                {
                    if (attr["sql.ignore"] != null)
                        continue;
                }

                var item = new PropertyItem
                {
                    Name = property.Name,
                    PropertyInfo = property,
                    TrackerPropertyInfo = trackerType.GetProperty(property.Name + "Tracker")
                };

                if (item.TrackerPropertyInfo == null)
                    throw new ArgumentException($"Cannot find tracker type of '{property.Name}'");

                object[] mapperParameter;
                if (mapperParameterMap.TryGetValue(property.Name, out mapperParameter) == false)
                    throw new ArgumentException($"{property.Name} needs mapperParameter.");

                if (TrackableResolver.IsTrackablePoco(property.PropertyType))
                {
                    typeof(TrackableContainerSqlMapper<T>)
                        .GetMethod("BuildTrackablePocoProperty", BindingFlags.Static | BindingFlags.NonPublic)
                        .MakeGenericMethod(TrackableResolver.GetPocoType(property.PropertyType))
                        .Invoke(null, new object[] { sqlProvider, item, mapperParameter });
                }
                else if (TrackableResolver.IsTrackableDictionary(property.PropertyType))
                {
                    typeof(TrackableContainerSqlMapper<T>)
                        .GetMethod("BuildTrackableDictionaryProperty", BindingFlags.Static | BindingFlags.NonPublic)
                        .MakeGenericMethod(property.PropertyType.GetGenericArguments())
                        .Invoke(null, new object[] { sqlProvider, item, mapperParameter });
                }
                else if (TrackableResolver.IsTrackableSet(property.PropertyType))
                {
                    typeof(TrackableContainerSqlMapper<T>)
                        .GetMethod("BuildTrackableSetProperty", BindingFlags.Static | BindingFlags.NonPublic)
                        .MakeGenericMethod(property.PropertyType.GetGenericArguments())
                        .Invoke(null, new object[] { sqlProvider, item, mapperParameter });
                }
                else
                {
                    throw new InvalidOperationException("Cannot resolve property: " + property.Name);
                }

                items.Add(item);
            }
            return items.ToArray();
        }

        private static void BuildTrackablePocoProperty<TPoco>(ISqlProvider sqlProvider,
                                                              PropertyItem item,
                                                              object[] mapperParameters)
            where TPoco : ITrackablePoco<TPoco>
        {
            if (mapperParameters.Length != 2)
                throw new ArgumentException("The length of mapperParameters should be 2");

            var mapper = new TrackablePocoSqlMapper<TPoco>(sqlProvider,
                                                           (string)mapperParameters[0],
                                                           (ColumnDefinition[])mapperParameters[1]);
            item.Mapper = mapper;

            item.BuildCreateTableSql = (dropIfExists) =>
            {
                return mapper.BuildCreateTableSql(dropIfExists);
            };
            item.BuildSqlForCreate = (container, keyValues) =>
            {
                var value = (TPoco)item.PropertyInfo.GetValue(container);
                return value != null
                           ? mapper.BuildSqlForCreate(value, keyValues)
                           : string.Empty;
            };
            item.BuildSqlForDelete = (keyValues) =>
            {
                return mapper.BuildSqlForDelete(keyValues);
            };
            item.BuildSqlForLoad = (keyValues) =>
            {
                return mapper.BuildSqlForLoad(keyValues);
            };
            item.LoadAndSetAsync = async (reader, container) =>
            {
                var value = await mapper.LoadAsync(reader);
                item.PropertyInfo.SetValue(container, value);
                return value != null;
            };
            item.BuildSqlForSave = (tracker, keyValues) =>
            {
                var valueTracker = (TrackablePocoTracker<TPoco>)item.TrackerPropertyInfo.GetValue(tracker);
                return valueTracker.HasChange
                           ? mapper.BuildSqlForSave(valueTracker, keyValues)
                           : string.Empty;
            };
        }

        private static void BuildTrackableDictionaryProperty<TKey, TValue>(ISqlProvider sqlProvider,
                                                                           PropertyItem item,
                                                                           object[] mapperParameters)
        {
            if (mapperParameters.Length != 3 && mapperParameters.Length != 4)
                throw new ArgumentException("The length of mapperParameters should be 3 or 4");

            var mapper = mapperParameters.Length == 3
                             ? new TrackableDictionarySqlMapper<TKey, TValue>(
                                   sqlProvider,
                                   (string)mapperParameters[0],
                                   (ColumnDefinition)mapperParameters[1],
                                   (ColumnDefinition[])mapperParameters[2])
                             : new TrackableDictionarySqlMapper<TKey, TValue>(
                                   sqlProvider,
                                   (string)mapperParameters[0],
                                   (ColumnDefinition)mapperParameters[1],
                                   (ColumnDefinition)mapperParameters[2],
                                   (ColumnDefinition[])mapperParameters[3]);
            item.Mapper = mapper;

            item.BuildCreateTableSql = (dropIfExists) =>
            {
                return mapper.BuildCreateTableSql(dropIfExists);
            };
            item.BuildSqlForCreate = (container, keyValues) =>
            {
                var value = (IDictionary<TKey, TValue>)item.PropertyInfo.GetValue(container);
                return value != null
                           ? mapper.BuildSqlForCreate(value, keyValues)
                           : string.Empty;
            };
            item.BuildSqlForDelete = (keyValues) =>
            {
                return mapper.BuildSqlForDelete(keyValues);
            };
            item.BuildSqlForLoad = (keyValues) =>
            {
                return mapper.BuildSqlForLoad(keyValues);
            };
            item.LoadAndSetAsync = async (reader, container) =>
            {
                var value = await mapper.LoadAsync(reader);
                item.PropertyInfo.SetValue(container, value);
                return value != null;
            };
            item.BuildSqlForSave = (tracker, keyValues) =>
            {
                var valueTracker = (TrackableDictionaryTracker<TKey, TValue>)item.TrackerPropertyInfo.GetValue(tracker);
                return valueTracker.HasChange
                           ? mapper.BuildSqlForSave(valueTracker, keyValues)
                           : string.Empty;
            };
        }

        private static void BuildTrackableSetProperty<TValue>(ISqlProvider sqlProvider,
                                                              PropertyItem item,
                                                              object[] mapperParameters)
        {
            if (mapperParameters.Length != 3)
                throw new ArgumentException("The length of mapperParameters should be 3");

            var mapper = new TrackableSetSqlMapper<TValue>(
                sqlProvider,
                (string)mapperParameters[0],
                (ColumnDefinition)mapperParameters[1],
                (ColumnDefinition[])mapperParameters[2]);
            item.Mapper = mapper;

            item.BuildCreateTableSql = (dropIfExists) =>
            {
                return mapper.BuildCreateTableSql(dropIfExists);
            };
            item.BuildSqlForCreate = (container, keyValues) =>
            {
                var value = (ICollection<TValue>)item.PropertyInfo.GetValue(container);
                return value != null
                           ? mapper.BuildSqlForCreate(value, keyValues)
                           : string.Empty;
            };
            item.BuildSqlForDelete = (keyValues) =>
            {
                return mapper.BuildSqlForDelete(keyValues);
            };
            item.BuildSqlForLoad = (keyValues) =>
            {
                return mapper.BuildSqlForLoad(keyValues);
            };
            item.LoadAndSetAsync = async (reader, container) =>
            {
                var value = await mapper.LoadAsync(reader);
                item.PropertyInfo.SetValue(container, value);
                return value != null;
            };
            item.BuildSqlForSave = (tracker, keyValues) =>
            {
                var valueTracker = (TrackableSetTracker<TValue>)item.TrackerPropertyInfo.GetValue(tracker);
                return valueTracker.HasChange
                           ? mapper.BuildSqlForSave(valueTracker, keyValues)
                           : string.Empty;
            };
        }

        public IEnumerable<ITrackable> GetTrackables(T container)
        {
            return _items.Select(item => (ITrackable)item.PropertyInfo.GetValue(container));
        }

        public IEnumerable<ITracker> GetTrackers(T container)
        {
            var tracker = container.Tracker;
            return _items.Select(item => (ITracker)item.TrackerPropertyInfo.GetValue(tracker));
        }

        public string BuildCreateTableSql(bool dropIfExists = false)
        {
            var sql = new StringBuilder();
            foreach (var pi in _items)
            {
                sql.Append(pi.BuildCreateTableSql(dropIfExists));
            }
            return sql.ToString();
        }

        public string BuildSqlForCreate(T value, params object[] keyValues)
        {
            var sql = new StringBuilder();
            foreach (var pi in _items)
            {
                sql.Append(pi.BuildSqlForCreate(value, keyValues));
            }
            return sql.ToString();
        }

        public string BuildSqlForDelete(params object[] keyValues)
        {
            var sql = new StringBuilder();
            foreach (var pi in _items)
            {
                sql.Append(pi.BuildSqlForDelete(keyValues));
            }
            return sql.ToString();
        }

        public string BuildSqlForLoad(params object[] keyValues)
        {
            var sql = new StringBuilder();
            foreach (var pi in _items)
            {
                sql.Append(pi.BuildSqlForLoad(keyValues));
            }
            return sql.ToString();
        }

        public string BuildSqlForSave(IContainerTracker<T> tracker, params object[] keyValues)
        {
            var sql = new StringBuilder();
            foreach (var pi in _items)
            {
                sql.Append(pi.BuildSqlForSave(tracker, keyValues));
            }
            return sql.ToString();
        }

        public async Task<int> ResetTableAsync(DbConnection connection, bool dropIfExists = false)
        {
            var sql = BuildCreateTableSql(dropIfExists);
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

        public async Task<T> LoadAsync(DbConnection connection, params object[] keyValues)
        {
            var container = (T)Activator.CreateInstance(_trackableType);
            var sql = BuildSqlForLoad(keyValues);
            using (var command = _sqlProvider.CreateDbCommand(sql, connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    foreach (var pi in _items)
                    {
                        var readed = await pi.LoadAndSetAsync(reader, container);
                        if (readed == false)
                            return default(T);
                        await reader.NextResultAsync();
                    }
                }
            }
            return container;
        }

        public async Task<T> LoadSerializedAsync(DbConnection connection, params object[] keyValues)
        {
            var container = (T)Activator.CreateInstance(_trackableType);
            foreach (var pi in _items)
            {
                var sql = pi.BuildSqlForLoad(keyValues);
                using (var command = _sqlProvider.CreateDbCommand(sql.ToString(), connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        var readed = await pi.LoadAndSetAsync(reader, container);
                        if (readed == false)
                            return default(T);
                    }
                }
            }
            return container;
        }

        public Task<int> SaveAsync(DbConnection connection, ITracker tracker, params object[] keyValues)
        {
            return SaveAsync(connection, (IContainerTracker<T>)tracker, keyValues);
        }

        public async Task<int> SaveAsync(DbConnection connection, IContainerTracker<T> tracker,
                                         params object[] keyValues)
        {
            var sql = BuildSqlForSave(tracker, keyValues);
            using (var command = _sqlProvider.CreateDbCommand(sql, connection))
            {
                return await command.ExecuteNonQueryAsync();
            }
        }
    }
}
