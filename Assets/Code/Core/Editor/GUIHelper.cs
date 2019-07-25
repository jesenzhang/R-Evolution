using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum GUIStyleEnum
{
    BLACKSTYLE= 0,
    COMMENTSTYLE,
    DISABLEDSTYLE,
    FOLDOUTBOLD,
    FOLDOUTNORMAL,
    FOLDOUTDIM,
    FOLDOUTRTF,
    BUTTONNORMAL,
    BUTTONSELECTED,
    STATESTYLE,
    MIDDLETITLE,
}

public enum GUIContentEnum
{
    MATERIALICON = 0,
    SHADERICON,
}

public class GUIHelper
{
    public static string LastFoldPath = Application.dataPath;
    public static string LastFilePath = Application.dataPath;
    public static Rect TempRect = new Rect();
    public static int FontSize = 12;
    public static GUIContent TempContent = new GUIContent();
    public static List<string> TempStringList = new List<string>();

    private static Dictionary<string, bool> cacheBoolState = new Dictionary<string, bool>();
    private static Dictionary<string, int> cacheIntState = new Dictionary<string, int>();

    public delegate void OnSelectChanged(int value);
    public delegate void OnCheckChanged(bool value);

    public static void RemoveCacheIntState(string key)
    {
        if (cacheIntState.ContainsKey(key))
        {
            cacheIntState.Remove(key);
        }
    }

    public static void RemoveCacheBoolState(string key)
    {
        if (cacheBoolState.ContainsKey(key))
        {
            cacheBoolState.Remove(key);
        }
    }

    public static void CleanCache()
    {
        if (cacheBoolState != null)
            cacheBoolState.Clear();
        if (cacheIntState != null)
            cacheIntState.Clear();
    }

    #region GUI
    private GenericMenu m_operationMenu;
    //UI样式
    public static GUIStyle blackStyle, middleTitleStyle, commentStyle, disabledStyle, foldoutBold, foldoutNormal, foldoutDim, foldoutRTF, buttonNormal, buttonSelected, stateStyle;
    //图标
    public static GUIContent matIcon, shaderIcon;

    private Color m_preColor;
    //左侧shader列表ScrollView位置
    Vector2 scrollViewPos;

    float mTitleWiith = 100;
    float mCommonItemWidth = 300;
    //剔除滑动界面
    Vector2 stripScrollViewPos;
    #endregion
    static Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++)
            pix[i] = col;
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }

    public static GUIContent GetContent(GUIContentEnum guitype)
    {
        switch (guitype)
        {
            case GUIContentEnum.MATERIALICON:
                {
                    if (matIcon == null)
                    {
                        matIcon = EditorGUIUtility.IconContent("PreMatSphere");
                        if (matIcon == null)
                            matIcon = new GUIContent();
                    }
                    return matIcon;
                };
            case GUIContentEnum.SHADERICON:
                {
                    if (shaderIcon == null)
                    {
                        shaderIcon = EditorGUIUtility.IconContent("Shader Icon");
                        if (shaderIcon == null)
                            shaderIcon = new GUIContent();
                    }
                    return shaderIcon;
                };
            default:
                return null;
        }


    }

    public static GUIStyle GetStyle(GUIStyleEnum gUIStyle)
    {
        switch (gUIStyle)
        {
            case GUIStyleEnum.BLACKSTYLE:
                {
                    if (blackStyle == null)
                    {
                        Color backColor = EditorGUIUtility.isProSkin ? new Color(0.18f, 0.18f, 0.18f) : new Color(0.7f, 0.7f, 0.7f);
                        Texture2D _blackTexture;
                        _blackTexture = MakeTex(4, 4, backColor);
                        _blackTexture.hideFlags = HideFlags.DontSave;
                        blackStyle = new GUIStyle();
                        blackStyle.normal.background = _blackTexture;
                    }
                    return blackStyle;
                };
            case GUIStyleEnum.COMMENTSTYLE:
                {
                    if (commentStyle == null)
                    {
                        commentStyle = new GUIStyle(EditorStyles.label);
                        commentStyle.normal.textColor = EditorGUIUtility.isProSkin ? new Color(0.62f, 0.76f, 0.9f) : new Color(0.32f, 0.36f, 0.42f);
                        commentStyle.alignment = TextAnchor.MiddleCenter;
                    }
                    return commentStyle;
                };
            case GUIStyleEnum.MIDDLETITLE:
                {
                    if (middleTitleStyle == null)
                    {
                        middleTitleStyle = new GUIStyle(EditorStyles.label);
                        middleTitleStyle.normal.textColor = EditorGUIUtility.isProSkin ? new Color(0.62f, 0.76f, 0.9f) : new Color(0.32f, 0.36f, 0.42f);
                        middleTitleStyle.alignment = TextAnchor.MiddleCenter;
                    }
                    return middleTitleStyle;
                };
            case GUIStyleEnum.DISABLEDSTYLE:
                {
                    if (disabledStyle == null)
                    {
                        disabledStyle = new GUIStyle(EditorStyles.label);
                        disabledStyle.normal.textColor = EditorGUIUtility.isProSkin ? new Color(0.52f, 0.66f, 0.8f) : new Color(0.32f, 0.32f, 0.32f);
                    }
                    return disabledStyle;
                };
            case GUIStyleEnum.FOLDOUTRTF:
                {
                    if (foldoutRTF == null)
                    {
                        foldoutRTF = new GUIStyle(EditorStyles.foldout);
                        foldoutRTF.richText = true;
                    }
                    return foldoutRTF;
                };
            case GUIStyleEnum.FOLDOUTBOLD:
                {
                    if (foldoutBold == null)
                    {
                        foldoutBold = new GUIStyle(EditorStyles.foldout);
                        foldoutBold.fontStyle = FontStyle.Bold;
                    }
                    return foldoutBold;
                };
            case GUIStyleEnum.FOLDOUTNORMAL:
                {
                    if (foldoutNormal == null)
                    {
                        foldoutNormal = new GUIStyle(EditorStyles.foldout);
                    }
                    return foldoutNormal;
                };
            case GUIStyleEnum.FOLDOUTDIM:
                {
                    if (foldoutDim == null)
                    {
                        foldoutDim = new GUIStyle(EditorStyles.foldout);
                        foldoutDim.fontStyle = FontStyle.Italic;
                    }
                    return foldoutDim;
                };
            case GUIStyleEnum.BUTTONNORMAL:
                {
                    if (buttonNormal == null)
                    {
                        buttonNormal = new GUIStyle(EditorStyles.foldout);
                        buttonNormal.fontStyle = FontStyle.Normal;
                        buttonNormal.alignment = TextAnchor.MiddleLeft;
                    }
                    return buttonNormal;
                };
            case GUIStyleEnum.BUTTONSELECTED:
                {
                    if (buttonSelected == null)
                    {
                        buttonSelected = new GUIStyle(EditorStyles.foldout);
                        buttonSelected.fontStyle = FontStyle.Normal;
                        buttonSelected.alignment = TextAnchor.MiddleLeft;
                    }
                    return buttonSelected;
                };
            case GUIStyleEnum.STATESTYLE:
                {
                    if (stateStyle == null)
                    {
                        stateStyle = new GUIStyle(EditorStyles.helpBox);
                        stateStyle.fontSize = 10;
                    }
                    return stateStyle;
                };
            default:
                return null;

        }
    }

    public static void DrawFolderPick(string title, ref string path, string defaultPath = "", int titleWidth = 80, string tooltip = "")
    {
        GUILayout.BeginHorizontal();
        TempContent.text = title;
        TempContent.tooltip = tooltip;
        if (path == string.Empty || path == "")
        {
            path = defaultPath;
        }
        EditorGUILayout.LabelField(TempContent, GUILayout.MaxWidth(titleWidth));
        path = EditorGUILayout.TextField(path);
        if (GUILayout.Button("...", GUILayout.Width(30)))
        {
            LastFoldPath = EditorUtility.OpenFolderPanel(title, LastFoldPath, "");
            path = LastFoldPath;
            GUIUtility.ExitGUI();
            return;
        }
        GUILayout.EndHorizontal();
    }
    public static void DrawFolderPick(string title, ref string path, int titleWidth = 80, string tooltip = "")
    {
        DrawFolderPick(title, ref path, "", titleWidth, tooltip);
    }
    public static void DrawFolderPick(string title, ref string path,string defaultPath = "")
    {
        DrawFolderPick(title, ref path,defaultPath, 80, "");
    }
    public static void DrawFolderPick(string title, ref string path)
    {
        DrawFolderPick(title, ref path, "", 80, "");
    }
    public static void DrawFilePick(string title, ref string path, string filter = "", string defaultPath = "", int titleWidth = 80, string tooltip = "")
    {
        GUILayout.BeginHorizontal();
        TempContent.text = title;
        TempContent.tooltip = tooltip;
        if (path == string.Empty || path == "")
        {
            path = defaultPath;
        }
        EditorGUILayout.LabelField(TempContent, GUILayout.MaxWidth(titleWidth));
        path = EditorGUILayout.TextField(path);
        if (GUILayout.Button("...", GUILayout.Width(30)))
        {
            LastFilePath = EditorUtility.OpenFilePanel(title, LastFilePath, filter);
            path = LastFilePath;
            GUIUtility.ExitGUI();
            return;
        }
        GUILayout.EndHorizontal();
    }
    public static void DrawFilePick(string title, ref string path, string filter = "", int titleWidth = 80, string tooltip = "")
    {
        DrawFilePick(title, ref path, filter = "", "", titleWidth, tooltip);
    }
    public static void DrawFilePick(string title, ref string path)
    {
        DrawFilePick(title, ref path,"", "", 80, "");
    }

    public static void DrawIntField(string title,ref int value, int titleWidth = 80, string tooltip = "", params GUILayoutOption[] option)
    {
        GUILayout.BeginHorizontal();
        TempContent.text = title;
        TempContent.tooltip = tooltip;
        EditorGUILayout.LabelField(TempContent, GUILayout.MaxWidth(titleWidth));
        value = EditorGUILayout.DelayedIntField(value,option);
        GUILayout.EndHorizontal();
    }

    public static void DrawLabel(string title,string value,int titleWidth = 80, string tooltip = "", params GUILayoutOption[] option)
    {
        GUILayout.BeginHorizontal();
        TempContent.text = title;
        TempContent.tooltip = tooltip;
        EditorGUILayout.LabelField(TempContent, GUILayout.MaxWidth(titleWidth));
        GUILayout.Label(value, option);
        GUILayout.EndHorizontal();
    }
    public static void DrawTextField(string title, ref string value, string defaultValue = "", int titleWidth = 80, string tooltip = "", params GUILayoutOption[] option)
    {
        GUILayout.BeginHorizontal();
        TempContent.text = title;
        TempContent.tooltip = tooltip;
        if (value == string.Empty)
        {
            value = defaultValue;
        }
        EditorGUILayout.LabelField(TempContent, GUILayout.MaxWidth(titleWidth));
        value = EditorGUILayout.DelayedTextField(value, option);
        GUILayout.EndHorizontal();
    }
    public static void DrawTextField(string title, ref string value)
    {
        DrawTextField(title, ref value, "", 80, "");
    }
    public static void DrawTextField(string title, ref string value, int titleWidth = 80, string tooltip = "", params GUILayoutOption[] option)
    {
        DrawTextField(title, ref value, "", titleWidth, tooltip, option);
    }
    
    public static bool DrawFold(string key,string title,int titleWidth = 80, string tooltip = "",bool toggleOnLabel=true, GUIStyle uIStyle = null)
    {
        GUILayout.BeginHorizontal();
        bool value = false;
        if (!cacheBoolState.ContainsKey(key))
        {
            cacheBoolState.Add(key, false);
        }
        else
        {
            value = cacheBoolState[key];
        }
        TempContent.text = title;
        TempContent.tooltip = tooltip;
        cacheBoolState[key] = EditorGUILayout.Foldout(value, TempContent, toggleOnLabel, uIStyle==null?GetStyle(GUIStyleEnum.FOLDOUTNORMAL): uIStyle);
        GUILayout.EndHorizontal();
        return cacheBoolState[key];
    }

    public static void DrawButton(string title,Action onClick, GUIStyle uIStyle = null, params GUILayoutOption[] option)
    {
        GUILayout.BeginHorizontal();
        TempContent.text = title;
        if (uIStyle == null? GUILayout.Button(TempContent, option):GUILayout.Button(TempContent, uIStyle, option))
        {
            onClick?.Invoke();
            GUIUtility.ExitGUI();
        }
        GUILayout.EndHorizontal();
    }

    public static void DrawToolbar(string key,string[] titles, int defaultValue = 0, OnSelectChanged onClick = null,GUIStyle uIStyle = null, params GUILayoutOption[] option)
    {
        GUILayout.BeginHorizontal();
        int select = defaultValue;
        if (!cacheIntState.ContainsKey(key))
        {
            cacheIntState.Add(key, select);
        }
        else
        {
            select = cacheIntState[key];
        }
        cacheIntState[key] = uIStyle == null ? GUILayout.Toolbar(select, titles, option) : GUILayout.Toolbar(select, titles, uIStyle, option);
        if(select!= cacheIntState[key])
        {
            onClick?.Invoke(cacheIntState[key]);
            GUIUtility.ExitGUI();
        }
        GUILayout.EndHorizontal();
    }

   
    public static void DrawToggle(string key, string title, bool defaultValue = false, OnCheckChanged onClick = null, int titleWidth = 80, string tooltip = "", GUIStyle uIStyle = null, params GUILayoutOption[] option)
    {
        GUILayout.BeginHorizontal();
        TempContent.text = title;
        TempContent.tooltip = tooltip;
        EditorGUILayout.LabelField(TempContent, GUILayout.MaxWidth(titleWidth));
        bool value = defaultValue;
        if (!cacheBoolState.ContainsKey(key))
        {
            cacheBoolState.Add(key, value);
        }
        else
        {
            value = cacheBoolState[key];
        }
        cacheBoolState[key] = uIStyle == null ? EditorGUILayout.Toggle(value, option) : EditorGUILayout.Toggle(value, uIStyle, option);
        if (value != cacheBoolState[key])
        {
            onClick?.Invoke(cacheBoolState[key]);
            GUIUtility.ExitGUI();
        }
        GUILayout.EndHorizontal();
    }
    public static void DrawToggle(string title, ref bool value, OnCheckChanged onClick = null, int titleWidth = 80, string tooltip = "", GUIStyle uIStyle = null, params GUILayoutOption[] option)
    {
        GUILayout.BeginHorizontal();
        TempContent.text = title;
        TempContent.tooltip = tooltip;
        EditorGUILayout.LabelField(TempContent, GUILayout.MaxWidth(titleWidth));
        bool nvalue = uIStyle == null ? EditorGUILayout.Toggle(value, option) : EditorGUILayout.Toggle(value, uIStyle, option);
        if (value != nvalue)
        {
            value = nvalue;
            onClick?.Invoke(value);
            GUIUtility.ExitGUI();
        }
        GUILayout.EndHorizontal();
    }
    public static void DrawToggleLeft(string key, string title, bool defaultValue = false, OnCheckChanged onClick = null, int toggleWidth = 20, string tooltip = "", GUIStyle uIStyle = null, params GUILayoutOption[] option)
    {
        GUILayout.BeginHorizontal();
        TempContent.text = title;
        TempContent.tooltip = tooltip;
        bool value = defaultValue;
        if (!cacheBoolState.ContainsKey(key))
        {
            cacheBoolState.Add(key, value);
        }
        else
        {
            value = cacheBoolState[key];
        }
        cacheBoolState[key] = uIStyle == null ? EditorGUILayout.Toggle(value, GUILayout.MaxWidth(toggleWidth)) : EditorGUILayout.Toggle(value, uIStyle, GUILayout.MaxWidth(toggleWidth));
        EditorGUILayout.LabelField(TempContent, option);
        if (value != cacheBoolState[key])
        {
            onClick?.Invoke(cacheBoolState[key]);
            GUIUtility.ExitGUI();
        }
        GUILayout.EndHorizontal();
    }

    public static void DrawToggleLeft(string title, ref bool value, OnCheckChanged onClick = null, int toggleWidth = 20, string tooltip = "", GUIStyle uIStyle = null, params GUILayoutOption[] option)
    {
        GUILayout.BeginHorizontal();
        TempContent.text = title;
        TempContent.tooltip = tooltip;
        bool nvalue = uIStyle == null ? EditorGUILayout.Toggle(value, GUILayout.MaxWidth(toggleWidth)) : EditorGUILayout.Toggle(value, uIStyle, GUILayout.MaxWidth(toggleWidth));
        EditorGUILayout.LabelField(TempContent, option );
        if (value != nvalue)
        {
            value = nvalue;
            onClick?.Invoke(value);
            GUIUtility.ExitGUI();
        }
        GUILayout.EndHorizontal();
    }

    
    public static void DrawDeleteItem(string title, Action onClick, string tooltip = "", GUIStyle uIStyle = null, params GUILayoutOption[] option)
    {
        GUILayout.BeginHorizontal();
        TempContent.text = title;
        TempContent.tooltip = tooltip;
        if (uIStyle == null ? GUILayout.Button("X", GUILayout.Width(18)) : GUILayout.Button("X", uIStyle, GUILayout.Width(18)))
        {
            onClick?.Invoke();
            GUIUtility.ExitGUI();
        }
        GUILayout.Label(TempContent, option);
        GUILayout.EndHorizontal();
    }

    public static System.Enum DrawEnumPopup(string title, System.Enum value, int titleWidth = 80, string tooltip = "", params GUILayoutOption[] option)
    {
        GUILayout.BeginHorizontal();
        TempContent.text = title;
        TempContent.tooltip = tooltip;
        EditorGUILayout.LabelField(TempContent, GUILayout.MaxWidth(titleWidth));
        System.Enum nvalue = EditorGUILayout.EnumPopup(value, option);
        GUILayout.EndHorizontal();
        return nvalue;
    }


    public static int DrawMaskField(string title,int mask,Type type,int titleWidth = 80, string tooltip = "", GUIStyle uIStyle = null, params GUILayoutOption[] option)
    {
        GUILayout.BeginHorizontal();
        TempContent.text = title;
        TempContent.tooltip = tooltip;
        mask = uIStyle == null ? EditorGUILayout.MaskField(TempContent, mask, Enum.GetNames(type), option):EditorGUILayout.MaskField(TempContent, mask, Enum.GetNames(type), uIStyle, option);
        GUILayout.EndHorizontal();
        return mask;
    }

}
