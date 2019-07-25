using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using UnityEditor.Build.Reporting;
using System.IO;
using System.Text;

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
            //  if (JenkinsBuildAssetBundle.BuildAssetBundle())
            {
                BuildPlayer();
            }
        }
    }



    private static BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
    //打包开始时间
    private static string mBuildBeginTime;


    public static void BuildPlayer()
    {
        mBuildBeginTime = DateTime.Now.ToString("yyyy_MM_dd_HH_mm");
        BuildTarget mBuildTarget = GBuilderConfigure.Configure.AppBuildTarget;
        try
        {
            if (GBuilderConfigure.Configure.IsValidBuildTarget(mBuildTarget))
            {
                //保留原版本号
                string oldvesion = PlayerSettings.bundleVersion;
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
                bool mPlayerBuildSuccess = report != null && ret == BuildResult.Succeeded;
                //编译报告写入
                WriteReport(report, report_location);
                //判断编译结果
                if (mPlayerBuildSuccess)
                {
                    CompileXCode(mPlayerBuildSuccess,mBuildTarget,appName);
                    JenkinsBuildAssetBundle.SaveConfigure();
                    if (GBuilderConfigure.Configure.PublishRes)
                        JenkinsBuildAssetBundle.CopyResAppToSharePath();
                }
                else
                { 
                    //打包失败 回滚 版本号
                    PlayerSettings.bundleVersion = oldvesion;
                    throw new Exception("BuildPlayer Failed");
                }
            }
            else
            {
                Debug.LogError("Not Support BuildTarget");
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message + "\n" + e.StackTrace);
        }
    }

    private static void BuildCodeSign(string mCodeSignPath, string[] codePaths)
    {      
        //找出当前目标平台所有需要检查的代码
        string[] dirfilter = new string[] { "/Editor/",".."};
        string[] fileTypes = new string[] { ".cs", ".dll"};
        var codesigns = new Dictionary<string, string>();
        foreach (var dir in codePaths)
        {
            string[] dirs = FileHelper.GetDirectorys(dir, dirfilter);
            foreach (var filePath in dirs)
            {
                string[] files  = FileHelper.GetFiles(filePath, fileTypes);
                foreach (var file in files)
                {
                    //计算hash 加入字典
                    string hash =  HashHelper.HashToString(HashHelper.CalculateMD5(File.ReadAllBytes(file)));
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
                oldcodesigns.Add(sp[1], sp[0]);
            }
            reader.Close();
        }
        
        //检查代码签名是否一致,不一致更新版本号
        List<string> adds = new List<string>(), removes = new List<string>(), modifies = new List<string>();
        //检查变更或者删除列表
        foreach (var code in codesigns)
        {
            string codesign = null;
            commoncodes.TryGetValue(code.Key, out codesign);
            if (string.IsNullOrEmpty(codesign)) platformcodes.TryGetValue(code.Key, out codesign);
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
        foreach (var code in commoncodes) if (!codesigns.ContainsKey(code.Key)) adds.Add(code.Key);
        foreach (var code in platformcodes) if (!codesigns.ContainsKey(code.Key)) adds.Add(code.Key);

        if (adds.Count != 0 || removes.Count != 0 || modifies.Count != 0)
        {
            //生成签名文件
            using (var writer = new StreamWriter(mCodeSignPath, false))
            {
                foreach (var code in commoncodes) writer.WriteLine(string.Format("{0}==={1}", code.Value, code.Key));
                foreach (var code in platformcodes) writer.WriteLine(string.Format("{0}==={1}", code.Value, code.Key));
                writer.Close();
            }
            if (codesigns.Count != 0)
            {
                //生成对比文件
                using (var writer = new StreamWriter(string.Format("{0}/codesign_{1}_to_{2}.txt", mComparePath, mAppVersion, mAppVersion + 1, false)))
                {
                    foreach (var file in adds) writer.WriteLine(string.Format("a {0}", file));
                    foreach (var file in removes) writer.WriteLine(string.Format("d {0}", file));
                    foreach (var file in modifies) writer.WriteLine(string.Format("m {0}", file));
                    writer.Close();
                }
                //修改程序版本号
                mAppVersion += 1;
            }
        }
    }
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
    private static void CompileXCode(bool mPlayerBuildSuccess,BuildTarget mBuildTarget,string appName)
    {
        if (mPlayerBuildSuccess && mBuildTarget == BuildTarget.iOS)
        {
            if (mBuildTarget == BuildTarget.iOS)
            {
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

    //导出的app名
    private static string PublishAppName(BuildTarget mBuildTarget)
    {
        string appName = GBuilderConfigure.Configure.AppName;
        int prefix = GBuilderConfigure.Configure.VersionPrefix;
        int codev = GBuilderConfigure.Configure.CodeVersion;
        int resv = GBuilderConfigure.Configure.ResVersion;
        int svnv = GBuilderConfigure.Configure.SvnVersion;
        return string.Format("{0}_{1}_{2}_{3}_{4}_{5}{6}", appName, prefix, codev, resv, svnv, mBuildBeginTime,PathHelper.GetAppPlatformExt(mBuildTarget));
    }
    
    private static void UploadPackage(bool mPackageBuildSuccess, BuildTarget mBuildTarget)
    {
        string buildPath = GBuilderConfigure.Configure.BuildPath;
        string appURL = GBuilderConfigure.Configure.AppServerURL;
        string resURL = GBuilderConfigure.Configure.ResServerURL;
        string appName = GBuilderConfigure.Configure.AppName;
        string appPath = PathHelper.Combine(buildPath, appName, PathHelper.GetAppPlatformExt(mBuildTarget));
        string appPublishPath = PathHelper.Combine(appURL, PublishAppName(mBuildTarget));
        string bundlePath = GBuilderConfigure.Configure.BundlePath;
        int resv = GBuilderConfigure.Configure.ResVersion;
        string resPath = PathHelper.Combine(bundlePath, resv.ToString());
        string resPublishPath = PathHelper.Combine(resURL, resv.ToString());
        string mVersionPath = PathHelper.Combine(resPath, "version.txt");
        string mCodeSignPath = PathHelper.Combine(resPath, "codesign.txt");
        //上传资源包
        if (mPackageBuildSuccess && GBuilderConfigure.Configure.PublishRes)
        {
            //安装包
            CmdHelper.ExcuteProcess("scp", string.Format("-r {0} {1}", appPath, appPublishPath));
            //资源包
            CmdHelper.ExcuteProcess("scp", string.Format("-r {0} {1}", resPath, resPublishPath));
            //版本文件
            CmdHelper.ExcuteProcess("scp", string.Format("-r {0} {1}", mVersionPath, resPublishPath));
            //签名文件
            CmdHelper.ExcuteProcess("scp", string.Format("-r {0} {1}", mCodeSignPath, resPublishPath));
            //对比结果
           // CmdHelper.ExcuteProcess("scp", string.Format("-r {0} {1}", mComparePath, resPublishPath));
        }
    }

    private static void RevertVersion(bool mPackageBuildSuccess, BuildTarget mBuildTarget)
    {
        if (!mPackageBuildSuccess)
        {
        }
    }
}
