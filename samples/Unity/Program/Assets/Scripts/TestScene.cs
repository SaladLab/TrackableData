using Basic;
using UnityEngine;
using UnityEngine.UI;

public class TestScene : MonoBehaviour
{
    public Text LogText;

    void Start()
    {
        Il2cppWorkaround.Initialize();

        LogText.text = "";
        Log.OnLog = OnLog;
        Log.WriteLine("Started!");

        BasicExample.Run();
        JsonExample.Run();
        ProtobufExample.Run();

        Log.WriteLine("**** OK ****");
    }

    void OnLog(string str)
    {
        LogText.text += str;
    }
}
