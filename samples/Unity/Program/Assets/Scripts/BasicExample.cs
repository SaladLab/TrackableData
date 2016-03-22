using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TrackableData;
using Unity.Data;

namespace Basic
{
    class BasicExample
    {
        private static void RunTrackablePoco()
        {
            Log.WriteLine("***** TrackablePoco *****");

            var u = new TrackableUserData();
            u.SetDefaultTracker();

            u.Name = "Bob";
            u.Level = 1;
            u.Gold = 10;

            Log.WriteLine(u.Tracker.ToString());
            u.Tracker.Clear();

            u.Level += 10;
            u.Gold += 100;

            Log.WriteLine(u.Tracker.ToString());
            u.Tracker.Clear();

            Log.WriteLine();
        }

        private static void RunTrackableDictionary()
        {
            Log.WriteLine("***** TrackableDictionary *****");

            var dict = new TrackableDictionary<int, string>();
            dict.SetDefaultTracker();

            dict.Add(1, "One");
            dict.Add(2, "Two");
            dict.Add(3, "Three");

            Log.WriteLine(dict.Tracker.ToString());
            dict.Tracker.Clear();

            dict.Remove(1);
            dict[2] = "TwoTwo";
            dict.Add(4, "Four");

            Log.WriteLine(dict.Tracker.ToString());
            dict.Tracker.Clear();

            Log.WriteLine();
        }

        private static void RunTrackableSet()
        {
            Log.WriteLine("***** TrackableSet *****");

            var set = new TrackableSet<int>();
            set.SetDefaultTracker();

            set.Add(1);
            set.Add(2);
            set.Add(3);

            Log.WriteLine(set.Tracker.ToString());
            set.Tracker.Clear();

            set.Remove(1);
            set.Add(4);

            Log.WriteLine(set.Tracker.ToString());
            set.Tracker.Clear();

            Log.WriteLine();
        }

        private static void RunTrackableList()
        {
            Log.WriteLine("***** TrackableList *****");

            var list = new TrackableList<string>();
            list.SetDefaultTracker();

            list.Add("One");
            list.Add("Two");
            list.Add("Three");

            Log.WriteLine(list.Tracker.ToString());
            list.Tracker.Clear();

            list.RemoveAt(0);
            list[1] = "TwoTwo";
            list.Add("Four");

            Log.WriteLine(list.Tracker.ToString());
            list.Tracker.Clear();

            Log.WriteLine();
        }

        public static void Run()
        {
            RunTrackablePoco();
            RunTrackableDictionary();
            RunTrackableSet();
            RunTrackableList();
        }
    }
}
