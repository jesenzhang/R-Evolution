﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class GBuilderPlatformConfigure
{
    //打包平台
    public BuildTarget buildTarget = BuildTarget.Android;
}

[System.Serializable]
public class GBuilderConfigure
{
    //打包平台
    public BuildTarget buildTarget = BuildTarget.Android;
    //Unity app 路径
    public string unityPath;
    //xcode 路径
    public string xcodePath;
    //打包工程目录
    public string projectPath;
    //资源打包的存放路径
    public string bundlePath;
    //apk存储路径
    public string packageExportPath;
    //res存储路径
    public string resExportPath;
    //资源版本
    public int parentResVersion=-1;
    //资源版本
    public int resVersion=-1;
    //svn版本
    public int svnVersion=100;
    //c#代码版本
    public int codeVersion=-1;
    //发布版本前缀
    public string versionPrefix = "0";
    //发布版本
    public string publishVersion = "0.0.0";
    //是否热更新
    public bool hotFix;
    //是否进行大版本更新
    public bool appUpdate;
    //app下载根目录
    public string appURL;
    //res下载根目录
    public string resURL;
    //是否拷贝app res 到发布目录
    public bool publish = true;

    public BuildOptions options;

    public string build_location = string.Empty;
    public string report_location = string.Empty;
    public string appName = string.Empty;

    public void GenPublishVersion()
    {
        if(codeVersion!=-1 && resVersion!=-1)
            publishVersion = string.Format("{0}.{1}.{2}", versionPrefix, codeVersion, resVersion);
    }

    private static GBuilderConfigure config;

    private static GBuilderPlatformConfigure platformConfig;

    public static GBuilderConfigure Configure
    {
        get
        {
            if (config == null)
            {
                ReadBuildConfigure();
            }
            return config;
        }
    }

    public static GBuilderPlatformConfigure PlatformConfigure
    {
        get
        {
            if (platformConfig == null)
            {
                ReadPlatformConfig();
            }
            return platformConfig;
        }
    }

    private static string GetConfigureRoot()
    {
        string rootPath = Application.dataPath + "/../BuildConfigure";
        if (!Directory.Exists(rootPath))
        {
            Directory.CreateDirectory(rootPath);
        }
        return rootPath;
    }

    private static string GetBuildConfigurePath()
    {
        string rootPath = GetConfigureRoot();
        if (PlatformConfigure != null)
        {
            BuildTarget buildTarget = PlatformConfigure.buildTarget;
            string targetDir = "";
            if (buildTarget == BuildTarget.iOS)
            {
                targetDir = "iOS";
            }
            else if (buildTarget == BuildTarget.Android)
            {
                targetDir = "Android";
            }
            else
            {
                targetDir = "Windows";
            }
            rootPath = string.Format("{0}/{1}", GetConfigureRoot(), targetDir);
        }
        if (!Directory.Exists(rootPath))
        {
            Directory.CreateDirectory(rootPath);
        }
        return string.Format("{0}/{1}", rootPath, "buildconfigure.json");
    }

    private static string GetPlatformConfigPath()
    {
        return string.Format("{0}/{1}", GetConfigureRoot(), "platformconfig.json");
    }

    public static bool ReadPlatformConfig()
    {
        string path = GetPlatformConfigPath();
        if (File.Exists(path))
        {
            string alltext = File.ReadAllText(path);
            try
            {
                platformConfig = JsonUtility.FromJson<GBuilderPlatformConfigure>(alltext);
                return true;
            }
            catch
            {
                Debug.LogError("GBuilderPlatformConfigure parse failed!");
                return false;
            }
        }
        else
        {
            Debug.LogError("GBuilderPlatformConfigure Not Exit!");
            platformConfig = new GBuilderPlatformConfigure();
            platformConfig.buildTarget = BuildTarget.Android;
            string jsonstr = JsonUtility.ToJson(platformConfig);
            if (!Directory.Exists(GetConfigureRoot()))
            {
                Directory.CreateDirectory(GetConfigureRoot());
            }
            File.WriteAllText(path, jsonstr);
            Debug.Log(string.Format("GBuilderPlatformConfigure created at {0}!", path));
            return false;
        }
    }

    public static bool ReadBuildConfigure()
    {
        string path = GetBuildConfigurePath();
        if (File.Exists(path))
        {
            string alltext = File.ReadAllText(path);
            try
            {
                config = JsonUtility.FromJson<GBuilderConfigure>(alltext);
                return true;
            }
            catch
            {
                Debug.LogError("GBuilderConfigure parse failed!");
                return false;
            }
        }
        else
        {
            Debug.LogError("GBuilderConfigure Not Exit!");
            config = new GBuilderConfigure();
            config.buildTarget = PlatformConfigure.buildTarget;
            string jsonstr = JsonUtility.ToJson(config);
            if (!Directory.Exists(GetConfigureRoot())) {
                Directory.CreateDirectory(GetConfigureRoot());
            }
            File.WriteAllText(path, jsonstr);
            Debug.Log(string.Format("GBuilderConfigure created at {0}!", path));
            return false;
        }
    }
    public static void SavePlatformConfig()
    {
        string path = GetPlatformConfigPath();
        if (platformConfig == null)
            platformConfig = new GBuilderPlatformConfigure();
        platformConfig.buildTarget = config.buildTarget;
        string jsonstr = JsonUtility.ToJson(platformConfig);
        File.WriteAllText(path, jsonstr);
        Debug.Log(string.Format("GBuilderConfigure saved at {0}!", path));
    }

    public static void SaveBuildConfigure()
    {
        string path = GetBuildConfigurePath();
        if (config == null)
            config = new GBuilderConfigure();
        string resultJson = JsonUtility.ToJson(config);
        File.WriteAllText(path, resultJson);
        Debug.Log(string.Format("GBuilderConfigure saved at {0}!", path));
    }

}
