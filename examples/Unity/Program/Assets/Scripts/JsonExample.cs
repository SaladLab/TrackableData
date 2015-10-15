using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using TrackableData;
using Unity.Data;

namespace Basic
{
    class JsonExample
    {
        private static JsonSerializerSettings JsonSerializerSettings =
            new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.None,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                Converters = new JsonConverter[]
                {
                    new TrackablePocoTrackerJsonConverter<IUserData>(),
                    new TrackableDictionaryTrackerJsonConverter<int, string>(),
                    new TrackableListTrackerJsonConverter<string>(),
                }
            };

        private static void RunTrackablePoco()
        {
            Log.WriteLine("***** TrackablePoco (Json) *****");

            var u = new TrackableUserData();
            u.SetDefaultTracker();

            u.Name = "Bob";
            u.Level = 1;
            u.Gold = 10;

            var json = JsonConvert.SerializeObject(u.Tracker, JsonSerializerSettings);
            Log.WriteLine(json);
            u.Tracker.Clear();

            u.Level += 10;
            u.Gold += 100;

            var json2 = JsonConvert.SerializeObject(u.Tracker, JsonSerializerSettings);
            Log.WriteLine(json2);
            u.Tracker.Clear();

            Log.WriteLine();
        }

        private static void RunTrackableDictionary()
        {
            Log.WriteLine("***** TrackableDictionary (Json) *****");

            var dict = new TrackableDictionary<int, string>();
            dict.SetDefaultTracker();

            dict.Add(1, "One");
            dict.Add(2, "Two");
            dict.Add(3, "Three");

            var json = JsonConvert.SerializeObject(dict.Tracker, JsonSerializerSettings);
            Log.WriteLine(json);
            dict.Tracker.Clear();

            dict.Remove(1);
            dict[2] = "TwoTwo";
            dict.Add(4, "Four");

            var json2 = JsonConvert.SerializeObject(dict.Tracker, JsonSerializerSettings);
            Log.WriteLine(json2);
            dict.Tracker.Clear();

            Log.WriteLine();
        }

        private static void RunTrackableList()
        {
            Log.WriteLine("***** TrackableList (Json) *****");

            var list = new TrackableList<string>();
            list.SetDefaultTracker();

            list.Add("One");
            list.Add("Two");
            list.Add("Three");

            var json = JsonConvert.SerializeObject(list.Tracker, JsonSerializerSettings);
            Log.WriteLine(json);
            list.Tracker.Clear();

            list.RemoveAt(0);
            list[1] = "TwoTwo";
            list.Add("Four");

            var json2 = JsonConvert.SerializeObject(list.Tracker, JsonSerializerSettings);
            Log.WriteLine(json2);
            list.Tracker.Clear();

            Log.WriteLine();
        }

        public static void Run()
        {
            RunTrackablePoco();
            RunTrackableDictionary();
            RunTrackableList();
        }
    }
}
