using System;
using System.Reflection;
using Newtonsoft.Json;

namespace TrackableData.Json
{
    public class TrackableContainerTrackerJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(IContainerTracker).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
                                        JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.StartObject)
                return null;

            var tracker = (ITracker)Activator.CreateInstance(objectType);
            reader.Read();
            while (true)
            {
                if (reader.TokenType != JsonToken.PropertyName)
                    break;

                var pi = objectType.GetProperty((string)reader.Value + "Tracker");
                reader.Read();

                var subTracker = serializer.Deserialize(reader, pi.PropertyType);
                reader.Read();

                pi.SetValue(tracker, subTracker);
            }

            return tracker;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var tracker = (ITracker)value;

            writer.WriteStartObject();

            foreach (var pi in value.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (typeof(ITracker).IsAssignableFrom(pi.PropertyType) == false)
                    continue;

                var subTracker = (ITracker)pi.GetValue(value);
                if (subTracker != null && subTracker.HasChange)
                {
                    writer.WritePropertyName(pi.Name.Substring(0, pi.Name.Length - 7));
                    serializer.Serialize(writer, subTracker);
                }
            }

            writer.WriteEndObject();
        }
    }
}
