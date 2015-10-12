using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TrackableData
{
    public struct PathAndTrackerJObject
    {
        public string Path;
        public JObject Tracker;
    }

    public static class TrackableJsonExtentions
    {
        public static string SerializeChangedTrackersWithPath(
            this ITrackable trackable,
            JsonSerializerSettings jsonSerializerSettings)
        {
            var pathToChangeMap = trackable.GetChangedTrackersWithPath().ToDictionary(x => x.Key, x => x.Value);
            return JsonConvert.SerializeObject(pathToChangeMap, jsonSerializerSettings);
        }

        public static void ApplyTo(
            this string json,
            ITrackable trackable,
            JsonSerializerSettings jsonSerializerSettings)
        {
            var pathToChangeMap = JsonConvert.DeserializeObject<Dictionary<string, JObject>>(json, jsonSerializerSettings);
            pathToChangeMap.ApplyTo(trackable, jsonSerializerSettings);
        }

        public static void ApplyTo(
            this IEnumerable<KeyValuePair<string, JObject>> pathAndTrackerJObjects,
            ITrackable trackable,
            JsonSerializerSettings jsonSerializerSettings)
        {
            foreach (var item in pathAndTrackerJObjects)
            {
                var targetTrackable = trackable.GetTrackableByPath(item.Key);
                if (targetTrackable != null)
                {
                    var trackerType = TrackerResolver.GetDefaultTracker(targetTrackable.GetType());
                    var tracker = (ITracker)item.Value.ToObject(
                        trackerType, JsonSerializer.Create(jsonSerializerSettings));
                    tracker.ApplyTo(targetTrackable);
                }
            }
        }
    }
}
