using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

public class GBuilderConfigureEditor : EditorWindow
{ 
    public static GBuilderConfigureEditor curwindow;
    [MenuItem("GBuilder/Window", false,100)]
    static void ShowEditor()
    {
        curwindow = EditorWindow.GetWindow<GBuilderConfigureEditor>();
        curwindow.titleContent = new GUIContent("打包设置");         // 窗口的标题  
    }
    #region Main Motheds
    private void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("BaseSetting");
        BuildTarget old = GBuilderConfigure.Configure.buildTarget;
        // 设置目标平台
        GBuilderConfigure.Configure.buildTarget = (BuildTarget)EditorGUILayout.EnumPopup("buildTarget(目标平台):", GBuilderConfigure.Configure.buildTarget);
        if (old != GBuilderConfigure.Configure.buildTarget)
        {
            GBuilderConfigure.PlatformConfigure.buildTarget = GBuilderConfigure.Configure.buildTarget;
            GBuilderConfigure.SavePlatformConfig();
            GBuilderConfigure.ReadPlatformConfig();
            GBuilderConfigure.ReadBuildConfigure();

        }
        EditorGUILayout.Separator();

        GUILayout.BeginHorizontal();
        // 设置unity目录
        EditorGUILayout.LabelField("unityPath(Uniy路径):", GUILayout.Width(180));
        GBuilderConfigure.Configure.unityPath = EditorGUILayout.TextField(GBuilderConfigure.Configure.unityPath);
        if (GUILayout.Button("...",GUILayout.Width(30)))
        {
            // 导入
            string path = EditorUtility.OpenFilePanel("choose unityPath", Application.dataPath, "*");
            if (path.Length != 0)
            {
                GBuilderConfigure.Configure.unityPath = path;
            }
        }
        GUILayout.EndHorizontal();
        EditorGUILayout.Separator();
        GUILayout.BeginHorizontal();
        // 设置unity目录
        EditorGUILayout.LabelField("xcodePath(Xcode路径):", GUILayout.Width(180));
        GBuilderConfigure.Configure.unityPath = EditorGUILayout.TextField(GBuilderConfigure.Configure.xcodePath);
        if (GUILayout.Button("...", GUILayout.Width(30)))
        {
            // 导入
            string path = EditorUtility.OpenFilePanel("choose xcodePath", Application.dataPath, "*");
            if (path.Length != 0)
            {
                GBuilderConfigure.Configure.xcodePath = path;
            }
        }
        GUILayout.EndHorizontal();
        EditorGUILayout.Separator();
        GUILayout.BeginHorizontal();
        // projectPath
        EditorGUILayout.LabelField("projectPath(工程目录):", GUILayout.Width(180));
        GBuilderConfigure.Configure.projectPath = EditorGUILayout.TextField(GBuilderConfigure.Configure.projectPath);
        if (GUILayout.Button("...", GUILayout.Width(30)))
        {
            // 导入
            string path = EditorUtility.OpenFolderPanel("choose projectPath", Application.dataPath, "");
            if (path.Length != 0)
            {
                GBuilderConfigure.Configure.projectPath = path;
            }
        }
        GUILayout.EndHorizontal();
        EditorGUILayout.Separator();
        GUILayout.BeginHorizontal();
        // bundlePath
        EditorGUILayout.LabelField("bundlePath(资源的生成目录):",GUILayout.Width(180));
        GBuilderConfigure.Configure.bundlePath = EditorGUILayout.TextField( GBuilderConfigure.Configure.bundlePath);
        if (GUILayout.Button("...", GUILayout.Width(30)))
        {
            // 导入
            string path = EditorUtility.OpenFolderPanel("choose bundlePath", Application.dataPath, "");
            if (path.Length != 0)
            {
                GBuilderConfigure.Configure.bundlePath = path;
            }
        }
        GUILayout.EndHorizontal();
        EditorGUILayout.Separator();
        GUILayout.BeginHorizontal();
        // packageExportPath
        EditorGUILayout.LabelField("packageExportPath(app发布目录):", GUILayout.Width(180));
        GBuilderConfigure.Configure.packageExportPath = EditorGUILayout.TextField(GBuilderConfigure.Configure.packageExportPath);
        if (GUILayout.Button("...", GUILayout.Width(30)))
        {
            // 导入
            string path = EditorUtility.OpenFolderPanel("choose packageExportPath", Application.dataPath, "");
            if (path.Length != 0)
            {
                GBuilderConfigure.Configure.packageExportPath = path;
            }
        }
        GUILayout.EndHorizontal();
        EditorGUILayout.Separator();
        GUILayout.BeginHorizontal();
        // resExportPath
        EditorGUILayout.LabelField("resExportPath(资源发布目录):", GUILayout.Width(180));
        GBuilderConfigure.Configure.resExportPath = EditorGUILayout.TextField(GBuilderConfigure.Configure.resExportPath);
        if (GUILayout.Button("...", GUILayout.Width(30)))
        {
            // 导入
            string path = EditorUtility.OpenFolderPanel("choose resExportPath", Application.dataPath, "");
            if (path.Length != 0)
            {
                GBuilderConfigure.Configure.resExportPath = path;
            }
        }
        GUILayout.EndHorizontal();
        EditorGUILayout.Separator();
        GUILayout.BeginHorizontal();
        if (GBuilderConfigure.Configure.build_location == string.Empty)
        {
            GBuilderConfigure.Configure.build_location = Application.dataPath + "/../Build";
        }
        GBuilderConfigure.Configure.build_location = EditorGUILayout.TextField("build_location(app生成位置):", GBuilderConfigure.Configure.build_location);
        if (GUILayout.Button("...", GUILayout.Width(30)))
        {
            string path = EditorUtility.OpenFolderPanel("choose build_location", Application.dataPath, "");
            if (path.Length != 0)
            {
                GBuilderConfigure.Configure.build_location = path;
            }
        }
        GUILayout.EndHorizontal();
        EditorGUILayout.Separator();
        GUILayout.BeginHorizontal();
        if (GBuilderConfigure.Configure.report_location == string.Empty)
        {
            GBuilderConfigure.Configure.report_location = Application.dataPath + "/../Build";
        }
        GBuilderConfigure.Configure.build_location = EditorGUILayout.TextField("build_location(app生成位置):", GBuilderConfigure.Configure.build_location);
        if (GUILayout.Button("...", GUILayout.Width(30)))
        {
            string path = EditorUtility.OpenFolderPanel("choose build_location", Application.dataPath, "");
            if (path.Length != 0)
            {
                GBuilderConfigure.Configure.build_location = path;
            }
        }
        GUILayout.EndHorizontal();
        EditorGUILayout.Separator();

        if (GBuilderConfigure.Configure.appName == string.Empty)
        {
            GBuilderConfigure.Configure.appName = "game";
        }
        GBuilderConfigure.Configure.appName = EditorGUILayout.TextField("appName(程序名称)", GBuilderConfigure.Configure.appName);


        // publishVersion
        GBuilderConfigure.Configure.publishVersion = EditorGUILayout.TextField("publishVersion(发布版本号)", GBuilderConfigure.Configure.publishVersion);
        EditorGUILayout.Separator();
        // versionPrefix
        GBuilderConfigure.Configure.versionPrefix = EditorGUILayout.TextField("versionPrefix(版本号前缀)", GBuilderConfigure.Configure.versionPrefix);

        EditorGUILayout.Separator();
        // resVersion
        GBuilderConfigure.Configure.parentResVersion = EditorGUILayout.IntField("parentResVersion(上次打包资源版本):", GBuilderConfigure.Configure.parentResVersion);
        EditorGUILayout.Separator();
        // resVersion
        GBuilderConfigure.Configure.resVersion = EditorGUILayout.IntField("resVersion(资源版本):", GBuilderConfigure.Configure.resVersion);
        EditorGUILayout.Separator();
        // svnVersion
        GBuilderConfigure.Configure.svnVersion = EditorGUILayout.IntField("svnVersion:", GBuilderConfigure.Configure.svnVersion);
        EditorGUILayout.Separator();
        // codeVersion
        GBuilderConfigure.Configure.codeVersion = EditorGUILayout.IntField("codeVersion(代码版本):", GBuilderConfigure.Configure.codeVersion);
        EditorGUILayout.Separator();
        // appURL
        GBuilderConfigure.Configure.appURL = EditorGUILayout.TextField("appURL(app更新链接):", GBuilderConfigure.Configure.appURL);
        EditorGUILayout.Separator();
        // resURL
        GBuilderConfigure.Configure.resURL = EditorGUILayout.TextField("resURL(资源热更目录):", GBuilderConfigure.Configure.resURL);
        EditorGUILayout.Separator();
        // hotFix
        bool hotfix = EditorGUILayout.ToggleLeft("hotFix(是否热更新)", GBuilderConfigure.Configure.hotFix);
        if (GBuilderConfigure.Configure.hotFix != hotfix)
        {
            GBuilderConfigure.Configure.hotFix = hotfix;
            BuildTargetGroup buildTargetGroup = GBuilderConfigure.Configure.buildTarget == BuildTarget.Android ? BuildTargetGroup.Android : GBuilderConfigure.Configure.buildTarget == BuildTarget.iOS ? BuildTargetGroup.iOS : BuildTargetGroup.Standalone;
            string oldDefineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup).Trim();
            int hotfixindex = oldDefineSymbols.IndexOf("HOTFIX");
            if (hotfixindex >= 0 && GBuilderConfigure.Configure.hotFix == false)
            {
                string newDefineSymbols = "";
                if (oldDefineSymbols.IndexOf(";HOTFIX;") >= 0)
                {
                    newDefineSymbols = oldDefineSymbols.Replace(";HOTFIX;", ";");
                }
                else if (oldDefineSymbols.IndexOf(";HOTFIX") >= 0)
                {
                    newDefineSymbols = oldDefineSymbols.Replace(";HOTFIX", "");
                }
                else if (oldDefineSymbols.IndexOf("HOTFIX;") >= 0)
                {
                    newDefineSymbols = oldDefineSymbols.Replace("HOTFIX;", "");
                };
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, newDefineSymbols);
            }
            else if (hotfixindex < 0 && GBuilderConfigure.Configure.hotFix)
            {
                string newDefineSymbols = oldDefineSymbols + ";HOTFIX";
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, newDefineSymbols);
            }
        }

        EditorGUILayout.Separator();
        // appUpdate
        GBuilderConfigure.Configure.appUpdate = EditorGUILayout.ToggleLeft("appUpdate(是否更新大版本)",GBuilderConfigure.Configure.appUpdate);
        EditorGUILayout.Separator();
        // publish
        GBuilderConfigure.Configure.publish = EditorGUILayout.ToggleLeft("publish(拷贝资源到发布目录)", GBuilderConfigure.Configure.publish);
        EditorGUILayout.Separator();

        GBuilderConfigure.Configure.options = (BuildOptions)EditorGUILayout.MaskField(new GUIContent("Build Options(打包app选项)"), (int)GBuilderConfigure.Configure.options, Enum.GetNames(typeof(BuildOptions)));
      

        EditorGUILayout.Separator();

        if (GUILayout.Button("Save"))
        {
            GBuilderConfigure.SaveBuildConfigure();
            GBuilderConfigure.SavePlatformConfig();
        }

        if (GUILayout.Button("Build Asset Only"))
        {
            GBuilderConfigure.SaveBuildConfigure();
            GBuilderConfigure.SavePlatformConfig();

            if (!EditorUtility.DisplayDialog("Build Asset Only", "Are you sure to build asset only ?", "Yes", "No"))
            {
                return;
            }
            JenkinsBuildAssetBundle.BuildAssetBundle();
            GBuilderConfigure.SaveBuildConfigure();
            GBuilderConfigure.SavePlatformConfig();
        }

        if (GUILayout.Button("Build App Only"))
        {
            GBuilderConfigure.SaveBuildConfigure();
            GBuilderConfigure.SavePlatformConfig();
            if (!EditorUtility.DisplayDialog("Build App Only", "Are you sure to build App only ?", "Yes", "No"))
            {
                return;
            }
            GBuilder.BuildAppByConfigure();
        }

        if (GUILayout.Button("Build Asset And App"))
        {
            GBuilderConfigure.SaveBuildConfigure();
            GBuilderConfigure.SavePlatformConfig();
            if (!EditorUtility.DisplayDialog("Build  Asset And App", "Are you sure to build asset and app?", "Yes", "No"))
            {
                return;
            }
            GBuilder.BuildByConfigure();
        }

        EditorGUILayout.EndVertical();
    }

    private void OnDestroy()
    {

    }
    #endregion

}
