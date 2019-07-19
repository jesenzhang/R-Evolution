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
    private static BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
    [MenuItem("GBuilder/BuildByConfigure")]
    public static void BuildByConfigure()
    {
        if (!EditorUtility.DisplayDialog("Build", "Are you sure to build by configure?", "Yes", "No"))
        {
            return;
        }
        if (GBuilderConfigure.Configure!=null)
        {
          //  if (JenkinsBuildAssetBundle.BuildAssetBundle())
            {
                BuildAppByConfigure();
            }
        }
    }


    public static void BuildAppByConfigure()
    {
        DateTime begin = DateTime.Now;
        if (GBuilderConfigure.Configure == null)
        {
            EditorUtility.DisplayDialog("GBuilderConfigure Empty", "Please make GBuilderConfigure Ready", "Yes");
            return;
        }
        try
            {
                if (GBuilderConfigure.Configure.buildTarget == BuildTarget.Android || GBuilderConfigure.Configure.buildTarget == BuildTarget.iOS || GBuilderConfigure.Configure.buildTarget == BuildTarget.StandaloneWindows64)
                {
                    string oldvesion = PlayerSettings.bundleVersion;
                    List<string> scenes;
                    string build_location = GBuilderConfigure.Configure.build_location;
                    string report_location = GBuilderConfigure.Configure.report_location;
                    string appName = GBuilderConfigure.Configure.appName;
                    switch (GBuilderConfigure.Configure.buildTarget)
                    {
                        case BuildTarget.Android:
                        {
                            build_location = "Build/unity-android.apk";
                            report_location = "Build/unity_android_report.log";
                            break;
                            }
                        case BuildTarget.iOS:
                        {
                            build_location = "Build/export_xcode";
                            report_location = "Build/unity_ios_report.log";
                            break;
                            }
                        case BuildTarget.StandaloneWindows64:
                        {
                            build_location = "Build/export_windows/game.exe";
                            report_location = "Build/unity_windows_report.log";
                            break;
                            }
                        default:
                            {
                                break;
                            }
                    }

                    //随包场景
                    scenes = new List<string>() { "Assets/Scene/Scene_Start.unity", "Assets/Scene/Scene_Switch.unity" };
                    foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes) scene.enabled = false;

                    GBuilderConfigure.Configure.GenPublishVersion();
                    PlayerSettings.bundleVersion = GBuilderConfigure.Configure.publishVersion;
                    buildPlayerOptions.scenes = scenes.ToArray();
                    buildPlayerOptions.locationPathName = build_location;
                    buildPlayerOptions.target = GBuilderConfigure.Configure.buildTarget;
                    buildPlayerOptions.options = GBuilderConfigure.Configure.options;

                    BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
                    BuildResult ret = report.summary.result;
                    LogBuildReport(report, report_location);
                    if (ret == BuildResult.Succeeded)
                    {
                        JenkinsBuildAssetBundle.SaveConfigure();
                        if (GBuilderConfigure.Configure.publish)
                            JenkinsBuildAssetBundle.CopyResAppToSharePath();
                    }
                    else
                    {
                        PlayerSettings.bundleVersion = oldvesion;
                    //打包失败
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
        DateTime end = DateTime.Now;
        Debug.Log((end.Ticks - begin.Ticks) / 1000f + "s");
    }

    private static void LogBuildReport(BuildReport report, string reportLocation)
    {
        string filePath = reportLocation;
        StreamWriter streamWriter = new StreamWriter(filePath, false, Encoding.UTF8);

        //输出文件列表
        streamWriter.WriteLine("Files");
        for (int i = 0; i < report.files.Length; i++)
        {
            var file = report.files[i];
            streamWriter.WriteLine("\t{0}",file.ToString());
        }
        //阶段信息列表
        streamWriter.WriteLine("Steps");
        for (int i = 0; i < report.steps.Length; i++)
        {
            var step = report.steps[i];
            streamWriter.WriteLine("\t{0}", step.ToString());
        }
        //剥离信息列表
        if(report.strippingInfo != null)
        {
            streamWriter.WriteLine("Strips");
            streamWriter.WriteLine("\t{0}", report.strippingInfo.ToString());
        }

        //总览信息
        streamWriter.WriteLine("Summary");
        streamWriter.WriteLine("\tbuildResult:{0} TotalSize:{1}MB TotalTime:{2} TotalErrors:{3} TotalWarnings:{4}", 
                                report.summary.result.ToString(),
                                (report.summary.totalSize / 1024.0f / 1024.0f).ToString(),
                                report.summary.totalTime.ToString(),
                                report.summary.totalErrors.ToString(),
                                report.summary.totalWarnings.ToString());

        streamWriter.Flush(); streamWriter.Close();
    }
}
