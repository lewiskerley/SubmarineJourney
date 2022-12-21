using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class BuildLogger : MonoBehaviour
{
    PlayerControls _playerControls;
    string myLog = "*begin log";
    string filename = "";
    bool doShow = false;
    int kLines = 12;
    void OnEnable() { Application.logMessageReceived += Log; _playerControls.Enable(); }
    void OnDisable() { Application.logMessageReceived -= Log; _playerControls.Disable(); }
    private void Awake()
    {
        _playerControls = new PlayerControls();
        _playerControls.Map.Console.performed += ToggleLog;
    }
    private void ToggleLog(InputAction.CallbackContext context) { doShow = !doShow; }
    public void Log(string logString, string stackTrace, LogType type)
    {
        // for onscreen...
        myLog = myLog + "\n" + logString;

        if (myLog.Split('\n').Length > kLines) {
            myLog = logString;
        }

        /*
        // for the file ...
        if (filename == "")
        {
            string d = System.Environment.GetFolderPath(
               System.Environment.SpecialFolder.Desktop) + "/YOUR_LOGS";
            System.IO.Directory.CreateDirectory(d);
            string r = Random.Range(1000, 9999).ToString();
            filename = d + "/log-" + r + ".txt";
        }
        try { System.IO.File.AppendAllText(filename, logString + "\n"); }
        catch { }*/
    }

    void OnGUI()
    {
        if (!doShow) { return; }
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity,
           new Vector3(Screen.width / 1200.0f, Screen.height / 800.0f, 1.0f));
        GUI.TextArea(new Rect(10, 150, 540, 370), myLog);
    }
}