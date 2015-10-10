using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;

namespace TrackableData
{
    public class TrackablePocoTrackerJsonConverter<T> : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(TrackablePocoTracker<T>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
                                        JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.StartObject)
                return null;

            var tracker = new TrackablePocoTracker<T>();
            reader.Read();
            while (true)
            {
                if (reader.TokenType != JsonToken.PropertyName)
                    break;

                var pi = (typeof(T)).GetProperty((string)reader.Value);
                reader.Read();

                var obj = serializer.Deserialize(reader, pi.PropertyType);
                reader.Read();

                tracker.TrackSet(pi, null, obj);
            }

            return tracker;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var tracker = (TrackablePocoTracker<T>)value;

            writer.WriteStartObject();

            foreach (var item in tracker.ChangeMap)
            {
                writer.WritePropertyName(item.Key.Name);
                serializer.Serialize(writer, item.Value);
            }

            writer.WriteEndObject();
        }
    }
}
