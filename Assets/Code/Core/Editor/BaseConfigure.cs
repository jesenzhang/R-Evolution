using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class BaseConfigure<T> where T : BaseConfigure<T>
{
    private static T configure;

    public static T Configure
    {
        get
        {
            if (configure == null)
            {
                configure = RefelctionHelper.CreateNew<T>();
                configure.ReadConfigure(CurrentBuildTarget);
            }
            return configure;
        }
    }
    private static BuildTarget currentTarget = BuildTarget.NoTarget;

    public static BuildTarget CurrentBuildTarget
    {
        get {
            if (Configure.IsValidBuildTarget(currentTarget))
            {
                return currentTarget;
            }
            BuildTarget[] buildTargets = Configure.SupportBuildTargets();
            if (buildTargets != null && buildTargets.Length>=1)
            {
                currentTarget = buildTargets[0];
                return currentTarget;
            }
            return currentTarget;
        }

        set
        {
            if (currentTarget != value)
            {
                if (configure.IsValidBuildTarget(value))
                {
                    currentTarget = value;
                    configure.ReadConfigure(currentTarget);
                }
                else
                    Debug.LogError("Not Valid BuildTarget");
            }
        }
    }

    public static BuildTarget[] DefaultTargets = new BuildTarget[] { BuildTarget.NoTarget };
    
    public virtual BuildTarget[] SupportBuildTargets()
    {
        return DefaultTargets;
    }

    private bool IsValidBuildTarget(BuildTarget buildTarget)
    {
        BuildTarget[] buildTargets = SupportBuildTargets();
        if (buildTargets != null)
        {
            for (int i = 0; i < buildTargets.Length; i++)
            {
                if (buildTargets[i] == buildTarget)
                {
                    return true;
                }
            }
        }
        return false;
    }
    public virtual string ConfigureFileName()
    {
        string classname = typeof(T).Name;
        return classname+".json";
    }

    public static string ConfigureRootPath()
    {
        string classname = typeof(T).Name;
        string rootPath = Application.dataPath + "/../" + classname;
        if (!Directory.Exists(rootPath))
        {
            Directory.CreateDirectory(rootPath);
        }
        return rootPath;
    }

    public string ConfigurePlatformPath(BuildTarget buildTarget)
    {
        if (!IsValidBuildTarget(buildTarget))
        {
            Debug.LogError("buildTarget Is InValid ");
            return string.Empty;
        }
        string rootPath = ConfigureRootPath();
        return PathHelper.GetPlatformPath(rootPath, buildTarget, string.Empty);
    }

    public string ConfigureFilePath(BuildTarget buildTarget)
    {
        string rootPath = ConfigureRootPath();
        return PathHelper.GetPlatformPath(rootPath, buildTarget, ConfigureFileName());
    }
    
    public bool ReadConfigure(BuildTarget buildTarget)
    {
        if (!IsValidBuildTarget(buildTarget))
        {
            Debug.LogError("buildTarget Is InValid ");
            return false;
        }
        string path = ConfigureFilePath(buildTarget);
        if (File.Exists(path))
        {
            string alltext = File.ReadAllText(path);
            try
            {
                configure = JsonUtility.FromJson<T>(alltext);
                return true;
            }
            catch
            {
                Debug.LogError(path +" Configure parse failed! ");
                return false;
            }
        }
        else
        {
            Debug.LogError(path +" Configure Not Exit!");
            configure = RefelctionHelper.CreateNew<T>();
            string jsonstr = JsonUtility.ToJson(configure);
            if (!Directory.Exists(ConfigureRootPath()))
            {
                Directory.CreateDirectory(ConfigureRootPath());
            }
            File.WriteAllText(path, jsonstr);
            Debug.Log(string.Format("Configure created at {0}!", path));
            return false;
        }
    }
    
    public void SaveConfigure(BuildTarget buildTarget)
    {
        if (!IsValidBuildTarget(buildTarget))
        {
            Debug.LogError("buildTarget Is InValid ");
            return;
        }
        string path = ConfigureFilePath(buildTarget);
        if (configure == null)
            configure = RefelctionHelper.CreateNew<T>();
        string resultJson = JsonUtility.ToJson(configure);
        File.WriteAllText(path, resultJson);
        Debug.Log(string.Format("Configure saved at {0}!", path));
    }

    public static void Save()
    {
        Configure.SaveConfigure(CurrentBuildTarget);
    }
}
