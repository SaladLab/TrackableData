using System;
using Newtonsoft.Json;

namespace TrackableData
{
    public class TrackableListTrackerJsonConverter<T> : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(TrackableListTracker<T>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
                                        JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.StartArray)
                return null;

            var tracker = new TrackableListTracker<T>();
            reader.Read();
            while (true)
            {
                if (reader.TokenType != JsonToken.StartArray)
                    break;
                reader.Read();

                var str = (string)reader.Value;
                reader.Read();

                int index;
                if (int.TryParse(str.Substring(1), out index) == false)
                    throw new Exception("Invalid token: " + str);

                T obj;
                switch (str[0])
                {
                    case '+':
                        obj = serializer.Deserialize<T>(reader);
                        reader.Read();
                        tracker.TrackInsert(index, obj);
                        break;

                    case '-':
                        tracker.TrackRemove(index, default(T));
                        break;

                    case '=':
                        obj = serializer.Deserialize<T>(reader);
                        reader.Read();
                        tracker.TrackModify(index, default(T), obj);
                        break;
                }

                if (reader.TokenType != JsonToken.EndArray)
                    throw new Exception("Wrong token type: " + reader.TokenType);

                reader.Read();
            }

            return tracker;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var tracker = (TrackableListTracker<T>)value;

            writer.WriteStartArray();

            foreach (var item in tracker.ChangeList)
            {
                writer.WriteStartArray();

                switch (item.Operation)
                {
                    case TrackableListOperation.Insert:
                        writer.WriteValue("+" + item.Index);
                        serializer.Serialize(writer, item.NewValue);
                        break;

                    case TrackableListOperation.Remove:
                        writer.WriteValue("-" + item.Index);
                        break;

                    case TrackableListOperation.Modify:
                        writer.WriteValue("=" + item.Index);
                        serializer.Serialize(writer, item.NewValue);
                        break;
                }

                writer.WriteEndArray();
            }

            writer.WriteEndArray();
        }
    }
}
