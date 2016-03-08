#if !NET35

using System;
using System.Reflection;
using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace TrackableData.Json
{
    public class TrackerJsonConverter : JsonConverter
    {
        private ConcurrentDictionary<Type, JsonConverter> _converterMap =
            new ConcurrentDictionary<Type, JsonConverter>();

        public override bool CanConvert(Type objectType)
        {
            return typeof(ITracker).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
                                        JsonSerializer serializer)
        {
            var converter = FindConverter(objectType);
            if (converter == null)
                throw new InvalidOperationException("Cannot convert type: " + objectType);

            return converter.ReadJson(reader, objectType, existingValue, serializer);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var objectType = value.GetType();
            var converter = FindConverter(objectType);
            if (converter == null)
                throw new InvalidOperationException("Cannot convert type: " + objectType);

            converter.WriteJson(writer, value, serializer);
        }

        private JsonConverter FindConverter(Type objectType)
        {
            return _converterMap.GetOrAdd(objectType, type =>
            {
                if (typeof(IPocoTracker).IsAssignableFrom(type))
                {
                    var trackerType = typeof(TrackablePocoTrackerJsonConverter<>)
                        .MakeGenericType(type.GetGenericArguments());
                    return (JsonConverter)Activator.CreateInstance(trackerType);
                }
                if (typeof(IContainerTracker).IsAssignableFrom(type))
                {
                    return new TrackableContainerTrackerJsonConverter();
                }
                if (typeof(IDictionaryTracker).IsAssignableFrom(type))
                {
                    var trackerType = typeof(TrackableDictionaryTrackerJsonConverter<,>)
                        .MakeGenericType(type.GetGenericArguments());
                    return (JsonConverter)Activator.CreateInstance(trackerType);
                }
                if (typeof(IListTracker).IsAssignableFrom(type))
                {
                    var trackerType = typeof(TrackableListTrackerJsonConverter<>)
                        .MakeGenericType(type.GetGenericArguments());
                    return (JsonConverter)Activator.CreateInstance(trackerType);
                }
                return null;
            });
        }
    }
}

#endif