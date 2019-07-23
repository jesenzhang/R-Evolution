using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace ShaderVariantsStripper
{
    public class ShaderCompileReportWindow : EditorWindow
    {
        private static ShaderCompileReportWindow m_window;
        public static ShaderCompileReportWindow Window
        {
            get
            {
                if (m_window == null)
                {
                    m_window = EditorWindow.GetWindow<ShaderCompileReportWindow>("ShaderCompileReportWindow");
                    m_window.minSize = new Vector2(900, 600);
                }
                return m_window;
            }
        }
        [MenuItem("ShaderVariantsStripper/OpenReportWindow", priority = 400)]
        public static void OpenWindow()
        {
            Window.Show();
        }


        ShaderCompileReport currentReport;
        BuildTarget currentBuildTarget = BuildTarget.Android;
        int selectBuildTarget = 0;
        BuildTarget[] buildTargets = new BuildTarget[] { BuildTarget.Android, BuildTarget.iOS, BuildTarget.StandaloneWindows64, BuildTarget.StandaloneOSX };
        string[] displayBuildTargets = new string[] { "Android", "iOS", "Windows", "MacOS" };

        int selection = -1;
        int snippetSelection = -1;
        bool showText = false;
        //左侧shader列表ScrollView位置
        Vector2 scrollViewPos;
        //UI样式
        GUIStyle blackStyle, commentStyle, disabledStyle, foldoutBold, foldoutNormal, foldoutDim, foldoutRTF, buttonNormal, buttonSelected, stateStyle;
        //图标
        GUIContent matIcon, shaderIcon;

        Color preColor;

        SORT_TYPE sortType = SORT_TYPE.VariantsCount;
        //筛选条件
        string keywordFilter;
        string fileFilter;
        //最小关键字数
        int minimumKeywordCount;
        //找到的最大关键字数
        int maxKeywordsCountFound = 0;
        bool showInFilterListShader;

        Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++)
                pix[i] = col;
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        void SetUpStyles()
        {
            if (blackStyle == null)
            {
                // Setup styles
                Color backColor = EditorGUIUtility.isProSkin ? new Color(0.18f, 0.18f, 0.18f) : new Color(0.7f, 0.7f, 0.7f);
                Texture2D _blackTexture;
                _blackTexture = MakeTex(4, 4, backColor);
                _blackTexture.hideFlags = HideFlags.DontSave;
                blackStyle = new GUIStyle();
                blackStyle.normal.background = _blackTexture;
            }
  
            if (commentStyle == null)
            {
                commentStyle = new GUIStyle(EditorStyles.label);
            }
            commentStyle.normal.textColor = EditorGUIUtility.isProSkin ? new Color(0.62f, 0.76f, 0.9f) : new Color(0.32f, 0.36f, 0.42f);
            if (disabledStyle == null)
            {
                disabledStyle = new GUIStyle(EditorStyles.label);
            }
            disabledStyle.normal.textColor = EditorGUIUtility.isProSkin ? new Color(0.52f, 0.66f, 0.8f) : new Color(0.32f, 0.32f, 0.32f);
            if (foldoutRTF == null)
            {
                foldoutRTF = new GUIStyle(EditorStyles.foldout);
            }
            foldoutRTF.richText = true;
            if (foldoutBold == null)
            {
                foldoutBold = new GUIStyle(EditorStyles.foldout);
                foldoutBold.fontStyle = FontStyle.Bold;
            }
            if (foldoutNormal == null)
            {
                foldoutNormal = new GUIStyle(EditorStyles.foldout);
            }
            if (foldoutDim == null)
            {
                foldoutDim = new GUIStyle(EditorStyles.foldout);
                foldoutDim.fontStyle = FontStyle.Italic;
            }
            if (matIcon == null)
            {
                matIcon = EditorGUIUtility.IconContent("PreMatSphere");
                if (matIcon == null)
                    matIcon = new GUIContent();
            }
            if (shaderIcon == null)
            {
                shaderIcon = EditorGUIUtility.IconContent("Shader Icon");
                if (shaderIcon == null)
                    matIcon = new GUIContent();
            }
            if (buttonNormal == null)
            {
                buttonNormal = new GUIStyle(EditorStyles.foldout);
                buttonNormal.fontStyle = FontStyle.Normal;
                buttonNormal.alignment = TextAnchor.MiddleLeft;
            }
            if (buttonSelected == null)
            {
                buttonSelected = new GUIStyle(EditorStyles.foldout);
                buttonSelected.fontStyle = FontStyle.Normal;
                buttonSelected.alignment = TextAnchor.MiddleLeft;
            }
            if (stateStyle == null)
            {
                stateStyle = new GUIStyle(EditorStyles.helpBox);
                stateStyle.fontSize = 10;
            }
        }

        void Awake()
        {
            SetUpStyles();
            if (currentReport == null)
            {
                currentReport = ShaderCompileReport.GetTargetConfigure(currentBuildTarget);
            }
            if (currentReport != null)
            {
                Summary();
            }
        }

        private void Summary()
        {
            int count = currentReport.infos.Count;
            for (int index = 0; index < count; index++)
            {
                ShaderCompileVariantInfo info = currentReport.infos[index];
                info.Summary();
                maxKeywordsCountFound = Mathf.Max(maxKeywordsCountFound, info.keywordsCount);
            }
        }

        void OnGUI()
        {
            //外横框
            GUILayout.BeginHorizontal(GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));

            GUILayout.BeginVertical(blackStyle);
            showText = GUILayout.Toggle(showText,"显示为文本", GUILayout.Width(100), GUILayout.Height(18));
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("平台", GUILayout.Width(30));
            int newtarget = EditorGUILayout.Popup(selectBuildTarget, displayBuildTargets);
            if (selectBuildTarget != newtarget)
            {
                currentBuildTarget = buildTargets[selectBuildTarget];
                currentReport = ShaderCompileReport.GetTargetConfigure(currentBuildTarget);
            }
            EditorGUILayout.EndHorizontal();

            string btnShowString = "显示完全剔除的shader";
            preColor = GUI.color;
            GUI.color = showInFilterListShader ? Color.white : Color.gray;
            if (GUILayout.Button(btnShowString, GUILayout.Width(200)))
            {
                showInFilterListShader = !showInFilterListShader;
            }
            GUI.color = preColor;

            EditorGUILayout.BeginHorizontal(blackStyle);
            EditorGUILayout.LabelField("排序条件", GUILayout.Width(90));
            SORT_TYPE prevSortType = sortType;
            sortType = (SORT_TYPE)EditorGUILayout.EnumPopup(sortType);
            if (sortType != prevSortType)
            {
                SortShaderList();
                EditorGUIUtility.ExitGUI();
                return;
            }
            EditorGUILayout.EndHorizontal();
            if (sortType != SORT_TYPE.Keyword)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("关键字数 >=", GUILayout.Width(90));
                minimumKeywordCount = EditorGUILayout.IntSlider(minimumKeywordCount, 0, maxKeywordsCountFound);
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("文件名过滤", GUILayout.Width(90));
            fileFilter = EditorGUILayout.TextField(fileFilter);
            if (GUILayout.Button(new GUIContent("清空", "清空文件名过滤"), GUILayout.Width(60)))
            {
                fileFilter = "";
                GUIUtility.keyboardControl = 0;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("关键字过滤", GUILayout.Width(90));
            keywordFilter = EditorGUILayout.TextField(keywordFilter);
            if (GUILayout.Button(new GUIContent("清空", "清空关键字过滤"), GUILayout.Width(60)))
            {
                keywordFilter = "";
                GUIUtility.keyboardControl = 0;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Separator();


            scrollViewPos = EditorGUILayout.BeginScrollView(scrollViewPos);

            if (currentReport != null)
            {
                int count =  currentReport.infos.Count;
                for (int index = 0; index < count; index++)
                {
                    preColor = GUI.color;
                    GUIStyle btnStyle = selection == index ? foldoutNormal : foldoutNormal;
                    GUI.color = selection == index ? Color.yellow:Color.white;
                    ShaderCompileVariantInfo info = currentReport.infos[index];
                    
                    if (info.keywordsCount < minimumKeywordCount)
                        continue;
                    if (!showInFilterListShader)
                    {
                        if (info.totalVaraints == info.strippedVaraints)
                        {
                            continue;
                        }
                    }
                    if (!string.IsNullOrEmpty(fileFilter))
                    {
                        bool found = false;
                        if (info.shaderName.IndexOf(fileFilter, StringComparison.CurrentCultureIgnoreCase) >= 0)
                        {
                            found = true;
                        }
                        if (!found)
                            continue;
                    }
                    if (!string.IsNullOrEmpty(keywordFilter))
                    {
                        int kwCount = info.keyWordList.Count;
                        bool found = false;
                        for (int w = 0; w < kwCount; w++)
                        {
                            if (info.keyWordList[w].IndexOf(keywordFilter, StringComparison.CurrentCultureIgnoreCase) >= 0)
                            {
                                found = true;
                                break;
                            }
                        }
                        if (!found)
                            continue;
                    }

                    bool clickshaderName = (GUILayout.Button("" + index + " " + info.shaderName + " (关键字数： " + info.keywordsCount + " 变体总数： " + info.totalVaraints +" 剔除的变体数： " + info.strippedVaraints+")", btnStyle));
                    selection = clickshaderName ? (selection != index ? index : -1) : selection;
                    GUI.color = preColor;
                    if (selection == index)
                    {
                        #region 列表单项
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("", GUILayout.Width(15), GUILayout.Height(18));
                        EditorGUILayout.BeginVertical();

                        if (showText)
                        {
                            string tKeystring = "";
                            foreach (string str in info.keyWordList)
                            {
                                tKeystring += str + ";";
                            } 
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField("关键字:", GUILayout.Width(50), GUILayout.Height(18));
                            EditorGUILayout.TextField(tKeystring, GUILayout.Height(18));
                            EditorGUILayout.EndHorizontal();
                        }
                        else
                        {
                            string[] buildinkeys = info.keyWordList.ToArray();
                        
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField("关键字:", GUILayout.Width(50), GUILayout.Height(18));
                            int select = GUILayout.SelectionGrid(-1, buildinkeys, 3);
                            EditorGUILayout.EndHorizontal();
                        }

                        for (int snippetIndex = 0; snippetIndex < info.snippetComileDatas.Count; snippetIndex++)
                        {
                            SnippetComileDataTuple tuple = info.snippetComileDatas[snippetIndex];
                            //绘制snippet
                            preColor = GUI.color;
                            GUI.color = snippetSelection == snippetIndex ? Color.yellow : Color.white;
                            string snippetTitle = "" + snippetIndex + " PassName:" + tuple.snippet.passName + " ShaderType:" + Enum.GetName(typeof(StripShaderType), tuple.snippet.shaderType) + " PassType:" + Enum.GetName(typeof(StripPassType), tuple.snippet.passType) + " 变体数量:" + tuple.shaderVariants.Count;
                            bool clicksnippetTitle = (GUILayout.Button(new GUIContent(snippetTitle), btnStyle));
                            snippetSelection = clicksnippetTitle ? (snippetSelection != snippetIndex ? snippetIndex:-1) : snippetSelection;
                            GUI.color = preColor;
                            if (snippetSelection == snippetIndex)
                            {
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField("", GUILayout.Width(15), GUILayout.Height(18));

                                EditorGUILayout.BeginVertical();

                                for (int variantIndex = 0; variantIndex < tuple.shaderVariants.Count; variantIndex++)
                                {
                                    StripShaderCompilerData stripShaderCompilerData = tuple.shaderVariants[variantIndex];
                                    preColor = GUI.color;
                                    GUI.color = stripShaderCompilerData.isStripped ? Color.cyan : Color.white;
                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.LabelField("" + variantIndex, GUILayout.Width(45), GUILayout.Height(18));
                                    EditorGUILayout.EndHorizontal();
                                    EditorGUILayout.Toggle("isStripped", stripShaderCompilerData.isStripped);
                                   
                                    string shaderRequirements = Enum.Format(typeof(UnityEditor.Rendering.ShaderRequirements), stripShaderCompilerData.shaderRequirements, "G");
                                    string platforName = Enum.Format(typeof(UnityEditor.Rendering.ShaderCompilerPlatform), stripShaderCompilerData.shaderCompilerPlatform, "G");
                                    string graphicsTierName = Enum.Format(typeof(UnityEngine.Rendering.GraphicsTier), stripShaderCompilerData.graphicsTier, "G");
                                    string stripBuiltinShaderDefineName = Enum.Format(typeof(StripBuiltinShaderDefine), stripShaderCompilerData.platformKeywordSet, "G");
                                    if (showText)
                                    {
                                        EditorGUILayout.TextField("ShaderCompilerPlatform:", platforName, GUILayout.Height(18));
                                        EditorGUILayout.TextField("GraphicsTier:", graphicsTierName, GUILayout.Height(18));
                                        EditorGUILayout.TextField("ShaderRequirements:", shaderRequirements, GUILayout.Height(18));
                                        EditorGUILayout.TextField("PlatformKeywordSet:", stripBuiltinShaderDefineName, GUILayout.Height(18));
                                        
                                        string builtinDefaultKeystring = "";
                                        foreach (string str in stripShaderCompilerData.builtinDefaultList)
                                        {
                                            builtinDefaultKeystring += str +";";
                                        }
                                        EditorGUILayout.BeginHorizontal();
                                       // EditorGUILayout.LabelField("BuiltinDefault Keywords:", GUILayout.Width(200), GUILayout.Height(18));
                                        EditorGUILayout.TextField("BuiltinDefault Keywords:", builtinDefaultKeystring, GUILayout.Height(18));
                                        EditorGUILayout.EndHorizontal();

                                        string builtinExtraKeystring = "";
                                        foreach (string str in stripShaderCompilerData.builtinExtraList)
                                        {
                                            builtinExtraKeystring += str + ";";
                                        }
                                        EditorGUILayout.BeginHorizontal();
                                      //  EditorGUILayout.LabelField("BuiltinExtra Keywords:", GUILayout.Width(200), GUILayout.Height(18));
                                        EditorGUILayout.TextField("BuiltinExtra Keywords:", builtinExtraKeystring, GUILayout.Height(18));
                                        EditorGUILayout.EndHorizontal();

                                        string builtinAutoStrippedKeystring = "";
                                        foreach (string str in stripShaderCompilerData.builtinAutoStrippedList)
                                        {
                                            builtinAutoStrippedKeystring += str + ";";
                                        }
                                        EditorGUILayout.BeginHorizontal();
                                     //   EditorGUILayout.LabelField("BuiltinAutoStripped Keywords:", GUILayout.Width(200), GUILayout.Height(18));
                                        EditorGUILayout.TextField("BuiltinAutoStripped Keywords:", builtinAutoStrippedKeystring, GUILayout.Height(18));
                                        EditorGUILayout.EndHorizontal();

                                        string userDefinedKeystring = "";
                                        foreach (string str in stripShaderCompilerData.userDefinedList)
                                        {
                                            builtinAutoStrippedKeystring += str + ";";
                                        }
                                        EditorGUILayout.BeginHorizontal();
                                       // EditorGUILayout.LabelField("UserDefined Keywords:", GUILayout.Width(200), GUILayout.Height(18));
                                        EditorGUILayout.TextField("UserDefined Keywords:",userDefinedKeystring, GUILayout.Height(18));
                                        EditorGUILayout.EndHorizontal();
                                    }
                                    else
                                    {
                                        EditorGUILayout.EnumPopup("ShaderCompilerPlatform:", stripShaderCompilerData.shaderCompilerPlatform, GUILayout.Height(18));
                                        EditorGUILayout.EnumPopup("GraphicsTier:", stripShaderCompilerData.graphicsTier, GUILayout.Height(18));
                                        EditorGUILayout.EnumFlagsField("ShaderRequirements:", stripShaderCompilerData.shaderRequirements, GUILayout.Height(18));
                                        EditorGUILayout.EnumFlagsField("PlatformKeywordSet:", stripShaderCompilerData.platformKeywordSet, GUILayout.Height(18));

                                        EditorGUILayout.BeginHorizontal();
                                        EditorGUILayout.LabelField("BuiltinDefault Keywords:", GUILayout.Height(18));
                                        GUILayout.SelectionGrid(-1, stripShaderCompilerData.builtinDefaultList.ToArray(), 5);
                                        EditorGUILayout.EndHorizontal();
                                        EditorGUILayout.BeginHorizontal();
                                        EditorGUILayout.LabelField("BuiltinExtra Keywords:", GUILayout.Height(18));
                                        GUILayout.SelectionGrid(-1, stripShaderCompilerData.builtinExtraList.ToArray(), 5);
                                        EditorGUILayout.EndHorizontal();
                                        EditorGUILayout.BeginHorizontal();
                                        EditorGUILayout.LabelField("BuiltinAutoStripped Keywords:", GUILayout.Height(18));
                                        GUILayout.SelectionGrid(-1, stripShaderCompilerData.builtinAutoStrippedList.ToArray(), 5);
                                        EditorGUILayout.EndHorizontal();
                                        EditorGUILayout.BeginHorizontal();
                                        EditorGUILayout.LabelField("UserDefined Keywords:", GUILayout.Height(18));
                                        GUILayout.SelectionGrid(-1, stripShaderCompilerData.userDefinedList.ToArray(), 5);
                                        EditorGUILayout.EndHorizontal();
                                    }
                                    GUI.color = preColor;
                                   EditorGUILayout.Separator();
                                }
                                EditorGUILayout.EndVertical();
                                EditorGUILayout.EndHorizontal();
                            }
                        }
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.EndHorizontal();
                        #endregion
                    }

                }
            }

            EditorGUILayout.EndScrollView();

            GUILayout.EndVertical();

            //外横框结束
            GUILayout.EndHorizontal();
        }

        //对list排序
        void SortShaderList()
        {
            switch (sortType)
            {
                case SORT_TYPE.VariantsCount:
                    currentReport.infos.Sort((ShaderCompileVariantInfo x, ShaderCompileVariantInfo y) => {
                        return y.totalVaraints.CompareTo(x.totalVaraints);
                    });
                    break;
                case SORT_TYPE.EnabledKeywordsCount:
                    currentReport.infos.Sort((ShaderCompileVariantInfo x, ShaderCompileVariantInfo y) => {
                        return y.keywordsCount.CompareTo(x.keywordsCount);
                    });
                    break;
                case SORT_TYPE.ShaderFileName:
                    currentReport.infos.Sort((ShaderCompileVariantInfo x, ShaderCompileVariantInfo y) => {
                        return x.shaderName.CompareTo(y.shaderName);
                    });
                    break;
            }
        }
    }
}