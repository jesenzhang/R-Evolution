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

    #region GUI
    private GenericMenu m_operationMenu;
    //UI样式
    public static GUIStyle blackStyle, middleTitleStyle, commentStyle, disabledStyle, foldoutBold, foldoutNormal, foldoutDim, foldoutRTF, buttonNormal, buttonSelected, stateStyle;
    //图标
    public static  GUIContent matIcon, shaderIcon;

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

    public static void DrawFolderPick(string title,ref string path)
    {
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(title, GUILayout.MaxWidth(80));
        path = EditorGUILayout.TextField(path);
        if (GUILayout.Button("...", GUILayout.Width(30)))
        {
            LastFoldPath = EditorUtility.OpenFolderPanel("FlactcPath", LastFoldPath, "");
            path = LastFoldPath;
            GUIUtility.ExitGUI();
            return;
        }
        GUILayout.EndHorizontal();
    }

    public static void DrawFilePick(string title, ref string path,string filter = "")
    {
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(title, GUILayout.MaxWidth(80));
        path = EditorGUILayout.TextField(path);
        if (GUILayout.Button("...", GUILayout.Width(30)))
        {
            LastFilePath = EditorUtility.OpenFilePanel("FlactcPath", LastFilePath, filter);
            path = LastFilePath;
            GUIUtility.ExitGUI();
            return;
        }
        GUILayout.EndHorizontal();
    }
}
