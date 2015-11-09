using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TrackableData;

namespace Basic
{
    class BasicExample
    {
        private static void RunTrackablePoco()
        {
            Console.WriteLine("***** TrackablePoco *****");

            var u = new TrackableUserData();
            u.SetDefaultTracker();

            u.Name = "Bob";
            u.Level = 1;
            u.Gold = 10;

            Console.WriteLine(u.Tracker);
            u.Tracker.Clear();

            u.Level += 10;
            u.Gold += 100;

            Console.WriteLine(u.Tracker);
            u.Tracker.Clear();

            Console.WriteLine();
        }

        private static void RunTrackableDictionary()
        {
            Console.WriteLine("***** TrackableDictionary *****");

            var dict = new TrackableDictionary<int, string>();
            dict.SetDefaultTracker();

            dict.Add(1, "One");
            dict.Add(2, "Two");
            dict.Add(3, "Three");

            Console.WriteLine(dict.Tracker);
            dict.Tracker.Clear();

            dict.Remove(1);
            dict[2] = "TwoTwo";
            dict.Add(4, "Four");

            Console.WriteLine(dict.Tracker);
            dict.Tracker.Clear();

            Console.WriteLine();
        }

        private static void RunTrackableList()
        {
            Console.WriteLine("***** TrackableList *****");

            var list = new TrackableList<string>();
            list.SetDefaultTracker();

            list.Add("One");
            list.Add("Two");
            list.Add("Three");

            Console.WriteLine(list.Tracker);
            list.Tracker.Clear();

            list.RemoveAt(0);
            list[1] = "TwoTwo";
            list.Add("Four");

            Console.WriteLine(list.Tracker);
            list.Tracker.Clear();

            Console.WriteLine();
        }

        public static void Run()
        {
            RunTrackablePoco();
            RunTrackableDictionary();
            RunTrackableList();
        }
    }
}
