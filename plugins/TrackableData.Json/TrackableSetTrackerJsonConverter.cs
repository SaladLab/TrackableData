using System;
using System.Linq;
using Newtonsoft.Json;

namespace TrackableData.Json
{
    public class TrackableSetTrackerJsonConverter<T> : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(TrackableSetTracker<T>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
                                        JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.StartObject)
                return null;

            var tracker = new TrackableSetTracker<T>();
            reader.Read();
            while (true)
            {
                if (reader.TokenType != JsonToken.PropertyName)
                    break;

                var str = (string)reader.Value;
                reader.Read();

                var add = (str == "+");

                if (reader.TokenType != JsonToken.StartArray)
                    break;
                reader.Read();

                while (reader.TokenType != JsonToken.EndArray)
                {
                    var value = serializer.Deserialize<T>(reader);
                    reader.Read();
                    if (add)
                        tracker.TrackAdd(value);
                    else
                        tracker.TrackRemove(value);
                }
                reader.Read();
            }

            return tracker;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var tracker = (TrackableSetTracker<T>)value;

            writer.WriteStartObject();

            if (tracker.AddValues.Any())
            {
                writer.WritePropertyName("+");
                serializer.Serialize(writer, tracker.AddValues);
            }

            if (tracker.RemoveValues.Any())
            {
                writer.WritePropertyName("-");
                serializer.Serialize(writer, tracker.RemoveValues);
            }

            writer.WriteEndObject();
        }
    }
}
