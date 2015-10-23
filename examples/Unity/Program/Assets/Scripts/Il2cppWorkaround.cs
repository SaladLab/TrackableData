using System;
using System.Linq;
using Basic;
using TrackableData;
using Unity.Data;
using UnityEngine;
using UnityEngine.UI;

public static class Il2cppWorkaround
{
    public static void Initialize()
    {
        if (DateTime.UtcNow.Year > 1)
            return;

        if (StubForTracker().Count(o => o == null) > 0)
            throw new Exception("Il2cppWorkaround got an error!");
    }

    private static object[] StubForTracker()
    {
        return new object[]
        {
            new TrackablePocoTracker<IUserData>(),
            new TrackableDictionaryTracker<int, ItemData>(),
            new TrackableList<string>()
        };
    }
}
