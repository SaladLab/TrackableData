using System;
using UnityEngine;
using UnityEngine.UI;

public static class Log
{
    public static Action<string> OnLog;

    public static void Write(string str)
    {
        if (OnLog != null)
            OnLog(str);
    }

    public static void WriteLine()
    {
        WriteLine("");
    }

    public static void WriteLine(string str)
    {
        if (OnLog != null)
            OnLog(str + "\n");
    }
}
