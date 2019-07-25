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
public class GBuilderConfigure : BaseConfigure<GBuilderConfigure>
{
    //打包平台
    public BuildTarget AppBuildTarget = BuildTarget.Android;
    //Unity app 路径
    public string UnityPath;
    //xcode 路径
    public string XcodePath;
    //打包工程目录
    public string ProjectPath;
    //资源打包的存放路径
    public string BundlePath;
    //打包工作目录
    public string BuildPath = string.Empty; 
    //编译报告生成路径
    public string ReportPath = string.Empty;
    //应用发布路径
    public string AppReleasePath;
    //res发布路径
    public string ResReleasePath;

    //app下载服务器url
    public string AppServerURL;
    //res下载服务器url
    public string ResServerURL;

    //资源版本
    public int ParentResVersion=-1;
    //资源版本
    public int ResVersion=-1;
    //svn版本
    public int SvnVersion=100;
    //c#代码版本
    public int CodeVersion=-1;
    //发布版本前缀
    public int VersionPrefix = 0;
    //发布版本
    public string PublishVersion
    {
        get
        {
               return string.Format("{0}.{1}.{2}", VersionPrefix, CodeVersion, ResVersion);
        }
    }
    //是否热更新
    public bool HotFix;
    //是否进行大版本更新
    public bool AppUpdate;
    //是否拷贝app res 到发布目录
    public bool PublishRes = true;
    //是否支持X86(关闭可以加快生成速度)
    public bool BuildAppX86 = true;      
    public BuildOptions Options;
    //应用名
    public string AppName = string.Empty;

    public List<string> Scenes =  new List<string>();

    BuildTarget[] supportBuildTargets = new BuildTarget[] { BuildTarget.Android, BuildTarget.iOS,BuildTarget.StandaloneWindows,BuildTarget.StandaloneOSX };

    public override BuildTarget[] SupportBuildTargets()
    {
        return supportBuildTargets;
    }


   
}
