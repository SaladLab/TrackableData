using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace TrackableData.MongoDB
{
    public class TrackableMongoDbMapper
    {
        public static Type GetMapperType(Type trackableType)
        {
            if (typeof(ITrackablePoco).IsAssignableFrom(trackableType))
            {
                var trackerType = trackableType.GetInterfaces()
                                               .FirstOrDefault(t => t.IsGenericType &&
                                                                    t.GetGenericTypeDefinition() == typeof(ITrackable<>));
                if (trackerType != null)
                    return typeof(TrackablePocoMongoDbMapper<>).MakeGenericType(trackerType.GetGenericArguments()[0]);
            }
            if (trackableType.IsGenericType)
            {
                var genericType = trackableType.GetGenericTypeDefinition();
                if (genericType == typeof(TrackableDictionary<,>))
                {
                    return typeof(TrackableDictionaryMongoDbMapper<,>).MakeGenericType(
                        trackableType.GetGenericArguments());
                }
                if (genericType == typeof(TrackableList<>))
                {
                    return typeof(TrackableListMongoDbMapper<>).MakeGenericType(
                        trackableType.GetGenericArguments());
                }
            }
            return null;
        }

        public static Type GetPocoInterfaceType(Type trackableType)
        {
            if (typeof(ITrackablePoco).IsAssignableFrom(trackableType))
            {
                var trackerType = trackableType.GetInterfaces()
                                               .FirstOrDefault(t => t.IsGenericType &&
                                                                    t.GetGenericTypeDefinition() == typeof(ITrackable<>));
                if (trackerType != null)
                    return trackerType.GetGenericArguments()[0];
            }

            return null;
        }

        public static
            Func<UpdateDefinition<BsonDocument>, TTrackablePoco, object[], UpdateDefinition<BsonDocument>>
            CreatePocoUpdateFunc<TTrackablePoco>()
        {
            var pocoType = GetPocoInterfaceType(typeof(TTrackablePoco));
            if (pocoType == null)
                return null;

            var genericMethod = typeof(TrackableMongoDbMapper).GetMethod(
                "GeneratePocoUpdateBson", BindingFlags.Static | BindingFlags.NonPublic);
            var method = genericMethod.MakeGenericMethod(pocoType, typeof(TTrackablePoco));
            var mapper = Activator.CreateInstance(GetMapperType(typeof(TTrackablePoco)));
            var func = method.Invoke(null, new object[] { mapper });
            return (Func<UpdateDefinition<BsonDocument>, TTrackablePoco, object[], UpdateDefinition<BsonDocument>>)func;
        }

        private static Func<UpdateDefinition<BsonDocument>, TTrackablePoco, object[], UpdateDefinition<BsonDocument>>
            GeneratePocoUpdateBson<TPoco, TTrackablePoco>(TrackablePocoMongoDbMapper<TPoco> mapper)
            where TPoco : ITrackablePoco
            where TTrackablePoco : ITrackable<TPoco>
        {
            return (update, trackablePoco, keyValues) =>
                   mapper.GenerateUpdateBson(update, (TrackablePocoTracker<TPoco>)trackablePoco.Tracker, keyValues);
        }
    }
}
