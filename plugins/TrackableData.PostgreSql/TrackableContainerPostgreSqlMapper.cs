using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace TrackableData.PostgreSql
{
    public class TrackableContainerPostgreSqlMapper<T>
        where T : ITrackableContainer<T>
    {
        private readonly Type _trackableType;

        private class PropertyItem
        {
            public string Name;
            public PropertyInfo Property;
            public PropertyInfo TrackerProperty;
            public object Mapper;
            public Func<string> BuildCreateTableSql;
            public Func<T, object[], string> BuildSqlForCreate;
            public Func<object[], string> BuildSqlForDelete;
            public Func<NpgsqlConnection, object[], T, Task> LoadAndSetAsync;
            public Func<IContainerTracker<T>, object[], string> BuildSqlForSave;
        }

        private readonly PropertyItem[] PropertyItems;

        public TrackableContainerPostgreSqlMapper(Tuple<string, object[]>[] mapperParameters)
        {
            _trackableType = TrackableResolver.GetContainerTrackerType(typeof(T));

            PropertyItems = ConstructPropertyItems(mapperParameters);
        }

        #region Property Accessor

        private static PropertyItem[] ConstructPropertyItems(Tuple<string, object[]>[] mapperParameters)
        {
            var trackerType = TrackerResolver.GetDefaultTracker(typeof(T));
            var mapperParameterMap = mapperParameters.ToDictionary(x => x.Item1, x => x.Item2);

            var propertyItems = new List<PropertyItem>();
            foreach (var property in typeof(T).GetProperties())
            {
                var item = new PropertyItem
                {
                    Name = property.Name,
                    Property = property,
                    TrackerProperty = trackerType.GetProperty(property.Name + "Tracker")
                };

                object[] mapperParameter;
                if (mapperParameterMap.TryGetValue(property.Name, out mapperParameter) == false)
                    throw new ArgumentException($"{property.Name} needs mapperParameter.");

                if (TrackableResolver.IsTrackablePoco(property.PropertyType))
                {
                    typeof(TrackableContainerPostgreSqlMapper<T>)
                        .GetMethod("BuildTrackablePocoProperty", BindingFlags.Static | BindingFlags.NonPublic)
                        .MakeGenericMethod(TrackableResolver.GetPocoType(property.PropertyType))
                        .Invoke(null, new object[] { item, mapperParameter });
                }
                else if (TrackableResolver.IsTrackableDictionary(property.PropertyType))
                {
                    typeof(TrackableContainerPostgreSqlMapper<T>)
                        .GetMethod("BuildTrackableDictionaryProperty", BindingFlags.Static | BindingFlags.NonPublic)
                        .MakeGenericMethod(property.PropertyType.GetGenericArguments())
                        .Invoke(null, new object[] { item, mapperParameter });
                }
                else
                {
                    throw new InvalidOperationException("Cannot resolve property: " + property.Name);
                }

                propertyItems.Add(item);
            }
            return propertyItems.ToArray();
        }

        private static void BuildTrackablePocoProperty<TPoco>(PropertyItem item, object[] mapperParameters)
            where TPoco : ITrackablePoco<TPoco>
        {
            if (mapperParameters.Length != 2)
                throw new ArgumentException("The length of mapperParameters should be  2");

            var mapper = new TrackablePocoPostgreSqlMapper<TPoco>((string)mapperParameters[0],
                                                                  (ColumnDefinition[])mapperParameters[1]);
            item.Mapper = mapper;

            item.BuildCreateTableSql = () => { return mapper.BuildCreateTableSql(true); };
            item.BuildSqlForCreate = (container, keyValues) =>
            {
                var value = (TPoco)item.Property.GetValue(container);
                return value != null
                           ? mapper.BuildSqlForCreate(value, keyValues)
                           : string.Empty;
            };
            item.BuildSqlForDelete = (keyValues) => { return mapper.BuildSqlForDelete(keyValues); };
            item.LoadAndSetAsync = async (connection, keyValues, container) =>
            {
                var value = await mapper.LoadAsync(connection, keyValues);
                item.Property.SetValue(container, value);
            };
            item.BuildSqlForSave = (tracker, keyValues) =>
            {
                var valueTracker = (TrackablePocoTracker<TPoco>)item.TrackerProperty.GetValue(tracker);
                return valueTracker.HasChange
                           ? mapper.BuildSqlForSave(valueTracker, keyValues)
                           : string.Empty;
            };
        }

        private static void BuildTrackableDictionaryProperty<TKey, TValue>(PropertyItem item, object[] mapperParameters)
        {
            if (mapperParameters.Length != 3 && mapperParameters.Length != 4)
                throw new ArgumentException("The length of mapperParameters should be 3 or 4");

            var mapper = mapperParameters.Length == 3
                             ? new TrackableDictionaryPostgreSqlMapper<TKey, TValue>(
                                   (string)mapperParameters[0],
                                   (ColumnDefinition)mapperParameters[1],
                                   (ColumnDefinition[])mapperParameters[2])
                             : new TrackableDictionaryPostgreSqlMapper<TKey, TValue>(
                                   (string)mapperParameters[0],
                                   (ColumnDefinition)mapperParameters[1],
                                   (ColumnDefinition)mapperParameters[2],
                                   (ColumnDefinition[])mapperParameters[3]);
            item.Mapper = mapper;

            item.BuildCreateTableSql = () => { return mapper.BuildCreateTableSql(true); };
            item.BuildSqlForCreate = (container, keyValues) =>
            {
                var value = (IDictionary<TKey, TValue>)item.Property.GetValue(container);
                return value != null
                           ? mapper.BuildSqlForCreate(value, keyValues)
                           : string.Empty;
            };
            item.BuildSqlForDelete = (keyValues) => { return mapper.BuildSqlForDelete(keyValues); };
            item.LoadAndSetAsync = async (connection, keyValues, container) =>
            {
                var value = await mapper.LoadAsync(connection, keyValues);
                item.Property.SetValue(container, value);
            };
            item.BuildSqlForSave = (tracker, keyValues) =>
            {
                var valueTracker = (TrackableDictionaryTracker<TKey, TValue>)item.TrackerProperty.GetValue(tracker);
                return valueTracker.HasChange
                           ? mapper.BuildSqlForSave(valueTracker, keyValues)
                           : string.Empty;
            };
        }

        #endregion

        #region Helpers

        public async Task<int> ResetTableAsync(NpgsqlConnection connection)
        {
            var sql = new StringBuilder();
            foreach (var pi in PropertyItems)
            {
                sql.Append(pi.BuildCreateTableSql());
            }
            using (var command = new NpgsqlCommand(sql.ToString(), connection))
            {
                return await command.ExecuteNonQueryAsync();
            }
        }

        public async Task<int> CreateAsync(NpgsqlConnection connection, T value, params object[] keyValues)
        {
            var sql = new StringBuilder();
            foreach (var pi in PropertyItems)
            {
                sql.Append(pi.BuildSqlForCreate(value, keyValues));
            }
            using (var command = new NpgsqlCommand(sql.ToString(), connection))
            {
                return await command.ExecuteNonQueryAsync();
            }
        }

        public async Task<int> DeleteAsync(NpgsqlConnection connection, params object[] keyValues)
        {
            var sql = new StringBuilder();
            foreach (var pi in PropertyItems)
            {
                sql.Append(pi.BuildSqlForDelete(keyValues));
            }
            using (var command = new NpgsqlCommand(sql.ToString(), connection))
            {
                return await command.ExecuteNonQueryAsync();
            }
        }

        public async Task<T> LoadAsync(NpgsqlConnection connection, params object[] keyValues)
        {
            var container = (T)Activator.CreateInstance(_trackableType);
            foreach (var pi in PropertyItems)
            {
                await pi.LoadAndSetAsync(connection, keyValues, container);
            }
            return container;
        }

        public Task<int> SaveAsync(NpgsqlConnection connection, ITracker tracker, params object[] keyValues)
        {
            return SaveAsync(connection, (IContainerTracker<T>)tracker, keyValues);
        }

        public async Task<int> SaveAsync(NpgsqlConnection connection, IContainerTracker<T> tracker,
                                         params object[] keyValues)
        {
            if (tracker.HasChange == false)
                return 0;

            var sql = new StringBuilder();
            foreach (var pi in PropertyItems)
            {
                sql.Append(pi.BuildSqlForSave(tracker, keyValues));
            }
            using (var command = new NpgsqlCommand(sql.ToString(), connection))
            {
                return await command.ExecuteNonQueryAsync();
            }
        }

        #endregion
    }
}
