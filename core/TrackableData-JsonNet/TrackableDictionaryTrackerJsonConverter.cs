using System;
using Newtonsoft.Json;

namespace TrackableData
{
    public class TrackableDictionaryTrackerJsonConverter<TKey, TValue> : JsonConverter
        where TValue : new()
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(TrackableDictionaryTracker<TKey, TValue>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
                                        JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.StartObject)
                return null;

            var tracker = new TrackableDictionaryTracker<TKey, TValue>();
            reader.Read();
            while (true)
            {
                if (reader.TokenType != JsonToken.PropertyName)
                    break;

                var str = (string)reader.Value;
                reader.Read();

                var key = JsonConvert.DeserializeObject<TKey>(str.Substring(1));
                TValue obj;
                switch (str[0])
                {
                    case '+':
                        obj = serializer.Deserialize<TValue>(reader);
                        reader.Read();
                        tracker.TrackAdd(key, obj);
                        break;

                    case '-':
                        reader.Read();
                        tracker.TrackRemove(key, default(TValue));
                        break;

                    case '=':
                        obj = serializer.Deserialize<TValue>(reader);
                        reader.Read();
                        tracker.TrackModify(key, default(TValue), obj);
                        break;
                }
            }

            return tracker;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var tracker = (TrackableDictionaryTracker<TKey, TValue>)value;

            writer.WriteStartObject();

            foreach (var item in tracker.ChangeMap)
            {
                switch (item.Value.Operation)
                {
                    case TrackableDictionaryOperation.Add:
                        writer.WritePropertyName("+" + item.Key);
                        serializer.Serialize(writer, item.Value.NewValue);
                        break;

                    case TrackableDictionaryOperation.Remove:
                        writer.WritePropertyName("-" + item.Key);
                        serializer.Serialize(writer, 0);
                        break;

                    case TrackableDictionaryOperation.Modify:
                        writer.WritePropertyName("=" + item.Key);
                        serializer.Serialize(writer, item.Value.NewValue);
                        break;
                }
            }

            writer.WriteEndObject();
        }
    }
}
