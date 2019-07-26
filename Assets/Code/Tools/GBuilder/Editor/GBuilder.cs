using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using UnityEditor.Build.Reporting;
using System.IO;
using System.Text;
using GFramework.Core;

/// <summary>
///   Builder unity相关的函数调用。供给给外部使用
/// </summary>
public class GBuilder
{
    [MenuItem("GBuilder/BuildByConfigure")]
    public static void BuildByConfigure()
    {
        if (!EditorUtility.DisplayDialog("Build", "Are you sure to build by configure?", "Yes", "No"))
        {
            return;
        }
        if (GBuilderConfigure.Configure == null)
        {
            EditorUtility.DisplayDialog("GBuilderConfigure Empty", "Please make GBuilderConfigure Ready", "Yes");
            return;
        }
        if (GBuilderConfigure.Configure != null)
        {
            Build();
        }
    }


    #region 变量
    private static BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
    //打包开始时间
    private static string mBuildBeginTime;
    //导出的app名
    private static string PublishAppName()
    {
        BuildTarget mBuildTarget = GBuilderConfigure.Configure.AppBuildTarget;
        string appName = GBuilderConfigure.Configure.AppName;
        int prefix = GBuilderConfigure.Configure.VersionPrefix;
        int codev = GBuilderConfigure.Configure.CodeVersion;
        int resv = GBuilderConfigure.Configure.ResVersion;
        int svnv = GBuilderConfigure.Configure.SvnVersion;
        return string.Format("{0}_{1}_{2}_{3}_{4}_{5}{6}", appName, prefix, codev, resv, svnv, mBuildBeginTime, PathHelper.GetAppPlatformExt(mBuildTarget));
    }

    //bundle生成目录下的当前资源版本的版本文件路径
    private static string LocalBundleVersionFilePath()
    {
        string bundlePath = GBuilderConfigure.Configure.LocalBundlePath;
        int resv = GBuilderConfigure.Configure.ResVersion;
        string resPath = PathHelper.Combine(bundlePath, resv.ToString());
        string mVersionPath = PathHelper.Combine(resPath, "version.txt");
        return mVersionPath;
    }

    //bundle生成目录下的代码签名文件路径
    private static string LocalBundleCodeSignFilePath()
    {
        string bundlePath = GBuilderConfigure.Configure.LocalBundlePath;
        string mVersionPath = PathHelper.Combine(bundlePath, "codesign.txt");
        return mVersionPath;
    }

    //bundle生成目录下的代码签名文件对比路径
    private static string LocalBundleCodeSignReportPath()
    {
        string bundlePath = GBuilderConfigure.Configure.LocalBundlePath;
        string mVersionPath = PathHelper.Combine(bundlePath, "CodeSignReport");
        return mVersionPath;
    }

    //bundle生成目录下的当前资源版本的assetBundle文件夹路径
    private static string LocalBundleVersionBundleDataPath()
    {
        string bundlePath = GBuilderConfigure.Configure.LocalBundlePath;
        int resv = GBuilderConfigure.Configure.ResVersion;
        string resPath = PathHelper.Combine(bundlePath, resv.ToString());
        string mBundlePath = PathHelper.Combine(resPath, "BundleData");
        return mBundlePath;
    }

    //本地生成的App路径
    private static string LocalBuildAppPath()
    {
        BuildTarget mBuildTarget = GBuilderConfigure.Configure.AppBuildTarget;
        string buildPath = GBuilderConfigure.Configure.BuildPath;
        string appName = GBuilderConfigure.Configure.AppName;
        string appPath = PathHelper.Combine(buildPath, appName, PathHelper.GetAppPlatformExt(mBuildTarget));
        return appPath;
    }

    //当前版本的资源包的发布路径
    private static string VersionResPublishPath()
    {
        string resURL = GBuilderConfigure.Configure.ResServerURL;
        int resv = GBuilderConfigure.Configure.ResVersion;
        string resPublishPath = PathHelper.Combine(resURL, resv.ToString());
        return resPublishPath;
    }

    //当前版本的app的发布路径
    private static string VersionAppPublishPath()
    {
        string appURL = GBuilderConfigure.Configure.AppServerURL;
        string appPublishPath = PathHelper.Combine(appURL, PublishAppName());
        return appPublishPath;
    }

    #endregion
     
    #region 打包流程

    //总入口
    public static void Build()
    { 
        //保留原版本号
        string oldvesion = PlayerSettings.bundleVersion;
        bool ret = BuildBundle();
        if (ret)
        {
            ret = BuildPlayer();
        }
        if (ret)
        {
            //成功写入版本号文件
            WriteVersionFile();
            //保存配置文件
            GBuilderConfigure.Save();
        }
        //发布资源
        PublishPackage(ret);

        //判断编译结果
        RevertVersion(ret, oldvesion);
    }

    //打包bundle
    private static bool BuildBundle()
    {
        try
        {
            //资源打包成功更资源新版本号
            UpdateVersion();
        }
        catch {
        }
        finally {
        }
        return true;
    }

    //更新配置变量的资源版本号
    private static void UpdateVersion()
    {
        GBuilderConfigure.Configure.ParentResVersion = GBuilderConfigure.Configure.ResVersion;
        GBuilderConfigure.Configure.ResVersion += 1;
    }

    //更新代码版本
    private static void UpdateCodeVersion()
    {
        BuildCodeSign(LocalBundleCodeSignFilePath(), LocalBundleCodeSignReportPath(), GBuilderConfigure.Configure.CodeCheckRules, ref GBuilderConfigure.Configure.CodeVersion, GBuilderConfigure.Configure.AppUpdate);
    }

    //打包App
    public static bool BuildPlayer()
    {
        mBuildBeginTime = DateTime.Now.ToString("yyyy_MM_dd_HH_mm");
        BuildTarget mBuildTarget = GBuilderConfigure.Configure.AppBuildTarget;
        try
        {
            if (GBuilderConfigure.Configure.IsValidBuildTarget(mBuildTarget))
            {
                //更新代码codeversion
                UpdateCodeVersion();
                //不带后缀的应用名
                string appName = GBuilderConfigure.Configure.AppName;
                //build生成位置 android是apk路径 ios是导出xcode的目录 windows是exe路径
                string build_location = PathHelper.Combine(GBuilderConfigure.Configure.BuildPath,appName,PathHelper.GetAppPlatformExt(mBuildTarget));
                if (mBuildTarget == BuildTarget.iOS)
                {
                    build_location = PathHelper.Combine(GBuilderConfigure.Configure.BuildPath, "ExportXcodeProject");
                }
                //编译报告路径
                string report_location = PathHelper.Combine(GBuilderConfigure.Configure.ReportPath,"buildreport.txt");
                foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes) scene.enabled = false;
                //设置版本号
                PlayerSettings.bundleVersion = GBuilderConfigure.Configure.PublishVersion;
                //随包场景
                buildPlayerOptions.scenes = GBuilderConfigure.Configure.Scenes.ToArray();
                buildPlayerOptions.locationPathName = build_location;
                buildPlayerOptions.target = mBuildTarget;
                buildPlayerOptions.options = GBuilderConfigure.Configure.Options;
                //是否只生成安装包,不打资源包
                if (GBuilderConfigure.Configure.BuildAppX86) PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7 | AndroidArchitecture.X86;
                //执行build
                BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
                BuildResult ret = report.summary.result;
                //编译成功或失败
                bool mPlayerBuildSuccess = (report != null && ret == BuildResult.Succeeded);
                //编译报告写入
                WriteReport(report, report_location);
                //编译xcode
                CompileXCode(ref mPlayerBuildSuccess);
                return mPlayerBuildSuccess;
            }
            else
            {
                Debug.LogError("Not Support BuildTarget");
                return false;
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message + "\n" + e.StackTrace);
            return false;
        }
    }

    //生成代码md5列表
    private static void BuildCodeSign(string mCodeSignPath,string reportPath,Triple<string, string[], string[]>[] codePathTypes ,ref int codeVersion,bool upadteCodeVer = true)
    {
        var codesigns = new Dictionary<string, string>();
        //找出当前目标平台所有需要检查的代码 
        foreach (var trip in codePathTypes)
        {
            string dir = trip.first;
            string[] dirfilter = trip.second;
            string[] fileTypes = trip.third;
            string[] dirs = FileHelper.GetDirectorys(dir, dirfilter, SearchOption.AllDirectories);
            foreach (var filePath in dirs)
            {
                string[] files = FileHelper.GetFiles(filePath, fileTypes, SearchOption.AllDirectories, false);
                foreach (var file in files)
                {
                    //计算hash 加入字典
                    string hash = HashHelper.HashToString(HashHelper.CalculateMD5(File.ReadAllBytes(file)));
                    codesigns.Add(file, hash);
                }
            }
        }
        //读取当前版本的代码签名文件信息
        var oldcodesigns = new Dictionary<string, string>();
        using (var reader = new StreamReader(mCodeSignPath, false))
        {
            while (reader != null && !reader.EndOfStream)
            {
                string[] sp = reader.ReadLine().Split(new string[] { "===" }, StringSplitOptions.None);
                oldcodesigns.Add(sp[0],sp[1]);
            }
            reader.Close();
        }
        
        //检查代码签名是否一致,不一致更新版本号
        List<string> adds = new List<string>(), removes = new List<string>(), modifies = new List<string>();
        //检查变更或者删除列表
        foreach (var code in oldcodesigns)
        {
            codesigns.TryGetValue(code.Key, out string codesign);
            if (string.IsNullOrEmpty(codesign))
            {
                //代码被删除了
                removes.Add(code.Key);
            }
            else if (codesign != code.Value)
            {
                //代码被修改了
                modifies.Add(code.Key);
            }
        }
        //检查新增列表
        foreach (var code in codesigns) if (!oldcodesigns.ContainsKey(code.Key)) adds.Add(code.Key);
        //发生变化 
        if (adds.Count != 0 || removes.Count != 0 || modifies.Count != 0)
        {
            //生成签名文件
            using (var writer = new StreamWriter(mCodeSignPath, false))
            {
                foreach (var code in codesigns) writer.WriteLine(string.Format("{0}==={1}", code.Value, code.Key));
                writer.Close();
            }
            if (upadteCodeVer)
            {
                if (codesigns.Count != 0)
                {
                    //生成对比文件
                    using (var writer = new StreamWriter(string.Format("{0}/codesign_{1}_to_{2}.txt", reportPath, codeVersion, codeVersion + 1, false)))
                    {
                        foreach (var file in adds) writer.WriteLine(string.Format("a {0}", file));
                        foreach (var file in removes) writer.WriteLine(string.Format("d {0}", file));
                        foreach (var file in modifies) writer.WriteLine(string.Format("m {0}", file));
                        writer.Close();
                    }
                    //修改程序版本号
                    codeVersion += 1;
                }
            }
        } 
    }

    //写入编译报告
    private static void WriteReport(BuildReport report, string filePath)
    {
        if (report != null)
        {
            StringBuilder reportData = new StringBuilder();
            //输出文件列表
            if (report.files != null && report.files.Length != 0)
            {
                reportData.AppendLine("Files");
                foreach (var file in report.files)
                {
                    reportData.AppendFormat("\trole:{0} size:{1} path:{2}\n", file.role, file.size, file.path);
                }
            }
            //阶段信息列表
            if (report.steps != null && report.steps.Length != 0)
            {
                reportData.AppendLine("Steps");
                foreach (var step in report.steps)
                {
                    reportData.AppendFormat("\tname:{0} time:{1}\n", step.name, step.duration.TotalSeconds);
                    if (step.messages != null && step.messages.Length != 0)
                    {
                        var sortedmessages = new List<BuildStepMessage>(step.messages);
                        sortedmessages.Sort((a, b) => { return a.type.CompareTo(b.type); });
                        foreach (var message in sortedmessages)
                        {
                            reportData.AppendFormat("\t\ttype:{0} content:{1}\n", message.type, message.content);
                        }
                    }
                }
            }
            //剥离信息列表
            if (report.strippingInfo != null)
            {
                reportData.AppendLine("Strips");
                foreach (var includeModule in report.strippingInfo.includedModules)
                {
                    reportData.AppendFormat("\t{0}\n", includeModule);
                }
            }

            //总览信息
            reportData.AppendLine("Summary");
            reportData.AppendFormat("\tbuildResult:{0} TotalSize:{1}MB TotalTime:{2} TotalErrors:{3} TotalWarnings:{4}\n",
                                    report.summary.result.ToString(),
                                    (report.summary.totalSize / 1024.0f / 1024.0f).ToString(),
                                    report.summary.totalTime.ToString(),
                                    report.summary.totalErrors.ToString(),
                                    report.summary.totalWarnings.ToString());
            string allReport = reportData.ToString();
            UnityEngine.Debug.Log(allReport); 
            File.WriteAllText(filePath, allReport);
        }
    }
    
    //导出xcode项目 编译生成ipa
    private static void CompileXCode(ref bool mPlayerBuildSuccess)
    {
        BuildTarget mBuildTarget = GBuilderConfigure.Configure.AppBuildTarget;
        if (mPlayerBuildSuccess && mBuildTarget == BuildTarget.iOS)
        {
            if (mBuildTarget == BuildTarget.iOS)
            { 
                //不带后缀的应用名
                string appName = GBuilderConfigure.Configure.AppName;
                string buildPath = GBuilderConfigure.Configure.BuildPath;
                BuildOptions opts = GBuilderConfigure.Configure.Options;
                //调试模式
                bool mBuildDebug = (opts & BuildOptions.Development) == BuildOptions.Development;
                //生成archive文件
                string archiveParams = string.Format("archive -project {1}/ExportXcodeProject/Unity-iPhone.xcodeproj -scheme Unity-iPhone -configuration {0} -archivePath {1}/Unity-iPhone.xcarchive -quiet -UseModernBuildSystem=NO", mBuildDebug ? "Debug" : "Release", buildPath);
                bool archiveRet = CmdHelper.ExcuteProcess("xcodebuild", archiveParams);
                if (!archiveRet) { Debug.LogError("xcodebuild archive Failed!"); return; }
                //导出符号表
                bool dSYMRet = CmdHelper.ExcuteProcess("cp", string.Format("-rf {0}/Unity-iPhone.xcarchive/dSYMs/projectf.app.dSYM {0}", buildPath));
                if (!dSYMRet) { Debug.LogError("cp dSYMRet Failed!"); return; }
                //生成ipa文件
                string ipaParams = string.Format("-exportArchive -archivePath {0}/Unity-iPhone.xcarchive -exportPath {0} -exportOptionsPlist {0}/../SDK/iOS/export_options.plist -quiet -UseModernBuildSystem=NO", buildPath);
                bool ipaRet = CmdHelper.ExcuteProcess("xcodebuild", ipaParams);
                if (!ipaRet) { Debug.LogError("xcodebuild exportArchive Failed!"); return; }
                string oldname = PathHelper.Combine(buildPath,"Unity-iPhone.ipa");
                string newname = PathHelper.Combine(buildPath, appName,PathHelper.GetAppPlatformExt(mBuildTarget));
                FileHelper.Rename(oldname, newname);
            }
        }
    }
   
    //更新Resversion资源文件夹下的版本文件
    private static void WriteVersionFile()
    {
        BuildTarget mBuildTarget = GBuilderConfigure.Configure.AppBuildTarget;
        string pAppName = PublishAppName();
        int resv = GBuilderConfigure.Configure.ResVersion;
        int codev = GBuilderConfigure.Configure.CodeVersion;
        int svnv = GBuilderConfigure.Configure.SvnVersion;

        //更新版本文件
        VersionInfo mVersionData = new VersionInfo();
        mVersionData.appVersion = codev.ToString();
        mVersionData.resVersion = resv.ToString();
        mVersionData.version = GBuilderConfigure.Configure.PublishVersion;
        mVersionData.appName = pAppName;
        mVersionData.appUrl = VersionAppPublishPath();
        mVersionData.resUrl = VersionResPublishPath();
        File.WriteAllText(LocalBundleVersionFilePath(), JsonUtility.ToJson(mVersionData));
    }

    //发布 app和资源包
    private static void PublishPackage(bool mPackageBuildSuccess)
    {
        //上传资源包
        if (mPackageBuildSuccess && GBuilderConfigure.Configure.PublishRes)
        {
            ///scp 命令  -r  递归复制整个目录。  src 可以使一个目录 一个文件 会是 /src/* 目录下全部  dst 对应src 拷贝目录时没有目录路径会创建重命名为目标路径 有目标路径文件夹时拷贝到目标目录下 文件名不变 目标路径是多级不存在的路径会失败 拷贝文件时不会自动创建目录
            //安装包
            CmdHelper.ExcuteProcess("scp", string.Format("-r {0} {1}", LocalBuildAppPath(), VersionAppPublishPath()));
            //资源包
            CmdHelper.ExcuteProcess("scp", string.Format("-r {0} {1}", LocalBundleVersionBundleDataPath(), VersionResPublishPath()));
            //版本文件
            CmdHelper.ExcuteProcess("scp", string.Format("-r {0} {1}", LocalBundleVersionFilePath(), VersionResPublishPath()));
            //签名文件
            CmdHelper.ExcuteProcess("scp", string.Format("-r {0} {1}", LocalBundleCodeSignFilePath(), VersionResPublishPath()));
            //对比结果
           // CmdHelper.ExcuteProcess("scp", string.Format("-r {0} {1}", mComparePath, resPublishPath));
        }
    }

    //失败回滚
    private static void RevertVersion(bool mPlayerBuildSuccess,string oldvesion)
    {
        if (!mPlayerBuildSuccess)
        {
            BuildTarget mBuildTarget = GBuilderConfigure.Configure.AppBuildTarget;
            //判断编译结果
            if (!mPlayerBuildSuccess)
            {
                //打包失败 回滚 版本号
                PlayerSettings.bundleVersion = oldvesion;
                throw new Exception("BuildPlayer Failed");
            }
        }
    }

    #endregion
}
