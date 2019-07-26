using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

public class GBuilderConfigureEditor : EditorWindow
{
    int titleWidth = 120;
    List<string> tempStringList = new List<string>();
    Vector2 sceneListPos;


    public static GBuilderConfigureEditor curwindow;
    [MenuItem("GBuilder/Window", false,100)]
    static void ShowEditor()
    {
        curwindow = EditorWindow.GetWindow<GBuilderConfigureEditor>();
        curwindow.titleContent = new GUIContent("GBuilder");         // 窗口的标题  
    }
    private void OnEnable()
    {
        tempStringList = new List<string>();
        GUIHelper.CleanCache();
    }

    private void OnDisable()
    {
        tempStringList = null;
        GBuilderConfigure.Save();
    }

    #region Main Motheds
    private void OnGUI()
    {
        Event uievent = Event.current;
        EditorGUILayout.BeginVertical();
        // 设置目标平台
        GBuilderConfigure.CurrentBuildTarget = (BuildTarget)GUIHelper.DrawEnumPopup("BuildTarget:", GBuilderConfigure.CurrentBuildTarget, titleWidth);
        if(GBuilderConfigure.CurrentBuildTarget != GBuilderConfigure.Configure.AppBuildTarget)
            GBuilderConfigure.Configure.AppBuildTarget = GBuilderConfigure.CurrentBuildTarget;

        if (GUIHelper.DrawFold("PathSetting", "PathSetting"))
        {
            EditorGUILayout.Separator();
            // 设置unity目录
            GUIHelper.DrawFilePick("UnityPath:", ref GBuilderConfigure.Configure.UnityPath, "", titleWidth, "Uniy路径");
            EditorGUILayout.Separator();
            // 设置unity目录
            GUIHelper.DrawFilePick("XcodePath:", ref GBuilderConfigure.Configure.XcodePath, "", titleWidth, "Xcode路径");
            EditorGUILayout.Separator();
            GUIHelper.DrawFolderPick("ProjectPath:", ref GBuilderConfigure.Configure.ProjectPath,Application.dataPath + "/../", titleWidth, "工程目录");
            EditorGUILayout.Separator();
            GUIHelper.DrawFolderPick("LocalBundlePath:", ref GBuilderConfigure.Configure.LocalBundlePath,PathHelper.ProjectPlatformPath("AssetBundleData", GBuilderConfigure.CurrentBuildTarget),titleWidth, "资源的生成目录");
            EditorGUILayout.Separator();
            GUIHelper.DrawFolderPick("BuildPath:", ref GBuilderConfigure.Configure.BuildPath, PathHelper.ProjectPlatformPath("Build", GBuilderConfigure.CurrentBuildTarget), titleWidth, "App生成生成目录");
            EditorGUILayout.Separator();
            GUIHelper.DrawFolderPick("ReportPath:", ref GBuilderConfigure.Configure.ReportPath, PathHelper.ProjectPlatformPath("BuildReport", GBuilderConfigure.CurrentBuildTarget), titleWidth, "编译报告生成目录");
            EditorGUILayout.Separator();
            GUIHelper.DrawFolderPick("AppReleasePath:", ref GBuilderConfigure.Configure.AppReleasePath, PathHelper.ProjectPlatformPath("AppRelease", GBuilderConfigure.CurrentBuildTarget),titleWidth, "应用发布目录");
            EditorGUILayout.Separator();
            GUIHelper.DrawFolderPick("ResReleasePath:", ref GBuilderConfigure.Configure.ResReleasePath, PathHelper.ProjectPlatformPath("ResRelease", GBuilderConfigure.CurrentBuildTarget), titleWidth, "资源发布目录");
            EditorGUILayout.Separator();
        }
        if (GUIHelper.DrawFold("ConfigureSetting", "ConfigureSetting"))
        {
            GUIHelper.DrawTextField("AppName", ref GBuilderConfigure.Configure.AppName,"game",80, "程序名称");
            EditorGUILayout.Separator();
            GUIHelper.DrawIntField("ParentResVersion", ref GBuilderConfigure.Configure.ParentResVersion, 80, "上次打包资源版本");
            EditorGUILayout.Separator();
            GUIHelper.DrawIntField("ResVersion", ref GBuilderConfigure.Configure.ResVersion, 80, "资源版本");
            EditorGUILayout.Separator();
            GUIHelper.DrawIntField("SvnVersion", ref GBuilderConfigure.Configure.SvnVersion, 80, "Svn版本");
            EditorGUILayout.Separator();
            GUIHelper.DrawIntField("CodeVersion", ref GBuilderConfigure.Configure.CodeVersion, 80, "代码版本");
            EditorGUILayout.Separator();
            GUIHelper.DrawIntField("VersionPrefix", ref GBuilderConfigure.Configure.VersionPrefix, 80, "版本前缀 大版本号");
            EditorGUILayout.Separator();
            GUIHelper.DrawLabel("PublishVersion", GBuilderConfigure.Configure.PublishVersion);
            EditorGUILayout.Separator();
            GUIHelper.DrawTextField("ResServerURL", ref GBuilderConfigure.Configure.ResServerURL, "", 80, "资源下载服务器地址");
            EditorGUILayout.Separator();
            GUIHelper.DrawTextField("AppServerURL", ref GBuilderConfigure.Configure.AppServerURL, "", 80, "App下载服务器地址");
        }

        // hotFix
        EditorGUILayout.Separator();
        GUIHelper.DrawToggle("HotFix",ref GBuilderConfigure.Configure.HotFix,(fix)=> {
            BuildTargetGroup buildTargetGroup = GBuilderConfigure.Configure.AppBuildTarget == BuildTarget.Android ? BuildTargetGroup.Android : GBuilderConfigure.Configure.AppBuildTarget == BuildTarget.iOS ? BuildTargetGroup.iOS : BuildTargetGroup.Standalone;
            string oldDefineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup).Trim();
            int hotfixindex = oldDefineSymbols.IndexOf("HOTFIX");
            if (hotfixindex >= 0 && GBuilderConfigure.Configure.HotFix == false)
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
            else if (hotfixindex < 0 && GBuilderConfigure.Configure.HotFix)
            {
                string newDefineSymbols = oldDefineSymbols + ";HOTFIX";
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, newDefineSymbols);
            }
        },80,"是否开启热更新");
        EditorGUILayout.Separator();
        // appUpdate
        GUIHelper.DrawToggle("AppUpdate", ref GBuilderConfigure.Configure.AppUpdate, null, 80, "是否更新大版本");
        EditorGUILayout.Separator();
        GUIHelper.DrawToggle("BuildAppX86", ref GBuilderConfigure.Configure.BuildAppX86, null, 80, "是否支持X86(关闭可以加快生成速度)");
        EditorGUILayout.Separator();
        // publish
        GUIHelper.DrawToggle("PublishRes", ref GBuilderConfigure.Configure.PublishRes, null, 80, "是否拷贝资源到发布目录");

        EditorGUILayout.Separator();
        
        if (GUIHelper.DrawFold("ScenesInBuild", "Scenes In Build",80,"打包场景",true))
        {
            sceneListPos = GUILayout.BeginScrollView(sceneListPos,GUILayout.MaxHeight(200));
            EditorGUILayout.BeginHorizontal();
            GUIHelper.DrawToolbar("ShowScenesBar",new string[] {"HideAllScenes","ShowAllScenes" },0, (index) => {
                if (index == 1)
                {
                    AddAllScenes(ref tempStringList);
                }
                if (index == 0)
                {
                    tempStringList.Clear();
                }
            });
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginVertical(GUILayout.MinHeight(100), GUILayout.MaxHeight(200));
            if (GBuilderConfigure.Configure.Scenes.Count == 0 && tempStringList.Count==0)
            {
                GUILayout.FlexibleSpace();
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label("Drag and drop scene assets/folders here");
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.FlexibleSpace();
            }
            else
            {
                for (int i = 0; i < GBuilderConfigure.Configure.Scenes.Count; i++)
                {
                    GUIHelper.DrawToggleLeft(GBuilderConfigure.Configure.Scenes[i], GBuilderConfigure.Configure.Scenes[i],true,(check) =>
                    {
                        if (check == false)
                        {
                            GBuilderConfigure.Configure.Scenes.RemoveAt(i);
                        }
                    });
                }
                for (int i = 0; i < tempStringList.Count; i++)
                {
                    if (!GBuilderConfigure.Configure.Scenes.Contains(tempStringList[i]))
                    {
                      GUIHelper.DrawToggleLeft(tempStringList[i], tempStringList[i],false, (check) =>
                      {
                          if (check == true)
                          {
                              GBuilderConfigure.Configure.Scenes.Add(tempStringList[i]);
                          }
                      });
                    }
                }
            }
            EditorGUILayout.EndVertical();
            if (GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
            {
                if (uievent.type == EventType.DragUpdated || uievent.type == EventType.DragPerform)
                {
                    if (DragAndDrop.paths.Length > 0)
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                    }
                }
                if (uievent.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    AddScenes(ref GBuilderConfigure.Configure.Scenes, DragAndDrop.paths);
                    GBuilderConfigure.Save();
                    Repaint();
                }
            }
            GUILayout.EndScrollView();
        }

        EditorGUILayout.Separator();
        GBuilderConfigure.Configure.Options = (BuildOptions)GUIHelper.DrawMaskField("Build Options:",(int)GBuilderConfigure.Configure.Options, typeof(BuildOptions));

        EditorGUILayout.Separator();

        if (GUILayout.Button("Save"))
        {
            GBuilderConfigure.Save();
        }

        if (GUILayout.Button("Build Asset Only"))
        {
            GBuilderConfigure.Save();

            if (!EditorUtility.DisplayDialog("Build Asset Only", "Are you sure to build asset only ?", "Yes", "No"))
            {
                return;
            }
            
            GBuilderConfigure.Save();
        }

        if (GUILayout.Button("Build App Only"))
        {
            GBuilderConfigure.Save();
            if (!EditorUtility.DisplayDialog("Build App Only", "Are you sure to build App only ?", "Yes", "No"))
            {
                return;
            }
            GBuilder.BuildPlayer();
        }

        if (GUILayout.Button("Build Asset And App"))
        {
            GBuilderConfigure.Save();
            if (!EditorUtility.DisplayDialog("Build  Asset And App", "Are you sure to build asset and app?", "Yes", "No"))
            {
                return;
            }
            GBuilder.BuildByConfigure();
        }

        EditorGUILayout.EndVertical();
    }

    private void AddAllScenes(ref List<string> list)
    {
        list.Clear();
       string[] paths = FileHelper.FindAssets("t:Scene");
        AddScenes(ref list, paths);
    }

    private void AddScenes(ref List<string> sceneSet_, string[] paths)
    {
        for (int i = 0; i < paths.Length; ++i)
        {
            string path = paths[i].Replace('\\', '/');

            if (File.Exists(path))
            {
                Type type = AssetDatabase.GetMainAssetTypeAtPath(path);

                if (type == typeof(SceneAsset))
                {
                   // string spath = AssetDatabase.AssetPathToGUID(path);
                    if (!sceneSet_.Contains(path))
                    {
                        sceneSet_.Add(path);
                        GUIHelper.RemoveCacheBoolState(path);
                    }
                }
            }
            else if (Directory.Exists(path))
            {
                AddScenes(ref sceneSet_,Directory.GetFiles(path, "*.*", SearchOption.AllDirectories));
            }
        }
    }

    private void OnDestroy()
    {

    }
    #endregion

}
