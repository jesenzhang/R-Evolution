using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ShaderVariantsStripper
{
    enum SORT_TYPE
    {
        VariantsCount = 0,
        EnabledKeywordsCount = 1,
        ShaderFileName = 2,
        Keyword = 3
    }

    //关键字与着色器的反向关系
    class KeywordShaderRelation
    {
        public string keyword;
        public List<StripShader> shaders;
        public bool foldout;
    }

    public class ShaderVariantsStripperWindow : EditorWindow
    {
        private static ShaderVariantsStripperWindow m_window;
        public static ShaderVariantsStripperWindow Window
        {
            get
            {
                if (m_window == null)
                {
                    m_window = EditorWindow.GetWindow<ShaderVariantsStripperWindow>("ShaderVariantsStripper");
                    m_window.minSize = new Vector2(900, 600);
                }
                return m_window;
            }
        }
        [MenuItem("ShaderVariantsStripper/OpenStripperWindow", priority = 200)]
        public static void OpenWindow()
        {
            Window.Show();
        }

        [MenuItem("ShaderVariantsStripper/Compiler")]
        public static void Compiler()
        {
            ShaderVariantCollection collection = AssetDatabase.LoadAssetAtPath<ShaderVariantCollection>("Assets/Shader/ShaderCollection/Shaders.shadervariants");
            collection.WarmUp();
        }

        [MenuItem("ShaderVariantsStripper/Create Report File")]
        public static void Report()
        {
            ShaderCompileReport configure = AssetDatabase.LoadAssetAtPath<ShaderCompileReport>(ShaderCompileReport.ConfigureFilePath());

            if (configure == null)
            {
                configure = ScriptableObject.CreateInstance<ShaderCompileReport>();
                AssetDatabase.CreateAsset(configure, ShaderCompileReport.ConfigureFilePath());
            }
            ShaderCompileReport.Save();
        }

        [MenuItem("ShaderVariantsStripper/Create Configure File")]
        static void CreateConfigureFile()
        {
            ShaderVariantsStripperConfigure configure = AssetDatabase.LoadAssetAtPath<ShaderVariantsStripperConfigure>(ShaderVariantsStripperConfigure.ConfigureFilePath());

            if (configure == null)
            {
                configure = ScriptableObject.CreateInstance<ShaderVariantsStripperConfigure>();
                AssetDatabase.CreateAsset(configure, ShaderVariantsStripperConfigure.ConfigureFilePath());
            }
            ShaderVariantsStripperConfigure.Save();
        }
        [MenuItem("ShaderVariantsStripper/Enable Log")]
        static void EnableLogOnly()
        {
            ShaderVariantsStripperConfigure.Configure.enableLog = true;
            ShaderVariantsStripperConfigure.Save();
        }

        [MenuItem("ShaderVariantsStripper/Disable Log")]
        static void DisableLogOnly()
        {
            ShaderVariantsStripperConfigure.Configure.enableLog = false;
            ShaderVariantsStripperConfigure.Save();
        }
        #region GUI
        private GenericMenu m_operationMenu;
        //UI样式
        GUIStyle blackStyle, commentStyle, disabledStyle, foldoutBold, foldoutNormal, foldoutDim, foldoutRTF, buttonNormal, buttonSelected, stateStyle;
        //图标
        GUIContent matIcon, shaderIcon;

        private Color m_preColor;
        //左侧shader列表ScrollView位置
        Vector2 scrollViewPos;

        float mTitleWiith = 100;
        float mCommonItemWidth = 300;
        //剔除滑动界面
        Vector2 stripScrollViewPos;
        #endregion

        #region 数据
        //所有shader的列表
        List<StripShader> shaderList;
        //全部shader数
        int totalShaderCount;
        //最小关键字数
        int minimumKeywordCount;
        //找到的最大关键字数
        int maxKeywordsCountFound = 0;
        //关键字数   变体数   使用的关键字数  编译的变体数   在白名单中的数量
        int totalKeywords, totalVariants, totalUsedKeywords, totalBuildVariants, totalInWhitelist;
        //是否全部
        bool scanAllShaders;
        //排序条件
        SORT_TYPE sortType = SORT_TYPE.VariantsCount;
        //筛选条件
        string keywordFilter;
        string fileFilter;

        Dictionary<string, List<StripShader>> uniqueKeywords, uniqueEnabledKeywords;

        Dictionary<string, int> shaderGUIDToWhitelistIndex;

        List<KeywordShaderRelation> keywordToShadersList;

        int selectBuildTarget = 0;
        BuildTarget[] buildTargets = new BuildTarget[] { BuildTarget.Android, BuildTarget.iOS, BuildTarget.StandaloneWindows64, BuildTarget.StandaloneOSX };
        string[] displayBuildTargets = new string[] { "Android", "iOS", "Windows", "MacOS" };
        ShaderVariantsStripperConfigure currentStripConfigure;
        BuildTarget currentBuildTarget = BuildTarget.Android;

        int selection = -1;
        string editorstring = "";
        bool addingKeyword = false;
        bool showKeyword = false;
        //显示在白名单中的shader
        bool showInWhiteListShader = true;
        //显示不在白名单中的shader
        bool showNotInWhiteListShader = true;
        //显示有源文件的shader
        bool showHasSourceShader = true;
        //显示多选shader
        bool showMultiSelect = true;
        //全选
        bool selectAll = false;
        //显示筛选条件
        bool showFilterItems = true;
        Color preColor;

        bool builtinDefaultOpen = false;
        bool builtinExtraOpen = false;
        bool BuiltinAutoStrippedOpen = false;
        bool BuiltinUserDefinedOpen = false;

        string[] buidinKeywordsNames = new string[] { "TANGENT_SPACE_ROTATION","FOG_EXP2",
        };
        string[] multi_compile_fwdbase_KeywordsNames = new string[] {
            "FOG_EXP","FOG_EXP2","FOG_LINEAR","INSTANCING_ON","DIRECTIONAL","DIRLIGHTMAP_COMBINED","DYNAMICLIGHTMAP_ON","LIGHTMAP_ON",
            "LIGHTMAP_SHADOW_MIXING","LIGHTPROBE_SH","SHADOWS_SCREEN","SHADOWS_SHADOWMASK","VERTEXLIGHT_ON","DIRECTIONAL_COOKIE","POINT","POINT_COOKIE","SPOT",
            "SHADOWS_CUBE","SHADOWS_DEPTH","UNITY_HDR_ON","EDITOR_VISUALIZATION",};
        string[] multi_compile_fwdadd_KeywordsNames = new string[] {
            "FOG_EXP","FOG_EXP2","FOG_LINEAR","INSTANCING_ON","DIRECTIONAL","DIRLIGHTMAP_COMBINED","DYNAMICLIGHTMAP_ON","LIGHTMAP_ON",
            "LIGHTMAP_SHADOW_MIXING","LIGHTPROBE_SH","SHADOWS_SCREEN","SHADOWS_SHADOWMASK","VERTEXLIGHT_ON","DIRECTIONAL_COOKIE","POINT","POINT_COOKIE","SPOT",
            "SHADOWS_CUBE","SHADOWS_DEPTH","UNITY_HDR_ON","EDITOR_VISUALIZATION",
        };
        string[] multi_compile_fwdadd_fullshadows_KeywordsNames = new string[] {
            "FOG_EXP","FOG_EXP2","FOG_LINEAR","INSTANCING_ON","DIRECTIONAL","DIRLIGHTMAP_COMBINED","DYNAMICLIGHTMAP_ON","LIGHTMAP_ON",
            "LIGHTMAP_SHADOW_MIXING","LIGHTPROBE_SH","SHADOWS_SCREEN","SHADOWS_SHADOWMASK","VERTEXLIGHT_ON","DIRECTIONAL_COOKIE","POINT","POINT_COOKIE","SPOT",
            "SHADOWS_CUBE","SHADOWS_DEPTH","UNITY_HDR_ON","EDITOR_VISUALIZATION","SHADOWS_SOFT",
        };
        string[] multi_compile_fog_KeywordsNames = new string[] {
            "FOG_EXP","FOG_EXP2","FOG_LINEAR","INSTANCING_ON","DIRECTIONAL","DIRLIGHTMAP_COMBINED","DYNAMICLIGHTMAP_ON","LIGHTMAP_ON",
            "LIGHTMAP_SHADOW_MIXING","LIGHTPROBE_SH","SHADOWS_SCREEN","SHADOWS_SHADOWMASK","VERTEXLIGHT_ON","DIRECTIONAL_COOKIE","POINT","POINT_COOKIE","SPOT",
            "SHADOWS_CUBE","SHADOWS_DEPTH","UNITY_HDR_ON","EDITOR_VISUALIZATION",
        };
        List<string> buildInDefaultKeys = new List<string>() { "POINT", "SPOT", "DIRECTIONAL", "POINT_COOKIE", "DIRECTIONAL_COOKIE", "VERTEXLIGHT_ON", "LOD_FADE_CROSSFADE", "EDITOR_VISUALIZATION", "_EMISSION", "LIGHTPROBE_SH", "SOFTPARTICLES_ON", "SHADOWS_SCREEN", "SHADOWS_SOFT", "SHADOWS_DEPTH", "SHADOWS_CUBE", "UNITY_HDR_ON", "SHADOWS_SPLIT_SPHERES", "ETC1_EXTERNAL_ALPHA", };
        List<string> buildInExtraKeys = new List<string>() { "BILLBOARD_FACE_CAMERA_POS", "_NORMALMAP", };
        List<string> buildInAutoStripKeys = new List<string>() { "UNITY_SINGLE_PASS_STEREO", "DYNAMICLIGHTMAP_ON", "DIRLIGHTMAP_COMBINED", "LIGHTMAP_ON", "STEREO_MULTIVIEW_ON", "STEREO_INSTANCING_ON", "FOG_LINEAR", "FOG_EXP", "FOG_EXP2", "LIGHTMAP_SHADOW_MIXING", "SHADOWS_SHADOWMASK", "INSTANCING_ON", "PROCEDURAL_INSTANCING_ON", "STEREO_CUBEMAP_RENDER_ON", };
        List<string> userDefineKeys = new List<string>() {  "EFFECT_BUMP","EFFECT_EXTRA_TEX","EFFECT_HUE_VARIATION","EFFECT_BILLBOARD","EFFECT_SUBSURFACE","_WINDQUALITY_BEST","_WINDQUALITY_PALM","GEOM_TYPE_BRANCH_DETAIL","LOD_FADE_PERCENTAGE","UNITY_COLORSPACE_GAMMA","_GLOSSYREFLECTIONS_OFF","PIXELSNAP_ON","_PARALLAXMAP","_SPECGLOSSMAP","_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A","_METALLICGLOSSMAP","_ALPHATEST_ON","_ALPHAPREMULTIPLY_ON","DIRLIGHTMAP_OFF","LIGHTMAP_OFF","DYNAMICLIGHTMAP_OFF","GEOM_TYPE_BRANCH","GEOM_TYPE_FROND","GEOM_TYPE_LEAF","_TERRAIN_NORMAL_MAP","_FADING_ON","_COLOROVERLAY_ON","_COLORCOLOR_ON","_COLORADDSUBDIFF_ON","_ALPHAMODULATE_ON","_WINDQUALITY_NONE","_WINDQUALITY_FAST","_WINDQUALITY_BETTER","GEOM_TYPE_MESH","_AO","UNITY_PASS_SHADOWCASTER","_SPECULARHIGHLIGHTS_OFF","_ALPHABLEND_ON","_DETAIL_MULX2","_REQUIRE_UV2",
};

        public ShaderVariantsStripperFilter DefaultShaderVariantsStripperFilter
        {
            get
            {
                if (currentStripConfigure == null)
                {
                    currentStripConfigure = ShaderVariantsStripperConfigure.GetTargetConfigure(currentBuildTarget);
                }
                if (currentStripConfigure != null)
                {
                    return currentStripConfigure.defaultShaderVariantsStripperFilter;
                }
                return null;
            }
        }

        #endregion

        void Awake()
        {
            SetUpStyles();

            if (currentStripConfigure == null)
            {
                currentStripConfigure = ShaderVariantsStripperConfigure.GetTargetConfigure(currentBuildTarget);
            }
            //  BuildinKeywordInit();
            ScanProject();
        }


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

        void OnGUI()
        {
            m_preColor = GUI.backgroundColor;
            //外横框
            GUILayout.BeginHorizontal(GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
            #region shader遍历分析
            EditorGUILayout.BeginVertical(blackStyle);
            #region 上半部分 包含扫描按钮和统计信息
            EditorGUILayout.Separator();

            EditorGUILayout.BeginHorizontal();
            scanAllShaders = EditorGUILayout.Toggle(new GUIContent("强制扫描全部shader", "包括工程目录下所有的shader"), scanAllShaders);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("扫描工程shader", "快速遍历工程shader和keyword")))
            {
                ScanProject();
                GUIUtility.ExitGUI();
                return;
            }
            if (shaderList != null && shaderList.Count > 0 && GUILayout.Button(new GUIContent("清理关联材质", "清理掉材质中关掉的关键字 保证材质中没有使用没用的关键字\n\nTo disable keywords, first expand any shader from the list and uncheck the unwanted keywords (press 'Save' to modify the shader file and to clean any existing material that uses that specific shader)."), GUILayout.Width(120)))
            {
                CleanAllMaterials();
                GUIUtility.ExitGUI();
                return;
            }
            EditorGUILayout.EndHorizontal();

            if (shaderList != null)
            {
                //所有关键字都被使用了
                if (totalKeywords == totalUsedKeywords || totalKeywords == 0)
                {
                    string statestring = "shader总数: " + totalShaderCount.ToString() + "  使用关键字的shader总数: " + shaderList.Count.ToString() + "\n关键字总数: " + totalKeywords.ToString() + "  变体总数: " + totalVariants.ToString() + " 在白名单中的shader总数：" + totalInWhitelist;
                    EditorGUILayout.HelpBox(statestring, MessageType.Info);
                }
                else
                {
                    int keywordsPerc = totalUsedKeywords * 100 / totalKeywords;
                    int variantsPerc = totalBuildVariants * 100 / totalVariants;
                    string statestring = "shader总数: " + totalShaderCount.ToString() + "  使用关键字的shader总数: " + shaderList.Count.ToString() + "\n被使用的关键字占比: " + totalUsedKeywords.ToString() + "/" + totalKeywords.ToString() + " (" + keywordsPerc.ToString() + "%) 实际变体占比: " + totalBuildVariants.ToString() + "/" + totalVariants.ToString() + " (" + variantsPerc.ToString() + "%)" + " 在白名单中的shader总数：" + totalInWhitelist;
                    EditorGUILayout.HelpBox(statestring, MessageType.Info);
                }
                EditorGUILayout.Separator();
            }
            #endregion

            EditorGUILayout.Separator();

            #region 下半部分 shader列表和变体材质信息
            if (shaderList != null)
            {
                GUILayout.BeginVertical(blackStyle);

                #region 左侧列表列表显示控制部分
                int shaderCount = shaderList.Count;
                //列表显示控制
                if (shaderCount > 1)
                {
                    EditorGUILayout.BeginHorizontal(blackStyle);
                    string btnShowFilter = "显示筛选条件";
                    preColor = GUI.color;
                    GUI.color = showFilterItems ? Color.white : Color.gray;
                    if (GUILayout.Button(btnShowFilter, GUILayout.MaxWidth(100)))
                    {
                        showFilterItems = !showFilterItems;
                    }
                    GUI.color = preColor;

                    string btnShowString0 = "显示复选框";
                    preColor = GUI.color;
                    GUI.color = showMultiSelect ? Color.white : Color.gray;
                    if (GUILayout.Button(btnShowString0, GUILayout.MaxWidth(100)))
                    {
                        showMultiSelect = !showMultiSelect;
                    }
                    GUI.color = preColor;
                    string btnShowString = "显示白名单中的shader";
                    preColor = GUI.color;
                    GUI.color = showInWhiteListShader ? Color.white : Color.gray;
                    if (GUILayout.Button(btnShowString, GUILayout.MaxWidth(200)))
                    {
                        showInWhiteListShader = !showInWhiteListShader;
                    }
                    GUI.color = preColor;
                    preColor = GUI.color;
                    GUI.color = showNotInWhiteListShader ? Color.white : Color.gray;
                    string btnShowString2 = "显示白名单之外的shader";
                    if (GUILayout.Button(btnShowString2, GUILayout.MaxWidth(200)))
                    {
                        showNotInWhiteListShader = !showNotInWhiteListShader;
                    }
                    GUI.color = preColor;
                    preColor = GUI.color;
                    GUI.color = showHasSourceShader ? Color.white : Color.gray;
                    string btnShowString3 = "仅显示有源文件的shader";
                    if (GUILayout.Button(btnShowString3, GUILayout.MaxWidth(200)))
                    {
                        showHasSourceShader = !showHasSourceShader;
                    }
                    GUI.color = preColor;
                    if (showMultiSelect)
                    {
                        preColor = GUI.color;
                        GUI.color = showHasSourceShader ? Color.white : Color.gray;
                        string btnselectall = selectAll ? "取消全选" : "全选";
                        if (GUILayout.Button(btnselectall, GUILayout.MaxWidth(100)))
                        {
                            selectAll = !selectAll;
                            for (int j = 0; j < shaderList.Count; j++)
                            {
                                shaderList[j].selected = selectAll;
                            }
                        }
                        GUI.color = preColor;
                    }
                    EditorGUILayout.EndHorizontal();

                    if (showFilterItems)
                    {
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
                        fileFilter = EditorGUILayout.TextField(keywordFilter);
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
                    }

                    EditorGUILayout.Separator();
                }
                #endregion

                #region Shader列表
                scrollViewPos = EditorGUILayout.BeginScrollView(scrollViewPos);
                if (sortType == SORT_TYPE.Keyword)
                {
                    DrawKeywordSortShaderList();
                }
                else
                {
                    DrawOtherSortShaderList();
                }
                EditorGUILayout.EndScrollView();
                #endregion
                GUILayout.EndVertical();
            }
            #endregion

            GUILayout.EndVertical();
            #endregion
            #region 剔除界面
            //右侧竖排
            if (shaderList != null)
            {
                GUILayout.BeginVertical(blackStyle);
                EditorGUILayout.LabelField("变体剔除白名单", GUILayout.Width(200));
                int newtarget = EditorGUILayout.Popup(selectBuildTarget, displayBuildTargets);
                if (selectBuildTarget != newtarget)
                {
                    selectBuildTarget = newtarget;
                    currentBuildTarget = buildTargets[selectBuildTarget];
                    currentStripConfigure = ShaderVariantsStripperConfigure.GetTargetConfigure(currentBuildTarget);
                    GUIUtility.ExitGUI();
                    return;
                }
                if (currentStripConfigure != null)
                {
                    GUILayout.BeginHorizontal(blackStyle);
                    EditorGUILayout.LabelField("是否使用变体剔除", GUILayout.Width(200));
                    bool useStripper = EditorGUILayout.Toggle(currentStripConfigure.useStripper, GUILayout.Width(18));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal(blackStyle);
                    EditorGUILayout.LabelField("是否使用白名单", GUILayout.Width(200));
                    bool useWhitelist = EditorGUILayout.Toggle(currentStripConfigure.useWhitelist, GUILayout.Width(18));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal(blackStyle);
                    EditorGUILayout.LabelField("是否输出剔除结果", GUILayout.Width(200));
                    bool enableLog = EditorGUILayout.Toggle(currentStripConfigure.enableLog, GUILayout.Width(18));
                    GUILayout.EndHorizontal();

                    if (useStripper != currentStripConfigure.useStripper)
                    {
                        currentStripConfigure.useStripper = useStripper;
                    }
                    if (useWhitelist != currentStripConfigure.useWhitelist)
                    {
                        currentStripConfigure.useWhitelist = useWhitelist;
                    }
                    if (enableLog != currentStripConfigure.enableLog)
                    {
                        currentStripConfigure.enableLog = enableLog;
                    }
                }

                if (currentStripConfigure.useWhitelist)
                {
                    GUILayout.Label("剔除白名单过滤模板：");
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("", GUILayout.Width(15), GUILayout.Height(18));
                    GUILayout.BeginVertical();
                    ShaderVariantsStripperFilter filter = DefaultShaderVariantsStripperFilter;
                    DrawShaderVariantsStripperFilter(ref filter);
                    GUILayout.EndVertical();
                    GUILayout.EndHorizontal();

                    if (GUILayout.Button(new GUIContent("对选中shader生成剔除白名单", "每个shader创建一个白名单 默认使用全部关键字 只会生成白名单内的变体")))
                    {
                        GenWhiteList();
                        GUIUtility.ExitGUI();
                        return;
                    }

                    if (GUILayout.Button(new GUIContent("将选中shader从白名单中删除")))
                    {
                        RemoveWhiteList();
                        GUIUtility.ExitGUI();
                        return;
                    }
                }

                EditorGUILayout.Separator();
                EditorGUILayout.Separator();
                EditorGUILayout.Separator();

                //绘制当前选中的shader的白名单过滤配置
                DrawShaderFilterInspector();

                if (GUILayout.Button(new GUIContent("保存")))
                {
                    ShaderVariantsStripperConfigure.Save();
                    GUIUtility.ExitGUI();
                    return;
                }
                EditorGUILayout.Separator();
                EditorGUILayout.Separator();
                EditorGUILayout.Separator();

                GUILayout.EndVertical();
            }
            #endregion

            //外横框结束
            GUILayout.EndHorizontal();

        }

        void OnDestroy()
        {

        }
        //绘制选中的shader的过滤信息
        void DrawShaderFilterInspector()
        {
            if (selection >= 0 && shaderList.Count > selection)
            {
                GUILayout.Label("当前选中shader过滤项：");
                stripScrollViewPos = GUILayout.BeginScrollView(stripScrollViewPos);
                StripShader stripShader = shaderList[selection];
                ShaderVariantsStripperFilter filter = GetShaderFilterItem(ref stripShader);
                if (filter != null)
                {
                    DrawShaderVariantsStripperFilter(ref filter);
                    addingKeyword = EditorGUILayout.Foldout(addingKeyword, new GUIContent("添加关键字："), true, foldoutNormal);
                    if (addingKeyword)
                    {
                        builtinDefaultOpen = EditorGUILayout.Foldout(builtinDefaultOpen, "BuiltinDefaultKeys", true);
                        if (builtinDefaultOpen)
                            DrawBuildInShaderKeywordSelectGrid(UnityEngine.Rendering.ShaderKeywordType.BuiltinDefault, ref filter.keywords);
                        builtinExtraOpen = EditorGUILayout.Foldout(builtinExtraOpen, "BuiltinExtraKeys", true);
                        if (builtinExtraOpen)
                            DrawBuildInShaderKeywordSelectGrid(UnityEngine.Rendering.ShaderKeywordType.BuiltinExtra, ref filter.keywords);
                        BuiltinAutoStrippedOpen = EditorGUILayout.Foldout(BuiltinAutoStrippedOpen, "BuiltinAutoStrippedKeys", true);
                        if (BuiltinAutoStrippedOpen)
                            DrawBuildInShaderKeywordSelectGrid(UnityEngine.Rendering.ShaderKeywordType.BuiltinAutoStripped, ref filter.keywords);
                        BuiltinUserDefinedOpen = EditorGUILayout.Foldout(BuiltinUserDefinedOpen, "BuiltinUserDefinedKeys", true);
                        if (BuiltinUserDefinedOpen)
                            DrawBuildInShaderKeywordSelectGrid(UnityEngine.Rendering.ShaderKeywordType.UserDefined, ref filter.keywords);
                        editorstring = "";
                        editorstring = GUILayout.TextField(editorstring);
                        editorstring = editorstring.Trim();
                        editorstring = editorstring.ToUpper();
                        if (GUILayout.Button(new GUIContent("添加关键字", "关键字以分号间隔")))
                        {
                            string[] keys = editorstring.Split(';');
                            for (int kk = 0; kk < keys.Length; kk++)
                            {
                                if (!filter.keywords.Contains(keys[kk]))
                                {
                                    filter.keywords.Add(keys[kk]);
                                }
                            }
                            GUIUtility.ExitGUI();
                            return;
                        }
                    }

                }
                GUILayout.EndScrollView();
            }
        }

        //排序条件不是keuword时的绘制函数
        void DrawOtherSortShaderList()
        {
            int shaderCount = shaderList.Count;
            for (int s = 0; s < shaderCount; s++)
            {
                StripShader shader = shaderList[s];
                shader.showing = false;
                //筛选显示的shader
                if (shader.inStripWhiteList && !showInWhiteListShader)
                    continue;
                if (!shader.inStripWhiteList && !showNotInWhiteListShader)
                    continue;
                if (shader.keywordEnabledCount < minimumKeywordCount)
                    continue;
                if (!string.IsNullOrEmpty(fileFilter))
                {
                    bool found = false;
                    if (shader.shaderName.IndexOf(fileFilter, StringComparison.CurrentCultureIgnoreCase) >= 0)
                    {
                        found = true;
                        break;
                    }
                    if (!found)
                        continue;
                }
                if (!string.IsNullOrEmpty(keywordFilter))
                {
                    int kwCount = shader.keywords.Count;
                    bool found = false;
                    for (int w = 0; w < kwCount; w++)
                    {
                        if (shader.keywords[w].name.IndexOf(keywordFilter, StringComparison.CurrentCultureIgnoreCase) >= 0)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                        continue;
                }
                if (!shader.hasSource && showHasSourceShader)
                {
                    continue;
                }

                shader.showing = true;

                #region 着色器选项开关绘制区
                EditorGUILayout.BeginHorizontal();
                string shaderName = shader.isReadOnly ? shader.shaderName + " (只读)" : shader.shaderName;
                string inwhitestate = shader.inStripWhiteList ? " (在白名单中) " : " (不在白名单中) ";
                string foldName = shader.hasSource ? "" + s + " " + shaderName + " (" + shader.keywords.Count + " 关键字（keywords）, " + shader.keywordEnabledCount + " 生效(enabled), " + shader.actualBuildVariantCount + "编译的变体(Variant))； 总共" + shader.totalVariantCount + "变体(包括unused shader_features))" + inwhitestate : "" + s + " " + shaderName + " (" + shader.keywordEnabledCount + " 关键字被材质使用)" + inwhitestate;
                //   shader.foldout = EditorGUILayout.Foldout(shader.foldout, new GUIContent(foldName), shader.hasSource?(shader.editedByShaderControl ? foldoutBold : foldoutNormal): foldoutDim);
                //选中的shader
                preColor = GUI.color;
                if (showMultiSelect)
                {
                    shader.selected = GUILayout.Toggle(shader.selected, "", GUILayout.Width(15));
                }
                else
                    shader.selected = false;
                GUIStyle btnStyle = shader.hasSource ? (shader.editedByShaderControl ? foldoutBold : foldoutNormal) : foldoutDim;
                GUI.color = shader.inStripWhiteList ? GUI.color = Color.white : GUI.color = Color.red;
                GUI.color = selection == s ? Color.yellow : GUI.color;
                if (GUILayout.Button(new GUIContent(foldName), btnStyle))
                {
                    if (selection != s)
                    {
                        selection = s;
                        OnShaderSelectionChange();
                    }
                    shader.foldout = !shader.foldout;
                }
                GUI.color = preColor;
                EditorGUILayout.EndHorizontal();
                #endregion

                if (shader.foldout)
                {
                    #region 着色器打开的文件源路径绘制区
                    if (shader.hasSource)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("", GUILayout.Width(15));
                        EditorGUILayout.LabelField("路径", GUILayout.Width(30));
                        EditorGUILayout.SelectableLabel(shader.path, GUILayout.Height(18));
                        EditorGUILayout.EndHorizontal();
                    }
                    else
                    {
                        EditorGUILayout.LabelField("(找不到着色器源文件)");
                        EditorGUILayout.BeginHorizontal();
                        if (shader.materials.Count > 0 && GUILayout.Button(new GUIContent(shader.showMaterials ? "隐藏材质" : "显示材质", "显示或隐藏使用关键字的材质"), GUILayout.Width(110)))
                        {
                            shader.showMaterials = !shader.showMaterials;
                            GUIUtility.ExitGUI();
                            return;
                        }
                        //白名单操作
                        DrawShaderWhitelistOption(ref shader);
                        EditorGUILayout.EndHorizontal();
                    }
                    #endregion

                    #region 着色器打开的功能按钮绘制区
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("", GUILayout.Width(15));
                    if (shader.hasSource)
                    {
                        //绘制定位按钮
                        DrawObjectLocateButton(shader.path);
                        //绘制打开按钮
                        DrawObjectOpenButton(shader.path);
                        //白名单操作
                        DrawShaderWhitelistOption(ref shader);
                        if (!shader.pendingChanges)
                            GUI.enabled = false;
                        if (GUILayout.Button(new GUIContent("保存", "保存关键字的改变 点击关键字左边的切换勾选 以打开或关闭关键字) 备份在同目录下生成"), GUILayout.Width(60)))
                        {
                            UpdateShader(ref shader);
                        }
                        GUI.enabled = true;
                        if (!shader.hasBackup)
                            GUI.enabled = false;
                        if (GUILayout.Button(new GUIContent("重载", "从备份文件夹拷贝"), GUILayout.Width(60)))
                        {
                            RestoreShader(ref shader);
                            GUIUtility.ExitGUI();
                            return;
                        }
                        GUI.enabled = true;
                        if (shader.materials.Count > 0 && GUILayout.Button(new GUIContent(shader.showMaterials ? "隐藏材质" : "显示材质", "显示或隐藏使用关键字的材质"), GUILayout.Width(95)))
                        {
                            shader.showMaterials = !shader.showMaterials;
                            GUIUtility.ExitGUI();
                            return;
                        }
                    }

                    EditorGUILayout.EndHorizontal();
                    #endregion

                    #region 着色器打开的关键字绘制区
                    for (int k = 0; k < shader.keywords.Count; k++)
                    {
                        StripKeyword keyword = shader.keywords[k];
                        //不绘制 __
                        if (keyword.isUnderscoreKeyword)
                            continue;

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("", GUILayout.Width(15));
                        if (shader.hasSource)
                        {
                            bool prevState = keyword.enabled;
                            keyword.enabled = EditorGUILayout.Toggle(prevState, GUILayout.Width(18));
                            if (prevState != keyword.enabled)
                            {
                                shader.pendingChanges = true;
                                shader.UpdateVariantCount();
                                UpdateProjectStats();
                                GUIUtility.ExitGUI();
                                return;
                            }
                        }
                        else
                        {
                            EditorGUILayout.Toggle(true, GUILayout.Width(18));
                        }
                        if (!keyword.enabled)
                        {
                            EditorGUILayout.SelectableLabel(keyword.name, disabledStyle, GUILayout.Height(18));
                        }
                        else
                        {
                            EditorGUILayout.SelectableLabel(keyword.name, GUILayout.Height(18));
                            if (!shader.hasSource && GUILayout.Button(new GUIContent("裁剪关键字", "从所有关联的材质中去掉该关键字."), GUILayout.Width(110)))
                            {
                                if (EditorUtility.DisplayDialog("裁剪关键字", "该操作会在所有使用" + shader.name + "着色器的材质中关闭关键字" + keyword.name + "\n是否继续?", "继续", "取消"))
                                {
                                    PruneMaterials(shader, keyword.name);
                                    UpdateProjectStats();
                                }
                            }
                        }
                        EditorGUILayout.EndHorizontal();

                        if (shader.showMaterials)
                        {
                            int matCount = shader.materials.Count;
                            for (int m = 0; m < matCount; m++)
                            {
                                StripMaterial material = shader.materials[m];
                                if (material.ContainsKeyword(keyword.name))
                                {
                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.LabelField("", GUILayout.Width(30));
                                    EditorGUILayout.LabelField(matIcon, GUILayout.Width(18));
                                    EditorGUILayout.LabelField(material.name);
                                    DrawObjectLocateButton(material.path);
                                    EditorGUILayout.EndHorizontal();
                                }
                            }
                        }
                    }
                    #endregion
                    if (shader.showMaterials)
                    {
                        // show materials using this shader that does not use any keywords
                        bool first = true;
                        int matCount = shader.materials.Count;
                        for (int m = 0; m < matCount; m++)
                        {
                            StripMaterial material = shader.materials[m];
                            if (material.keywords.Count == 0)
                            {
                                if (first)
                                {
                                    first = false;
                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.LabelField("", GUILayout.Width(15));
                                    EditorGUILayout.LabelField("使用该着色器但没有使用关键字的材质:");
                                    EditorGUILayout.EndHorizontal();
                                }
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField("", GUILayout.Width(15));
                                EditorGUILayout.LabelField(matIcon, GUILayout.Width(18));
                                EditorGUILayout.LabelField(material.name);
                                DrawObjectLocateButton(material.path);
                                EditorGUILayout.EndHorizontal();
                            }
                        }
                    }
                }
                EditorGUILayout.Separator();

            }
        }
        //绘制内置关键字选择按钮网格
        void DrawBuildInShaderKeywordSelectGrid(UnityEngine.Rendering.ShaderKeywordType keytype, ref List<string> keysList)
        {
            string[] buildinkeys = ShaderHelper.GetBuildinKeyswords(keytype);
            int select = GUILayout.SelectionGrid(-1, buildinkeys, 3);
            if (select >= 0)
            {
                if (!keysList.Contains(buildinkeys[select]))
                {
                    keysList.Add(buildinkeys[select]);
                }
                else
                {
                    keysList.Remove(buildinkeys[select]);
                }
            }
        }

        //绘制白名单曹组
        void DrawShaderWhitelistOption(ref StripShader shader)
        {
            if (!shader.inStripWhiteList)
            {
                if (GUILayout.Button(new GUIContent("添加到白名单", "添加默认过滤配置"), GUILayout.Width(100)))
                {
                    AddDefaultFilter(ref shader);
                    UpdateProjectStats();
                    GUIUtility.ExitGUI();
                }
            }
            if (shader.inStripWhiteList)
            {
                if (GUILayout.Button(new GUIContent("从白名单移除"), GUILayout.Width(100)))
                {
                    RemoveFromWhiteList(ref shader);
                    UpdateProjectStats();
                    GUIUtility.ExitGUI();
                }
                if (GUILayout.Button(new GUIContent("应用剔除关键字", "修改变体剔除白名单关键字"), GUILayout.Width(100)))
                {
                    ApplyStripKeywords(ref shader);
                    UpdateProjectStats();
                    GUIUtility.ExitGUI();
                }
            }
        }
        //绘制定位按钮
        void DrawObjectLocateButton(string path)
        {
            if (GUILayout.Button(new GUIContent("定位", "定位工程文件"), GUILayout.Width(60)))
            {
                UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                Selection.activeObject = obj;
                EditorGUIUtility.PingObject(obj);
            }
        }
        //绘制打开按钮
        void DrawObjectOpenButton(string path)
        {
            if (GUILayout.Button(new GUIContent("打开", "使用默认编辑器打开文件"), GUILayout.Width(60)))
            {
                EditorUtility.OpenWithDefaultApp(path);
            }
        }
        //绘制根据关键字排序的shader列表
        void DrawKeywordSortShaderList()
        {
            if (keywordToShadersList != null)
            {
                int kvCount = keywordToShadersList.Count;
                for (int s = 0; s < kvCount; s++)
                {
                    if (!string.IsNullOrEmpty(keywordFilter) && keywordToShadersList[s].keyword.IndexOf(keywordFilter, StringComparison.CurrentCultureIgnoreCase) < 0)
                        continue;
                    EditorGUILayout.BeginHorizontal();
                    keywordToShadersList[s].foldout = EditorGUILayout.Foldout(keywordToShadersList[s].foldout, new GUIContent("" + s + " 关键字 <b>" + keywordToShadersList[s].keyword + "</b> 被 " + keywordToShadersList[s].shaders.Count + " 个着色器使用"), true, foldoutRTF);
                    EditorGUILayout.EndHorizontal();
                    if (keywordToShadersList[s].foldout)
                    {
                        int kvShadersCount = keywordToShadersList[s].shaders.Count;
                        for (int m = 0; m < kvShadersCount; m++)
                        {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField("", GUILayout.Width(30));
                            EditorGUILayout.LabelField("" + m, GUILayout.Width(30));
                            EditorGUILayout.LabelField(shaderIcon, GUILayout.Width(18));
                            EditorGUILayout.SelectableLabel(keywordToShadersList[s].shaders[m].shaderName, GUILayout.Height(18));
                            StripShader shader = keywordToShadersList[s].shaders[m];
                            GUI.enabled = shader.hasSource;
                            //定位按钮
                            DrawObjectLocateButton(shader.path);
                            GUI.enabled = true;
                            if (GUILayout.Button(new GUIContent("过滤", "显示使用该关键字的着色器"), GUILayout.Width(60)))
                            {
                                keywordFilter = keywordToShadersList[s].keyword;
                                sortType = SORT_TYPE.VariantsCount;
                                EditorGUIUtility.ExitGUI();
                                return;
                            }
                            if (GUILayout.Button(new GUIContent("查看白名单过滤配置"), GUILayout.Width(120)))
                            {
                                selection = shaderList.IndexOf(shader);
                                OnShaderSelectionChange();
                                EditorGUIUtility.ExitGUI();
                                return;
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                }
            }

        }
        //绘制一个shader白名单剔除过滤对象的UI
        void DrawShaderVariantsStripperFilter(ref ShaderVariantsStripperFilter filter, int usedFor = 0)
        {
            GUILayout.BeginHorizontal(blackStyle);
            EditorGUILayout.LabelField("筛选项：", GUILayout.Width(mTitleWiith));
            filter.mask = (MatchLayer)EditorGUILayout.EnumFlagsField(filter.mask, GUILayout.Width(mCommonItemWidth));
            GUILayout.EndHorizontal();
            if ((filter.mask & MatchLayer.Shader) == MatchLayer.Shader)
            {
                GUILayout.BeginHorizontal(blackStyle);
                EditorGUILayout.LabelField("名称：", GUILayout.Width(mTitleWiith));
                EditorGUILayout.LabelField(filter.shaderName, GUILayout.Width(mCommonItemWidth));
                GUILayout.EndHorizontal();
            }
            if ((filter.mask & MatchLayer.ShaderType) == MatchLayer.ShaderType)
            {
                GUILayout.BeginHorizontal(blackStyle);
                EditorGUILayout.LabelField("着色器类型：", GUILayout.Width(mTitleWiith));
                filter.shaderType = (StripShaderType)EditorGUILayout.EnumFlagsField(filter.shaderType, GUILayout.Width(mCommonItemWidth));
                GUILayout.EndHorizontal();
            }
            if ((filter.mask & MatchLayer.PassType) == MatchLayer.PassType)
            {
                GUILayout.BeginHorizontal(blackStyle);
                EditorGUILayout.LabelField("渲染通道类型：", GUILayout.Width(mTitleWiith));
                filter.passType = (StripPassType)EditorGUILayout.EnumFlagsField(filter.passType, GUILayout.Width(mCommonItemWidth));
                GUILayout.EndHorizontal();
            }
            if ((filter.mask & MatchLayer.GraphicsTier) == MatchLayer.GraphicsTier)
            {
                GUILayout.BeginHorizontal(blackStyle);
                EditorGUILayout.LabelField("硬件等级：", GUILayout.Width(mTitleWiith));
                filter.graphicsTier = (StripGraphicsTier)EditorGUILayout.EnumFlagsField(filter.graphicsTier, GUILayout.Width(mCommonItemWidth));
                GUILayout.EndHorizontal();
            }
            if ((filter.mask & MatchLayer.ShaderCompilerPlatform) == MatchLayer.ShaderCompilerPlatform)
            {
                GUILayout.BeginHorizontal(blackStyle);
                EditorGUILayout.LabelField("编译平台：", GUILayout.Width(mTitleWiith));
                filter.shaderCompilerPlatform = (StripShaderCompilerPlatform)EditorGUILayout.EnumFlagsField(filter.shaderCompilerPlatform, GUILayout.Width(mCommonItemWidth));
                GUILayout.EndHorizontal();
            }
            if ((filter.mask & MatchLayer.BuiltinShaderDefine) == MatchLayer.BuiltinShaderDefine)
            {
                GUILayout.BeginHorizontal(blackStyle);
                EditorGUILayout.LabelField("内置定义：", GUILayout.Width(mTitleWiith));
                filter.builtinShaderDefine = (StripBuiltinShaderDefine)EditorGUILayout.EnumFlagsField(filter.builtinShaderDefine, GUILayout.Width(mCommonItemWidth));
                GUILayout.EndHorizontal();
            }
            if ((filter.mask & MatchLayer.ShaderRequirements) == MatchLayer.ShaderRequirements)
            {
                GUILayout.BeginHorizontal(blackStyle);
                EditorGUILayout.LabelField(new GUIContent("着色器特性：", " Required shader features for some particular shader. Features are bit flags."), GUILayout.Width(mTitleWiith));
                filter.shaderRequirements = (UnityEditor.Rendering.ShaderRequirements)EditorGUILayout.EnumFlagsField(filter.shaderRequirements, GUILayout.Width(mCommonItemWidth));
                GUILayout.EndHorizontal();
            }
            if ((filter.mask & MatchLayer.Keywords) == MatchLayer.Keywords)
            {
                GUILayout.BeginHorizontal(blackStyle);
                showKeyword = EditorGUILayout.Foldout(showKeyword, new GUIContent("关键字："), true, foldoutNormal);
                if (showKeyword)
                {
                    GUILayout.BeginVertical(blackStyle);
                    for (int keyindex = 0; keyindex < filter.keywords.Count; keyindex++)
                    {
                        GUILayout.BeginHorizontal(blackStyle);
                        if (GUILayout.Button(new GUIContent("X"), GUILayout.Width(18)))
                        {
                            filter.keywords.RemoveAt(keyindex);
                            GUIUtility.ExitGUI();
                            return;
                        }
                        EditorGUILayout.SelectableLabel(filter.keywords[keyindex], disabledStyle, GUILayout.Height(18), GUILayout.Width(mCommonItemWidth));
                        GUILayout.EndHorizontal();
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndHorizontal();
            }
        }
        void BuildinKeywordInit()
        {
            for (int i = 0; i < buidinKeywordsNames.Length; i++)
            {
                string keyWordName = buidinKeywordsNames[i];
                UnityEngine.Rendering.ShaderKeyword shaderKeyword = new UnityEngine.Rendering.ShaderKeyword(keyWordName);
                if (shaderKeyword != null)
                {
                    bool isUserDefined = shaderKeyword.GetKeywordType() == UnityEngine.Rendering.ShaderKeywordType.UserDefined;
                    bool isBuiltinDefault = shaderKeyword.GetKeywordType() == UnityEngine.Rendering.ShaderKeywordType.BuiltinDefault;
                    bool isBuiltinExtra = shaderKeyword.GetKeywordType() == UnityEngine.Rendering.ShaderKeywordType.BuiltinExtra;
                    bool isBuiltinAutoStripped = shaderKeyword.GetKeywordType() == UnityEngine.Rendering.ShaderKeywordType.BuiltinAutoStripped;
                    bool isNone = shaderKeyword.GetKeywordType() == UnityEngine.Rendering.ShaderKeywordType.None;

                    if (isBuiltinDefault && !buildInDefaultKeys.Contains(keyWordName))
                        buildInDefaultKeys.Add(keyWordName);
                    if (isBuiltinExtra && !buildInExtraKeys.Contains(keyWordName))
                        buildInExtraKeys.Add(keyWordName);
                    if (isBuiltinAutoStripped && !buildInAutoStripKeys.Contains(keyWordName))
                        buildInAutoStripKeys.Add(keyWordName);
                    if (isUserDefined && !userDefineKeys.Contains(keyWordName))
                        userDefineKeys.Add(keyWordName);
                    if (isNone)
                    {
                        Debug.Log("NONE " + keyWordName);
                    }
                }
                else
                {
                    Debug.Log("Failed keyWordName " + keyWordName);
                }
            }
            string defaultkeys = "";
            for (int i = 0; i < buildInDefaultKeys.Count; i++)
            {
                defaultkeys = defaultkeys + "\"" + buildInDefaultKeys[i] + "\",";
            }
            Debug.Log("defaultkeys " + defaultkeys);

            string extrakeys = "";
            for (int i = 0; i < buildInExtraKeys.Count; i++)
            {
                extrakeys = extrakeys + "\"" + buildInExtraKeys[i] + "\",";
            }
            Debug.Log("extrakeys " + extrakeys);

            string autoStripkeys = "";
            for (int i = 0; i < buildInAutoStripKeys.Count; i++)
            {
                autoStripkeys = autoStripkeys + "\"" + buildInAutoStripKeys[i] + "\",";
            }
            Debug.Log("autoStripkeys " + autoStripkeys);

            string userdefineKeys = "";
            for (int i = 0; i < userDefineKeys.Count; i++)
            {
                userdefineKeys = userdefineKeys + "\"" + userDefineKeys[i] + "\",";
            }
            Debug.Log("userdefineKeys " + userdefineKeys);
            Debug.Log("buildInDefaultKeys.Count" + buildInDefaultKeys.Count);
            Debug.Log("buildInExtraKeys.Count" + buildInExtraKeys.Count);
            Debug.Log("buildInAutoStripKeys.Count" + buildInAutoStripKeys.Count);
            Debug.Log("userDefineKeys.Count" + userDefineKeys.Count);
        }
        //扫描工程shader和材质
        void ScanProject()
        {
            try
            {
                if (shaderList == null)
                {
                    shaderList = new List<StripShader>();
                }
                else
                {
                    shaderList.Clear();
                }

                string[] guids = AssetDatabase.FindAssets("t:Shader");
                totalShaderCount = guids.Length;
                //工程所有shader 如果选择扫描全部或者是在Resource文件夹中的shader 会加进包中
                for (int k = 0; k < totalShaderCount; k++)
                {
                    string guid = guids[k];
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (path != null)
                    {
                        string pathUpper = path.ToUpper();
                        if (scanAllShaders || pathUpper.Contains("\\RESOURCES\\") || pathUpper.Contains("/RESOURCES/"))
                        {
                            Shader unityShader = AssetDatabase.LoadAssetAtPath<Shader>(path);
                            if (unityShader != null)
                            {
                                StripShader shader = new StripShader();
                                shader.name = Path.GetFileNameWithoutExtension(path);
                                shader.shaderName = unityShader.name;
                                shader.path = path;
                                string shaderGUID = path + "/" + unityShader.name;
                                shader.GUID = shaderGUID;
                                ScanShader(shader);
                                if (shader.keywords.Count >= 0)
                                {
                                    shaderList.Add(shader);
                                }
                            }
                        }
                    }
                    EditorUtility.DisplayProgressBar("扫描Resource文件夹", path, (float)k / (float)(totalShaderCount - 1));
                }
                EditorUtility.ClearProgressBar();
                //字典存放guid -- StripShader
                Dictionary<string, StripShader> shaderCache = new Dictionary<string, StripShader>(shaderList.Count);
                for (int i = 0; i < shaderList.Count; i++)
                {
                    StripShader shader = shaderList[i];
                    shaderCache.Add(shader.GUID, shader);
                }
                //扫描全部材质
                string[] matGuids = AssetDatabase.FindAssets("t:Material");
                int totaMatCount = matGuids.Length;
                for (int k = 0; k < totaMatCount; k++)
                {
                    string matGUID = matGuids[k];
                    string matPath = AssetDatabase.GUIDToAssetPath(matGUID);
                    Material mat = (Material)AssetDatabase.LoadAssetAtPath<Material>(matPath);
                    if (mat.shader == null)
                        continue;
                    StripMaterial scMat = new StripMaterial(mat.name, matPath, matGUID);
                    scMat.SetKeywords(mat.shaderKeywords);
                    string path = AssetDatabase.GetAssetPath(mat.shader);
                    string shaderGUID = path + "/" + mat.shader.name;
                    StripShader shader;
                    if (shaderCache.ContainsKey(shaderGUID))
                    {
                        shader = shaderCache[shaderGUID];
                    }
                    else
                    {
                        //处理材质相关shader 创建StripShader加入列表 和 字典
                        Shader shad = AssetDatabase.LoadAssetAtPath<Shader>(path);
                        shader = new StripShader();
                        shader.isReadOnly = IsFileWritable(path);
                        shader.GUID = shaderGUID;
                        //材质没有使用关键字 跳过
                        if (mat.shaderKeywords == null || mat.shaderKeywords.Length == 0)
                        {
                            shader.isMatUseKeyword = false;
                        }
                        else
                        {
                            shader.isMatUseKeyword = true;
                        }

                        if (shad != null)
                        {
                            shader.name = Path.GetFileNameWithoutExtension(path);
                            shader.shaderName = shad.name;
                            shader.path = path;
                            ScanShader(shader);
                        }
                        else
                        {
                            shader.name = mat.shader.name;
                            shader.shaderName = mat.shader.name;
                        }
                        shaderList.Add(shader);
                        shaderCache.Add(shaderGUID, shader);
                        totalShaderCount++;
                    }
                    //添加shader关联材质
                    shader.materials.Add(scMat);
                    //添加shader 材质使用的关键字
                    shader.AddKeywordsByName(mat.shaderKeywords);

                    EditorUtility.DisplayProgressBar("扫描材质", matPath, (float)k / (float)(totaMatCount - 1));
                }

                //更新统计数据
                maxKeywordsCountFound = 0;
                for (int i = 0; i < shaderList.Count; i++)
                {
                    StripShader shader = shaderList[i];
                    shader.UpdateVariantCount();
                    if (shader.keywordEnabledCount > maxKeywordsCountFound)
                    {
                        maxKeywordsCountFound = shader.keywordEnabledCount;
                    }
                }
                EditorUtility.ClearProgressBar();
                SortShaderList();
                UpdateProjectStats();
            }
            catch (Exception ex)
            {
                EditorUtility.ClearProgressBar();
                Debug.LogError("扫描工程错误: " + ex.Message);
            }
        }
        //扫描解析一个shader
        void ScanShader(StripShader stripShader)
        {
            ShaderHelper.ScanShader(ref stripShader);
        }
        //对list排序
        void SortShaderList()
        {
            switch (sortType)
            {
                case SORT_TYPE.VariantsCount:
                    shaderList.Sort((StripShader x, StripShader y) => {
                        return y.totalVariantCount.CompareTo(x.totalVariantCount);
                    });
                    break;
                case SORT_TYPE.EnabledKeywordsCount:
                    shaderList.Sort((StripShader x, StripShader y) => {
                        return y.keywordEnabledCount.CompareTo(x.keywordEnabledCount);
                    });
                    break;
                case SORT_TYPE.ShaderFileName:
                    shaderList.Sort((StripShader x, StripShader y) => {
                        return x.shaderName.CompareTo(y.shaderName);
                    });
                    break;
            }
        }
        void UpdateShader(ref StripShader shader)
        {
           // ShaderHelper.UpdateShader(ref shader);
        }

        void RestoreShader(ref StripShader shader)
        {
          //  ShaderHelper.RestoreShader(ref shader);
        }
        //更新统计数据
        void UpdateProjectStats()
        {
            totalKeywords = 0;
            totalUsedKeywords = 0;
            totalVariants = 0;
            totalBuildVariants = 0;
            totalInWhitelist = 0;
            if (shaderList == null)
                return;
            if (uniqueKeywords == null)
                uniqueKeywords = new Dictionary<string, List<StripShader>>();
            else
                uniqueKeywords.Clear();
            if (uniqueEnabledKeywords == null)
                uniqueEnabledKeywords = new Dictionary<string, List<StripShader>>();
            else
                uniqueEnabledKeywords.Clear();

            int shadersCount = shaderList.Count;
            for (int k = 0; k < shadersCount; k++)
            {
                StripShader shader = shaderList[k];
                int keywordsCount = shader.keywords.Count;
                for (int w = 0; w < keywordsCount; w++)
                {
                    StripKeyword keyword = shader.keywords[w];
                    List<StripShader> shadersWithThisKeyword;
                    if (!uniqueKeywords.TryGetValue(keyword.name, out shadersWithThisKeyword))
                    {
                        shadersWithThisKeyword = new List<StripShader>();
                        uniqueKeywords[keyword.name] = shadersWithThisKeyword;
                        totalKeywords++;
                    }
                    shadersWithThisKeyword.Add(shader);
                    if (keyword.enabled)
                    {
                        List<StripShader> shadersWithThisKeywordEnabled;
                        if (!uniqueEnabledKeywords.TryGetValue(keyword.name, out shadersWithThisKeywordEnabled))
                        {
                            shadersWithThisKeywordEnabled = new List<StripShader>();
                            uniqueEnabledKeywords[keyword.name] = shadersWithThisKeywordEnabled;
                            totalUsedKeywords++;
                        }
                        shadersWithThisKeywordEnabled.Add(shader);
                    }
                }
                totalVariants += shader.totalVariantCount;
                totalBuildVariants += shader.actualBuildVariantCount;
                if (GetShaderFilterItem(ref shader) == null)
                {
                    shader.inStripWhiteList = false;
                }
                else
                {
                    shader.inStripWhiteList = true;
                    totalInWhitelist++;
                }
            }
            if (keywordToShadersList == null)
            {
                keywordToShadersList = new List<KeywordShaderRelation>();
            }
            else
            {
                keywordToShadersList.Clear();
            }
            foreach (KeyValuePair<string, List<StripShader>> kvp in uniqueEnabledKeywords)
            {
                KeywordShaderRelation kv = new KeywordShaderRelation { keyword = kvp.Key, shaders = kvp.Value };
                keywordToShadersList.Add(kv);
            }
            keywordToShadersList.Sort(delegate (KeywordShaderRelation x, KeywordShaderRelation y) {
                return y.shaders.Count.CompareTo(x.shaders.Count);
            });
        }
        //生成默认白名单
        void GenWhiteList()
        {
            if (shaderList != null && currentStripConfigure != null)
            {
                List<ShaderVariantsStripperFilter> whitelist = currentStripConfigure.GetWhitelist();
                if (shaderGUIDToWhitelistIndex == null)
                {
                    shaderGUIDToWhitelistIndex = new Dictionary<string, int>();
                }
                else
                {
                    shaderGUIDToWhitelistIndex.Clear();
                }
                whitelist.Clear();
                for (int i = 0; i < shaderList.Count; i++)
                {
                    StripShader shader = shaderList[i];
                    if(shader.showing && shader.selected)
                        AddDefaultFilter(ref shader);
                }
            }
        }
        //删除白名单
        void RemoveWhiteList()
        {
            if (shaderList != null && currentStripConfigure != null)
            {
                for (int i = 0; i < shaderList.Count; i++)
                {
                    StripShader shader = shaderList[i];
                    if (shader.showing && shader.selected)
                    {
                        RemoveFromWhiteList(ref shader);
                    }
                }
            }
        }
        //设置stripShader 和 白名单的映射字典
        void SetShaderGUIDToWhitelistIndex(string shaderName, int index)
        {
            if (shaderGUIDToWhitelistIndex == null)
            {
                shaderGUIDToWhitelistIndex = new Dictionary<string, int>();
            }
            if (shaderGUIDToWhitelistIndex.ContainsKey(shaderName))
            {
                shaderGUIDToWhitelistIndex[shaderName] = index;
            }
            else
            {
                shaderGUIDToWhitelistIndex.Add(shaderName, index);
            }
        }
        void RemoveShaderGUIDToWhitelistIndex(string shaderName)
        {
            if (shaderGUIDToWhitelistIndex != null)
            {
                if (shaderGUIDToWhitelistIndex.ContainsKey(shaderName))
                {
                    shaderGUIDToWhitelistIndex.Remove(shaderName);
                }
            }
        }
        //获取shader在白名中的index
        int GetShaderWhitelistIndex(string guid)
        {
            if (shaderGUIDToWhitelistIndex == null)
            {
                return -1;
            }
            if (shaderGUIDToWhitelistIndex.ContainsKey(guid))
            {
                int index = shaderGUIDToWhitelistIndex[guid];
                List<ShaderVariantsStripperFilter> whitelist = currentStripConfigure.GetWhitelist();
                if (whitelist == null || index >= whitelist.Count)
                    return -1;
                if (whitelist[index].shaderGuid != guid)
                    return -1;
                return index;
            }
            return -1;
        }
        //获取白名单过滤项
        ShaderVariantsStripperFilter GetShaderFilterItem(ref StripShader shader)
        {
            if (shaderList != null && currentStripConfigure != null)
            {
                List<ShaderVariantsStripperFilter> whitelist = currentStripConfigure.GetWhitelist();
                bool find = false;
                int index = GetShaderWhitelistIndex(shader.GUID);
                if (index == -1)
                {
                    for (int i = 0; i < whitelist.Count; i++)
                    {
                        ShaderVariantsStripperFilter afilter = whitelist[i];
                        if (afilter.shaderName == shader.shaderName && afilter.shaderGuid == shader.GUID)
                        {
                            find = true;
                            index = i;
                            break;
                        }
                    }
                }
                else
                {
                    find = true;
                }
                if (find)
                {
                    shader.inStripWhiteList = true;
                    SetShaderGUIDToWhitelistIndex(shader.GUID, index);
                    ShaderVariantsStripperFilter filter = whitelist[index];
                    return filter;
                }
                return null;
            }
            return null;
        }
        //添加默认过滤项  默认模板须要配置
        void AddDefaultFilter(ref StripShader shader)
        {
            if (shaderList != null && currentStripConfigure != null)
            {
                List<ShaderVariantsStripperFilter> whitelist = currentStripConfigure.GetWhitelist();
                ShaderVariantsStripperFilter filter = new ShaderVariantsStripperFilter();

                //默认只过滤shader名和关键字
                filter.mask = (MatchLayer)DefaultShaderVariantsStripperFilter.mask;
                filter.shaderName = shader.shaderName;

                filter.shaderType = DefaultShaderVariantsStripperFilter.shaderType;
                filter.passType = DefaultShaderVariantsStripperFilter.passType;
                filter.graphicsTier = DefaultShaderVariantsStripperFilter.graphicsTier;
                filter.shaderRequirements = DefaultShaderVariantsStripperFilter.shaderRequirements;
                filter.shaderCompilerPlatform = DefaultShaderVariantsStripperFilter.shaderCompilerPlatform;
                filter.builtinShaderDefine = DefaultShaderVariantsStripperFilter.builtinShaderDefine;

                filter.shaderGuid = shader.GUID;
                filter.keywords = new List<string>();
                for (int k = 0; k < shader.enabledKeywords.Count; k++)
                {
                    filter.keywords.Add(shader.enabledKeywords[k]);
                }
                SetShaderGUIDToWhitelistIndex(shader.GUID, whitelist.Count);
                whitelist.Add(filter);
                shader.inStripWhiteList = true;
            }
        }
        //从白名单移除
        void RemoveFromWhiteList(ref StripShader shader)
        {
            if (shaderList != null && currentStripConfigure != null)
            {
                List<ShaderVariantsStripperFilter> whitelist = currentStripConfigure.GetWhitelist();
                int index = GetShaderWhitelistIndex(shader.GUID);
                if (index >= 0)
                {
                    RemoveShaderGUIDToWhitelistIndex(shader.GUID);
                    whitelist.RemoveAt(index);
                    shader.inStripWhiteList = false;
                }else
                {
                    for (int i = 0; i < whitelist.Count; i++)
                    {
                        ShaderVariantsStripperFilter afilter = whitelist[i];
                        if (afilter.shaderName == shader.shaderName && afilter.shaderGuid == shader.GUID)
                        {
                            index = i;
                            break;
                        }
                    }
                    if (index >= 0)
                    {
                        RemoveShaderGUIDToWhitelistIndex(shader.GUID);
                        whitelist.RemoveAt(index);
                        shader.inStripWhiteList = false;
                    }
                }
              
            }
        }
        //应用shader关键字
        void ApplyStripKeywords(ref StripShader shader)
        {
            ShaderVariantsStripperFilter filter = GetShaderFilterItem(ref shader);
            if (filter != null)
            {
                filter.keywords.Clear();
                for (int k = 0; k < shader.enabledKeywords.Count; k++)
                {
                    filter.keywords.Add(shader.enabledKeywords[k]);
                }
            }
        }
        //选中shader改变
        void OnShaderSelectionChange()
        {
            addingKeyword = false;
            showKeyword = false;
            editorstring = "";
        }
        #region 材质操作
        //关闭材质使用但是shader中没有生效的关键字
        void CleanMaterials(StripShader shader)
        {
            Shader shad = (Shader)AssetDatabase.LoadAssetAtPath<Shader>(shader.path);
            if (shad != null)
            {
                bool requiresSave = false;
                string[] matGUIDs = AssetDatabase.FindAssets("t:Material");
                foreach (string matGUID in matGUIDs)
                {
                    string matPath = AssetDatabase.GUIDToAssetPath(matGUID);
                    Material mat = (Material)AssetDatabase.LoadAssetAtPath<Material>(matPath);
                    if (mat != null && mat.shader.name.Equals(shad.name))
                    {
                        foreach (StripKeyword keyword in shader.keywords)
                        {
                            foreach (string matKeyword in mat.shaderKeywords)
                            {
                                if (matKeyword.Equals(keyword.name))
                                {
                                    //关闭材质使用但是shader中没有生效的关键字
                                    if (!keyword.enabled && mat.IsKeywordEnabled(keyword.name))
                                    {
                                        mat.DisableKeyword(keyword.name);
                                        EditorUtility.SetDirty(mat);
                                        requiresSave = true;
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
                if (requiresSave)
                {
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }
        }

        void CleanAllMaterials()
        {
            if (!EditorUtility.DisplayDialog("清理材质", "This option will scan all materials and will prune any disabled keywords. This option is provided to ensure no materials are referencing a disabled shader keyword.\n\nRemember: to disable keywords, first expand any shader from the list and uncheck the unwanted keywords (press 'Save' to modify the shader file and to clean any existing material that uses that specific shader).\n\nDo you want to continue?", "Yes", "Cancel"))
            {
                return;
            }
            try
            {
                for (int k = 0; k < shaderList.Count; k++)
                {
                    CleanMaterials(shaderList[k]);
                }
                ScanProject();
                Debug.Log("清理完毕");
            }
            catch (Exception ex)
            {
                Debug.LogError("Unexpected exception caught while cleaning materials: " + ex.Message);
            }
        }
        //裁剪材质中的关键字 DisableKeyword
        void PruneMaterials(StripShader shader, string keywordName)
        {
            try
            {
                bool requiresSave = false;
                int materialCount = shader.materials.Count;
                for (int k = 0; k < materialCount; k++)
                {
                    StripMaterial material = shader.materials[k];
                    if (material.ContainsKeyword(keywordName))
                    {
                        Material theMaterial = (Material)AssetDatabase.LoadAssetAtPath<Material>(shader.materials[k].path);
                        if (theMaterial == null)
                            continue;
                        theMaterial.DisableKeyword(keywordName);
                        EditorUtility.SetDirty(theMaterial);
                        material.RemoveKeyword(keywordName);
                        shader.RemoveKeyword(keywordName);
                        requiresSave = true;
                    }
                }
                if (requiresSave)
                {
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }
            catch (Exception ex)
            {
                Debug.Log("Unexpected exception caught while pruning materials: " + ex.Message);
            }

        }

        #endregion
        bool IsFileWritable(string path)
        {
            FileStream stream = null;
            try
            {
                FileAttributes fileAttributes = File.GetAttributes(path);
                if ((fileAttributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    return true;
                }
                FileInfo file = new FileInfo(path);
                stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
            //file is not locked
            return false;
        }
    }
}