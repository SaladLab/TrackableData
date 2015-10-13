using System;
using System.Reflection;
using Newtonsoft.Json;

namespace TrackableData
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

                var pi = objectType.GetField((string)reader.Value + "Tracker");
                reader.Read();

                var subTracker = serializer.Deserialize(reader, pi.FieldType);
                reader.Read();

                pi.SetValue(tracker, subTracker);
            }

            return tracker;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var tracker = (ITracker)value;

            writer.WriteStartObject();

            foreach (var fieldType in value.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                if (typeof (ITracker).IsAssignableFrom(fieldType.FieldType) == false)
                    continue;

                var subTracker = (ITracker)fieldType.GetValue(value);
                if (subTracker != null && subTracker.HasChange)
                {
                    writer.WritePropertyName(fieldType.Name.Substring(0, fieldType.Name.Length - 7));
                    serializer.Serialize(writer, subTracker);
                }
            }

            writer.WriteEndObject();
        }
    }
}
