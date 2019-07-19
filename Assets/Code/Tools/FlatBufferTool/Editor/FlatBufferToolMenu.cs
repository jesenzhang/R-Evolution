using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class FlatBufferToolMenu : Editor
{
    [MenuItem("FlatBufferTool/OpenWindow", priority = 200)]
    public static void OpenWindow()
    {
        FlatBufferToolWindow.Window.Show();
    }

}
