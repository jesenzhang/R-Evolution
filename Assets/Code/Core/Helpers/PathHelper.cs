using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class PathHelper
{
    public static string ProjectPath
    {
        get
        {
            return Application.dataPath + "/../";
        }
    }
    public static string ProjectPathAppend(string tail)
    {
        return string.Format("{0}/../{1}", Application.dataPath, tail);
    }
    public static string ProjectPlatformPath(string head, BuildTarget buildTarget, string tail = "")
    {
        return GetPlatformPath(ProjectPathAppend(head), buildTarget, tail);
    }

    public static string GetPlatformPath(string head, BuildTarget buildTarget, string tail = "")
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
            return rootPath;
        }
        return string.Format("{0}/{1}", rootPath, tail);
    }

    private static List<string> GetSceneAssetPaths()
    {
        var scenePaths = AssetDatabase.GetAllAssetPaths().Where(path =>
        {
            Type type = AssetDatabase.GetMainAssetTypeAtPath(path);

            if (type == typeof(SceneAsset))
            {
                return true;
            }

            return false;
        });

        return new List<string>(scenePaths);
    }

    public static string GetAppPlatformExt(BuildTarget buildTarget)
    {
        switch (buildTarget)
        {
            case BuildTarget.Android:
                {
                    return ".apk";
                }
            case BuildTarget.iOS:
                {
                    return ".ipa";
                }
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                {
                    return ".exe";
                }
            default:
                return "";
        }
    }

    public static void ClearPath(string filePath)
    {
        if (Directory.Exists(filePath)) Directory.Delete(filePath, true);
    }

    public static string Combine(params string[] paths)
    {
       return Path.Combine(paths);
    }
}
