using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class PathHelper
{
    public static string GetPlatformPath(string head,BuildTarget buildTarget,string tail)
    {
        string rootPath = head;
        if (buildTarget != BuildTarget.NoTarget)
        {
            string targetDir = Enum.GetName(typeof(BuildTarget), buildTarget);
            rootPath = string.Format("{0}/{1}", head, targetDir);
        }
        if (!Directory.Exists(rootPath))
        {
            Directory.CreateDirectory(rootPath);
        }
        if (tail == string.Empty || tail == "")
        {
            return  rootPath;
        }
        return string.Format("{0}/{1}", rootPath, tail);
    }
}
