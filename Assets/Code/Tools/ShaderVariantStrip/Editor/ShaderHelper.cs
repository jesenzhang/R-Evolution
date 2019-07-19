using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public enum ShaderLineType
{
    NONE = 0,
    MULTI_COMPILE =1,
    SHADER_FEATURE = 2,
    MULTI_COMPILE_LOCAL =3,
    SHADER_FEATURE_LOCAL = 4,
    MULTI_COMPILE_FWDBASE = 5,
    MULTI_COMPILE_FWDADD = 6,
    MULTI_COMPILE_FWDBASE_FULLSHADOWS = 7,
    MULTI_COMPILE_FOG = 8,
    SKIP_VARIANTS = 9,
    ONLY_RENDERERS = 10,
    EXCLUDE_RENDERERS = 11,
    MULTI_COMPILE_INSTANCING = 12,
    MULTI_COMPILE_SHADOWCASTER =13,
}

public enum EditorLineType
{
    NONE = 0,
    COMMENT_MARK = 1,
    DISABLED_MARK = 2,
}

public class ShaderHelper
{
    static string[] multi_compile_fwdbase_KeywordsNames = new string[] {
        "DIRECTIONAL","DIRLIGHTMAP_COMBINED","DYNAMICLIGHTMAP_ON","LIGHTMAP_ON","LIGHTMAP_SHADOW_MIXING","LIGHTPROBE_SH","SHADOWS_SCREEN","VERTEXLIGHT_ON","SHADOWS_SHADOWMASK",
            };
    static string[][] multi_compile_fwdbase_Variants = new string[][] {
        new string[] {"DIRECTIONAL"},
        new string[] {"DIRECTIONAL", "LIGHTPROBE_SH"},
        new string[] {"DIRECTIONAL", "LIGHTPROBE_SH", "SHADOWS_SHADOWMASK"},
        new string[] {"DIRECTIONAL", "DYNAMICLIGHTMAP_ON"},
        new string[] {"DIRECTIONAL", "LIGHTMAP_ON"},
        new string[] {"DIRECTIONAL", "DYNAMICLIGHTMAP_ON", "LIGHTMAP_ON"},
        new string[] {"DIRECTIONAL", "LIGHTMAP_ON", "LIGHTPROBE_SH"},
        new string[] {"DIRECTIONAL", "LIGHTMAP_ON", "LIGHTMAP_SHADOW_MIXING"},
        new string[] {"DIRECTIONAL", "DYNAMICLIGHTMAP_ON", "LIGHTMAP_ON", "LIGHTMAP_SHADOW_MIXING"},
        new string[] {"DIRECTIONAL", "LIGHTMAP_ON","LIGHTMAP_SHADOW_MIXING", "LIGHTPROBE_SH"},
        new string[] {"DIRECTIONAL", "DIRLIGHTMAP_COMBINED", "DYNAMICLIGHTMAP_ON"},
        new string[] {"DIRECTIONAL", "DIRLIGHTMAP_COMBINED", "LIGHTMAP_ON"},
        new string[] {"DIRECTIONAL", "DIRLIGHTMAP_COMBINED", "LIGHTMAP_ON","LIGHTPROBE_SH"},
        new string[] {"DIRECTIONAL", "DIRLIGHTMAP_COMBINED", "DYNAMICLIGHTMAP_ON","LIGHTMAP_ON"},
        new string[] {"DIRECTIONAL", "DIRLIGHTMAP_COMBINED", "LIGHTMAP_ON","LIGHTMAP_SHADOW_MIXING"},
        new string[] {"DIRECTIONAL", "DIRLIGHTMAP_COMBINED", "LIGHTMAP_ON","LIGHTMAP_SHADOW_MIXING","LIGHTPROBE_SH"},
        new string[] {"DIRECTIONAL", "DIRLIGHTMAP_COMBINED", "DYNAMICLIGHTMAP_ON","LIGHTMAP_ON","LIGHTMAP_SHADOW_MIXING"},
        new string[] {"DIRECTIONAL", "LIGHTMAP_ON", "SHADOWS_SHADOWMASK"},
        new string[] {"DIRECTIONAL", "DYNAMICLIGHTMAP_ON", "LIGHTMAP_ON","SHADOWS_SHADOWMASK"},
        new string[] {"DIRECTIONAL", "LIGHTMAP_ON", "LIGHTPROBE_SH","SHADOWS_SHADOWMASK"},
        new string[] {"DIRECTIONAL", "DIRLIGHTMAP_COMBINED", "LIGHTMAP_ON","SHADOWS_SHADOWMASK"},
        new string[] {"DIRECTIONAL", "DIRLIGHTMAP_COMBINED", "DYNAMICLIGHTMAP_ON","LIGHTMAP_ON","SHADOWS_SHADOWMASK"},
        new string[] {"DIRECTIONAL", "DIRLIGHTMAP_COMBINED", "LIGHTMAP_ON","LIGHTPROBE_SH","SHADOWS_SHADOWMASK"},
        new string[] {"DIRECTIONAL", "LIGHTMAP_ON", "LIGHTMAP_SHADOW_MIXING","SHADOWS_SHADOWMASK"},
        new string[] {"DIRECTIONAL", "DYNAMICLIGHTMAP_ON", "LIGHTMAP_ON","LIGHTMAP_SHADOW_MIXING","SHADOWS_SHADOWMASK"},
        new string[] {"DIRECTIONAL", "LIGHTMAP_ON", "LIGHTMAP_SHADOW_MIXING","LIGHTPROBE_SH","SHADOWS_SHADOWMASK"},
        new string[] {"DIRECTIONAL", "DIRLIGHTMAP_COMBINED","LIGHTMAP_ON","LIGHTMAP_SHADOW_MIXING","SHADOWS_SHADOWMASK"},
        new string[] {"DIRECTIONAL", "DIRLIGHTMAP_COMBINED","DYNAMICLIGHTMAP_ON","LIGHTMAP_ON","LIGHTMAP_SHADOW_MIXING","SHADOWS_SHADOWMASK"},
        new string[] {"DIRECTIONAL", "DIRLIGHTMAP_COMBINED","LIGHTMAP_ON","LIGHTMAP_SHADOW_MIXING","LIGHTPROBE_SH","SHADOWS_SHADOWMASK"},
        new string[] {"DIRECTIONAL", "SHADOWS_SCREEN"},
        new string[] {"DIRECTIONAL", "LIGHTPROBE_SH","SHADOWS_SCREEN"},
        new string[] {"DIRECTIONAL", "DYNAMICLIGHTMAP_ON","SHADOWS_SCREEN"},
        new string[] {"DIRECTIONAL", "LIGHTMAP_SHADOW_MIXING","LIGHTPROBE_SH","SHADOWS_SCREEN"},
        new string[] {"DIRECTIONAL", "DYNAMICLIGHTMAP_ON","LIGHTMAP_SHADOW_MIXING","SHADOWS_SCREEN"},
        new string[] {"DIRECTIONAL", "LIGHTPROBE_SH","SHADOWS_SCREEN","SHADOWS_SHADOWMASK"},
        new string[] {"DIRECTIONAL", "DYNAMICLIGHTMAP_ON","SHADOWS_SCREEN","SHADOWS_SHADOWMASK"},
        new string[] {"DIRECTIONAL", "LIGHTMAP_SHADOW_MIXING","LIGHTPROBE_SH","SHADOWS_SCREEN","SHADOWS_SHADOWMASK"},
        new string[] {"DIRECTIONAL", "DYNAMICLIGHTMAP_ON","LIGHTMAP_SHADOW_MIXING","SHADOWS_SCREEN","SHADOWS_SHADOWMASK"},
        new string[] {"DIRECTIONAL", "LIGHTMAP_ON","SHADOWS_SCREEN"},
        new string[] {"DIRECTIONAL", "DYNAMICLIGHTMAP_ON","LIGHTMAP_ON","SHADOWS_SCREEN"},
        new string[] {"DIRECTIONAL", "LIGHTMAP_ON","LIGHTPROBE_SH","SHADOWS_SCREEN"},
        new string[] {"DIRECTIONAL", "LIGHTMAP_ON","LIGHTMAP_SHADOW_MIXING","SHADOWS_SCREEN"},
        new string[] {"DIRECTIONAL", "DYNAMICLIGHTMAP_ON","LIGHTMAP_ON","LIGHTMAP_SHADOW_MIXING","SHADOWS_SCREEN"},
        new string[] {"DIRECTIONAL", "LIGHTMAP_ON","LIGHTMAP_SHADOW_MIXING","LIGHTPROBE_SH","SHADOWS_SCREEN"},
        new string[] {"DIRECTIONAL", "LIGHTMAP_ON","SHADOWS_SCREEN","SHADOWS_SHADOWMASK"},
        new string[] {"DIRECTIONAL", "DYNAMICLIGHTMAP_ON","LIGHTMAP_ON","SHADOWS_SCREEN","SHADOWS_SHADOWMASK"},
        new string[] {"DIRECTIONAL", "LIGHTMAP_ON","LIGHTPROBE_SH","SHADOWS_SCREEN","SHADOWS_SHADOWMASK"},
        new string[] {"DIRECTIONAL", "LIGHTMAP_ON","LIGHTMAP_SHADOW_MIXING","SHADOWS_SCREEN","SHADOWS_SHADOWMASK"},
        new string[] {"DIRECTIONAL", "DYNAMICLIGHTMAP_ON","LIGHTMAP_ON","LIGHTMAP_SHADOW_MIXING","SHADOWS_SCREEN","SHADOWS_SHADOWMASK"},
        new string[] {"DIRECTIONAL", "LIGHTMAP_ON","LIGHTMAP_SHADOW_MIXING","LIGHTPROBE_SH","SHADOWS_SCREEN","SHADOWS_SHADOWMASK"},
        new string[] {"DIRECTIONAL", "DIRLIGHTMAP_COMBINED","DYNAMICLIGHTMAP_ON","SHADOWS_SCREEN"},
        new string[] {"DIRECTIONAL", "DIRLIGHTMAP_COMBINED","LIGHTMAP_ON","SHADOWS_SCREEN"},
        new string[] {"DIRECTIONAL", "DIRLIGHTMAP_COMBINED","LIGHTMAP_ON","LIGHTPROBE_SH","SHADOWS_SCREEN"},
        new string[] {"DIRECTIONAL", "DIRLIGHTMAP_COMBINED","DYNAMICLIGHTMAP_ON","LIGHTMAP_ON","SHADOWS_SCREEN"},
        new string[] {"DIRECTIONAL", "DIRLIGHTMAP_COMBINED","LIGHTMAP_ON","LIGHTMAP_SHADOW_MIXING","SHADOWS_SCREEN"},
        new string[] {"DIRECTIONAL", "DIRLIGHTMAP_COMBINED","LIGHTMAP_ON","LIGHTMAP_SHADOW_MIXING","LIGHTPROBE_SH","SHADOWS_SCREEN"},
        new string[] {"DIRECTIONAL", "DIRLIGHTMAP_COMBINED","DYNAMICLIGHTMAP_ON","LIGHTMAP_ON","LIGHTMAP_SHADOW_MIXING","SHADOWS_SCREEN"},
        new string[] {"DIRECTIONAL", "DIRLIGHTMAP_COMBINED","LIGHTMAP_ON","SHADOWS_SCREEN","SHADOWS_SHADOWMASK"},
        new string[] {"DIRECTIONAL", "DIRLIGHTMAP_COMBINED","LIGHTMAP_ON","LIGHTPROBE_SH","SHADOWS_SCREEN","SHADOWS_SHADOWMASK"},
        new string[] {"DIRECTIONAL", "DIRLIGHTMAP_COMBINED","DYNAMICLIGHTMAP_ON","LIGHTMAP_ON","SHADOWS_SCREEN","SHADOWS_SHADOWMASK"},
        new string[] {"DIRECTIONAL", "DIRLIGHTMAP_COMBINED","LIGHTMAP_ON","LIGHTMAP_SHADOW_MIXING","SHADOWS_SCREEN","SHADOWS_SHADOWMASK"},
        new string[] {"DIRECTIONAL", "DIRLIGHTMAP_COMBINED","LIGHTMAP_ON","LIGHTMAP_SHADOW_MIXING","LIGHTPROBE_SH","SHADOWS_SCREEN","SHADOWS_SHADOWMASK"},
        new string[] {"DIRECTIONAL", "DIRLIGHTMAP_COMBINED","DYNAMICLIGHTMAP_ON","LIGHTMAP_ON","LIGHTMAP_SHADOW_MIXING","SHADOWS_SCREEN","SHADOWS_SHADOWMASK"},
        new string[] {"DIRECTIONAL", "VERTEXLIGHT_ON"},
        new string[] {"DIRECTIONAL", "LIGHTPROBE_SH","VERTEXLIGHT_ON"},
        new string[] {"DIRECTIONAL", "DYNAMICLIGHTMAP_ON","VERTEXLIGHT_ON"},
        new string[] {"DIRECTIONAL", "DIRLIGHTMAP_COMBINED","DYNAMICLIGHTMAP_ON","VERTEXLIGHT_ON"},
        new string[] {"DIRECTIONAL", "SHADOWS_SCREEN","VERTEXLIGHT_ON"},
        new string[] {"DIRECTIONAL", "LIGHTPROBE_SH","SHADOWS_SCREEN","VERTEXLIGHT_ON"},
        new string[] {"DIRECTIONAL", "DYNAMICLIGHTMAP_ON","SHADOWS_SCREEN","VERTEXLIGHT_ON"},
        new string[] {"DIRECTIONAL", "DIRLIGHTMAP_COMBINED","DYNAMICLIGHTMAP_ON","SHADOWS_SCREEN","VERTEXLIGHT_ON"},
        new string[] {"DIRECTIONAL", "LIGHTMAP_SHADOW_MIXING","SHADOWS_SCREEN","VERTEXLIGHT_ON"},
        new string[] {"DIRECTIONAL", "LIGHTMAP_SHADOW_MIXING","LIGHTPROBE_SH","SHADOWS_SCREEN","VERTEXLIGHT_ON"},
        new string[] {"DIRECTIONAL", "DYNAMICLIGHTMAP_ON","LIGHTMAP_SHADOW_MIXING","SHADOWS_SCREEN","VERTEXLIGHT_ON"},
        new string[] {"DIRECTIONAL", "DIRLIGHTMAP_COMBINED","DYNAMICLIGHTMAP_ON","LIGHTMAP_SHADOW_MIXING","SHADOWS_SCREEN","VERTEXLIGHT_ON"},
        new string[] {"DIRECTIONAL", "SHADOWS_SCREEN","SHADOWS_SHADOWMASK","VERTEXLIGHT_ON"},
        new string[] {"DIRECTIONAL", "LIGHTPROBE_SH","SHADOWS_SCREEN","SHADOWS_SHADOWMASK","VERTEXLIGHT_ON"},
        new string[] {"DIRECTIONAL", "DYNAMICLIGHTMAP_ON","SHADOWS_SCREEN","SHADOWS_SHADOWMASK","VERTEXLIGHT_ON"},
        new string[] {"DIRECTIONAL", "DIRLIGHTMAP_COMBINED","DYNAMICLIGHTMAP_ON","SHADOWS_SCREEN","SHADOWS_SHADOWMASK","VERTEXLIGHT_ON"},
        new string[] {"DIRECTIONAL", "LIGHTMAP_SHADOW_MIXING","SHADOWS_SCREEN","SHADOWS_SHADOWMASK","VERTEXLIGHT_ON"},
        new string[] {"DIRECTIONAL", "LIGHTMAP_SHADOW_MIXING","LIGHTPROBE_SH","SHADOWS_SCREEN","SHADOWS_SHADOWMASK","VERTEXLIGHT_ON"},
        new string[] {"DIRECTIONAL", "DYNAMICLIGHTMAP_ON","LIGHTMAP_SHADOW_MIXING","SHADOWS_SCREEN","SHADOWS_SHADOWMASK","VERTEXLIGHT_ON"},
        new string[] {"DIRECTIONAL", "DIRLIGHTMAP_COMBINED","DYNAMICLIGHTMAP_ON","LIGHTMAP_SHADOW_MIXING","SHADOWS_SCREEN","SHADOWS_SHADOWMASK","VERTEXLIGHT_ON"},
    };
    static string[] multi_compile_fwdadd_KeywordsNames = new string[] {
          "DIRECTIONAL","POINT","SPOT","POINT_COOKIE","DIRECTIONAL_COOKIE"
        };
    static string[][] multi_compile_fwdadd_Variants = new string[][]
        {
           new string[] {"POINT"}, new string[] {"DIRECTIONAL"}, new string[] {"SPOT"}, new string[] {"POINT_COOKIE"}, new string[] {"DIRECTIONAL_COOKIE"},
        };
    static string[] multi_compile_fwdadd_fullshadows_KeywordsNames = new string[] {
           "DIRECTIONAL","DIRECTIONAL_COOKIE","LIGHTMAP_SHADOW_MIXING","POINT","POINT_COOKIE","SPOT","SHADOWS_CUBE","SHADOWS_DEPTH","SHADOWS_SCREEN","SHADOWS_SHADOWMASK","SHADOWS_SOFT",
        };
    static string[][] multi_compile_fwdadd_fullshadows_Variants = new string[][]
      {
          new string[] {"POINT"},
          new string[] {"DIRECTIONAL"},
          new string[] {"SPOT"},
          new string[] {"POINT_COOKIE"},
          new string[] {"DIRECTIONAL_COOKIE"},
          new string[] {"POINT","SHADOWS_SHADOWMASK"},
          new string[] {"DIRECTIONAL","SHADOWS_SHADOWMASK"},
          new string[] {"SHADOWS_SHADOWMASK","SPOT"},
          new string[] {"POINT_COOKIE","SHADOWS_SHADOWMASK"},
          new string[] {"DIRECTIONAL_COOKIE","SHADOWS_SHADOWMASK"},
          new string[] {"LIGHTMAP_SHADOW_MIXING","POINT","SHADOWS_SHADOWMASK"},
          new string[] {"DIRECTIONAL","LIGHTMAP_SHADOW_MIXING","SHADOWS_SHADOWMASK"},
          new string[] {"LIGHTMAP_SHADOW_MIXING","SHADOWS_SHADOWMASK","SPOT"},
          new string[] {"LIGHTMAP_SHADOW_MIXING","POINT_COOKIE","SHADOWS_SHADOWMASK"},
          new string[] {"DIRECTIONAL_COOKIE","LIGHTMAP_SHADOW_MIXING","SHADOWS_SHADOWMASK"},
          new string[] {"SHADOWS_DEPTH","SPOT"},
          new string[] {"SHADOWS_DEPTH","SHADOWS_SOFT","SPOT"},
          new string[] {"LIGHTMAP_SHADOW_MIXING","SHADOWS_DEPTH","SPOT"},
          new string[] {"LIGHTMAP_SHADOW_MIXING","SHADOWS_DEPTH","SHADOWS_SOFT","SPOT"},
          new string[] {"SHADOWS_DEPTH","SHADOWS_SHADOWMASK","SPOT"},
          new string[] {"SHADOWS_DEPTH","SHADOWS_SHADOWMASK","SHADOWS_SOFT","SPOT"},
          new string[] {"LIGHTMAP_SHADOW_MIXING","SHADOWS_DEPTH","SHADOWS_SHADOWMASK","SPOT"},
          new string[] {"LIGHTMAP_SHADOW_MIXING","SHADOWS_DEPTH","SHADOWS_SHADOWMASK","SHADOWS_SOFT","SPOT"},
          new string[] {"DIRECTIONAL","SHADOWS_SCREEN"},
          new string[] {"DIRECTIONAL_COOKIE","SHADOWS_SCREEN"},
          new string[] {"DIRECTIONAL","LIGHTMAP_SHADOW_MIXING","SHADOWS_SCREEN"},
          new string[] {"DIRECTIONAL_COOKIE","LIGHTMAP_SHADOW_MIXING","SHADOWS_SCREEN"},
          new string[] {"DIRECTIONAL","SHADOWS_SCREEN","SHADOWS_SHADOWMASK"},
          new string[] {"DIRECTIONAL_COOKIE","SHADOWS_SCREEN","SHADOWS_SHADOWMASK"},
          new string[] {"DIRECTIONAL","LIGHTMAP_SHADOW_MIXING","SHADOWS_SCREEN","SHADOWS_SHADOWMASK"},
          new string[] {"DIRECTIONAL_COOKIE","LIGHTMAP_SHADOW_MIXING","SHADOWS_SCREEN","SHADOWS_SHADOWMASK"},
          new string[] {"POINT","SHADOWS_CUBE"},
          new string[] {"POINT","SHADOWS_CUBE","SHADOWS_SOFT"},
          new string[] {"LIGHTMAP_SHADOW_MIXING","POINT","SHADOWS_CUBE"},
          new string[] {"LIGHTMAP_SHADOW_MIXING","POINT","SHADOWS_CUBE","SHADOWS_SOFT"},
          new string[] {"POINT","SHADOWS_CUBE","SHADOWS_SHADOWMASK"},
          new string[] {"POINT","SHADOWS_CUBE","SHADOWS_SHADOWMASK","SHADOWS_SOFT"},
          new string[] {"LIGHTMAP_SHADOW_MIXING","POINT","SHADOWS_CUBE","SHADOWS_SHADOWMASK"},
          new string[] {"LIGHTMAP_SHADOW_MIXING","POINT","SHADOWS_CUBE","SHADOWS_SHADOWMASK","SHADOWS_SOFT"},
          new string[] {"POINT_COOKIE","SHADOWS_CUBE"},
          new string[] {"POINT_COOKIE","SHADOWS_CUBE","SHADOWS_SOFT"},
          new string[] {"LIGHTMAP_SHADOW_MIXING","POINT_COOKIE","SHADOWS_CUBE"},
          new string[] {"LIGHTMAP_SHADOW_MIXING","POINT_COOKIE","SHADOWS_CUBE","SHADOWS_SOFT"},
          new string[] {"POINT_COOKIE","SHADOWS_CUBE","SHADOWS_SHADOWMASK"},
          new string[] {"POINT_COOKIE","SHADOWS_CUBE","SHADOWS_SHADOWMASK","SHADOWS_SOFT"},
          new string[] {"LIGHTMAP_SHADOW_MIXING","POINT_COOKIE","SHADOWS_CUBE","SHADOWS_SHADOWMASK"},
          new string[] {"LIGHTMAP_SHADOW_MIXING","POINT_COOKIE","SHADOWS_CUBE","SHADOWS_SHADOWMASK","SHADOWS_SOFT"},
      };
    static string[] multi_compile_fog_KeywordsNames = new string[] {
            "FOG_EXP","FOG_EXP2","FOG_LINEAR",
        };
    static string[][] multi_compile_fog_Variants = new string[][]
        {
            new string[] {"<no keywords defined>"}, new string[] {"FOG_LINEAR"}, new string[] {"FOG_EXP"}, new string[] {"FOG_EXP2"},
        };
    static string[] multi_compile_instancing_KeywordsNames = new string[] {
           "INSTANCING_ON",
        };
    static string[][] multi_compile_instancing_Variants = new string[][]
       {
            new string[] {"<no keywords defined>"}, new string[] {"INSTANCING_ON"},
       };
    static string[] multi_compile_shadowcaster_KeywordsNames = new string[] {
           "SHADOWS_DEPTH","SHADOWS_CUBE",
        };
    static string[][] multi_compile_shadowcaster_Variants = new string[][]
     {
          new string[] {"SHADOWS_DEPTH"},new string[] {"SHADOWS_CUBE"},
     };
    
    //内置关键字
    static string[] buildInDefaultKeys = new string[] { "POINT", "SPOT", "DIRECTIONAL", "POINT_COOKIE", "DIRECTIONAL_COOKIE", "VERTEXLIGHT_ON", "LOD_FADE_CROSSFADE", "EDITOR_VISUALIZATION", "_EMISSION", "LIGHTPROBE_SH", "SOFTPARTICLES_ON", "SHADOWS_SCREEN", "SHADOWS_SOFT", "SHADOWS_DEPTH", "SHADOWS_CUBE", "UNITY_HDR_ON", "SHADOWS_SPLIT_SPHERES", "ETC1_EXTERNAL_ALPHA", };
    static string[] buildInExtraKeys = new string[] { "BILLBOARD_FACE_CAMERA_POS", "_NORMALMAP", };
    static string[] buildInAutoStripKeys = new string[] { "UNITY_SINGLE_PASS_STEREO", "DYNAMICLIGHTMAP_ON", "DIRLIGHTMAP_COMBINED", "LIGHTMAP_ON", "STEREO_MULTIVIEW_ON", "STEREO_INSTANCING_ON", "FOG_LINEAR", "FOG_EXP", "FOG_EXP2", "LIGHTMAP_SHADOW_MIXING", "SHADOWS_SHADOWMASK", "INSTANCING_ON", "PROCEDURAL_INSTANCING_ON", "STEREO_CUBEMAP_RENDER_ON", };
    //UnityCGInclude自带的userDefine关键字
    static string[] userDefineKeys = new string[] { "EFFECT_BUMP", "EFFECT_EXTRA_TEX", "EFFECT_HUE_VARIATION", "EFFECT_BILLBOARD", "EFFECT_SUBSURFACE", "_WINDQUALITY_BEST", "_WINDQUALITY_PALM", "GEOM_TYPE_BRANCH_DETAIL", "LOD_FADE_PERCENTAGE", "UNITY_COLORSPACE_GAMMA", "_GLOSSYREFLECTIONS_OFF", "PIXELSNAP_ON", "_PARALLAXMAP", "_SPECGLOSSMAP", "_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A", "_METALLICGLOSSMAP", "_ALPHATEST_ON", "_ALPHAPREMULTIPLY_ON", "DIRLIGHTMAP_OFF", "LIGHTMAP_OFF", "DYNAMICLIGHTMAP_OFF", "GEOM_TYPE_BRANCH", "GEOM_TYPE_FROND", "GEOM_TYPE_LEAF", "_TERRAIN_NORMAL_MAP", "_FADING_ON", "_COLOROVERLAY_ON", "_COLORCOLOR_ON", "_COLORADDSUBDIFF_ON", "_ALPHAMODULATE_ON", "_WINDQUALITY_NONE", "_WINDQUALITY_FAST", "_WINDQUALITY_BETTER", "GEOM_TYPE_MESH", "_AO", "UNITY_PASS_SHADOWCASTER", "_SPECULARHIGHLIGHTS_OFF", "_ALPHABLEND_ON", "_DETAIL_MULX2", "_REQUIRE_UV2", };

    const string PRAGMA_COMMENT_MARK = "// Edited by StripShader Control: ";
    const string PRAGMA_DISABLED_MARK = "// Disabled by StripShader Control: ";
    //全局关键字
    const string PRAGMA_MULTI_COMPILE = "#pragma multi_compile ";
    const string PRAGMA_SHADER_FEATURE = "#pragma shader_feature ";
    //本地关键字
    const string PRAGMA_MULTI_COMPILE_LOCAL = "#pragma multi_compile_local ";
    const string PRAGMA_SHADER_FEATURE_LOCAL = "#pragma shader_feature_local ";
    //keyword缩写
    const string PRAGMA_MULTI_COMPILE_FWDBASE = "#pragma multi_compile_fwdbase";
    const string PRAGMA_MULTI_COMPILE_FWDBASE_FULLSHADOWS = "#pragma multi_compile_fwdbase_fullshadows";
    const string PRAGMA_MULTI_COMPILE_FWDADD = "#pragma multi_compile_fwdadd";
    const string PRAGMA_MULTI_COMPILE_FOG = "#pragma multi_compile_fog";
    const string PRAGMA_MULTI_COMPILE_INSTANCING = "#pragma multi_compile_instancing";
    const string PRAGMA_MULTI_COMPILE_SHADOWCASTER = "#pragma multi_compile_shadowcaster";
    

    //跳过keyword
    const string PRAGMA_SKIP_VARIANTS = "#pragma skip_variants ";
    //渲染平台
    const string PRAGMA_ONLY_RENDERERS = "#pragma only_renderers ";
    const string PRAGMA_EXCLUDE_RENDERERS = "#pragma exclude_renderers ";

    const string BACKUP_SUFFIX = "_backup";
    const string PRAGMA_UNDERSCORE = "__ ";

    #region Gets
    public static string[] GetBuildinKeyswords(ShaderKeywordType keywordType)
    {
        switch (keywordType)
        {
            case ShaderKeywordType.BuiltinDefault:
                return BuildInDefaultKeys;
            case ShaderKeywordType.BuiltinExtra:
                return BuildInExtraKeys;
            case ShaderKeywordType.BuiltinAutoStripped:
                return BuildInAutoStripKeys;
            case ShaderKeywordType.UserDefined:
                return UserDefineKeys;
            default: return null;
        }
    }
    public static string[] BuildInDefaultKeys
    {
        get {
            return buildInDefaultKeys;
        }
    }

    public static string[] BuildInExtraKeys
    {
        get
        {
            return buildInExtraKeys;
        }
    }

    public static string[] BuildInAutoStripKeys
    {
        get
        {
            return buildInAutoStripKeys;
        }
    }

    public static string[] UserDefineKeys
    {
        get
        {
            return userDefineKeys;
        }
    }
    #endregion
   
    //根据文本返回一行语句的类型
    public static ShaderLineType GetShaderLineType(string shortCutKey)
    {
        switch (shortCutKey)
        {
            case PRAGMA_MULTI_COMPILE:
                return ShaderLineType.MULTI_COMPILE;
            case PRAGMA_SHADER_FEATURE:
                return ShaderLineType.SHADER_FEATURE;
            case PRAGMA_MULTI_COMPILE_LOCAL:
                return ShaderLineType.MULTI_COMPILE_LOCAL;
            case PRAGMA_SHADER_FEATURE_LOCAL:
                return ShaderLineType.SHADER_FEATURE_LOCAL;
            case PRAGMA_MULTI_COMPILE_FWDBASE:
                return ShaderLineType.MULTI_COMPILE_FWDBASE;
            case PRAGMA_MULTI_COMPILE_FWDADD:
                return ShaderLineType.MULTI_COMPILE_FWDADD;
            case PRAGMA_MULTI_COMPILE_FWDBASE_FULLSHADOWS:
                return ShaderLineType.MULTI_COMPILE_FWDBASE_FULLSHADOWS;
            case PRAGMA_MULTI_COMPILE_FOG:
                return ShaderLineType.MULTI_COMPILE_FOG;
            case PRAGMA_SKIP_VARIANTS:
                return ShaderLineType.SKIP_VARIANTS;
            case PRAGMA_ONLY_RENDERERS:
                return ShaderLineType.ONLY_RENDERERS;
            case PRAGMA_EXCLUDE_RENDERERS:
                return ShaderLineType.EXCLUDE_RENDERERS;
            case PRAGMA_MULTI_COMPILE_INSTANCING:
                return ShaderLineType.MULTI_COMPILE_INSTANCING;
            case PRAGMA_MULTI_COMPILE_SHADOWCASTER:
                return ShaderLineType.MULTI_COMPILE_SHADOWCASTER;
            default:
                return ShaderLineType.NONE;
        }
    }

    //根据文本返回一行语句编辑的类型
    public static EditorLineType GetEditorLineType(string lineKey)
    {
        switch (lineKey)
        {
            case PRAGMA_COMMENT_MARK:
                return EditorLineType.COMMENT_MARK;
            case PRAGMA_DISABLED_MARK:
                return EditorLineType.DISABLED_MARK;
            default:
                return EditorLineType.NONE;
        }
    }

    //根据语句的类型返回标志文本
    public static string GetShaderLineTypeString(ShaderLineType lineType)
    {
        switch (lineType)
        {
            case ShaderLineType.MULTI_COMPILE:
                return PRAGMA_MULTI_COMPILE;
            case ShaderLineType.SHADER_FEATURE:
                return PRAGMA_SHADER_FEATURE;
            case ShaderLineType.MULTI_COMPILE_LOCAL:
                return PRAGMA_MULTI_COMPILE_LOCAL;
            case ShaderLineType.SHADER_FEATURE_LOCAL:
                return PRAGMA_SHADER_FEATURE_LOCAL;
            case ShaderLineType.MULTI_COMPILE_FWDBASE:
                return PRAGMA_MULTI_COMPILE_FWDBASE;
            case ShaderLineType.MULTI_COMPILE_FWDADD:
                return PRAGMA_MULTI_COMPILE_FWDADD;
            case ShaderLineType.MULTI_COMPILE_FWDBASE_FULLSHADOWS:
                return PRAGMA_MULTI_COMPILE_FWDBASE_FULLSHADOWS;
            case ShaderLineType.MULTI_COMPILE_FOG:
                return PRAGMA_MULTI_COMPILE_FOG;
            case ShaderLineType.SKIP_VARIANTS:
                return PRAGMA_SKIP_VARIANTS;
            case ShaderLineType.ONLY_RENDERERS:
                return PRAGMA_ONLY_RENDERERS;
            case ShaderLineType.EXCLUDE_RENDERERS:
                return PRAGMA_EXCLUDE_RENDERERS;
            case ShaderLineType.MULTI_COMPILE_INSTANCING:
                return PRAGMA_MULTI_COMPILE_INSTANCING;
            case ShaderLineType.MULTI_COMPILE_SHADOWCASTER:
                return PRAGMA_MULTI_COMPILE_SHADOWCASTER;
            default:
                return "";
        }
    }

    //根据语句的编辑类型返回标志文本
    public static string GetEditorLineTypeString(EditorLineType lineType)
    {
        switch (lineType)
        {
            case EditorLineType.COMMENT_MARK:
                return PRAGMA_COMMENT_MARK;
            case EditorLineType.DISABLED_MARK:
                return PRAGMA_DISABLED_MARK;
            default:
                return "";
        }
    }
    //There are several “shortcut” notations for compiling multiple shader variants. These are mostly to deal with different light, shadow and lightmap types in Unity. See documentation on the rendering pipeline for details.
    //是否是内置短语
    public static bool IsBuiltinShortcut(ShaderLineType lineType)
    {
        switch (lineType)
        {
            case ShaderLineType.MULTI_COMPILE_FWDBASE:
                return true;
            case ShaderLineType.MULTI_COMPILE_FWDADD:
                return true;
            case ShaderLineType.MULTI_COMPILE_FWDBASE_FULLSHADOWS:
                return true;
            case ShaderLineType.MULTI_COMPILE_FOG:
                return true;
            case ShaderLineType.MULTI_COMPILE_INSTANCING:
                return true;
            case ShaderLineType.MULTI_COMPILE_SHADOWCASTER:
                return true;
            default:
                return false;
        }
    }
    //是否是输入的关键字
    public static bool IsInputShortcut(ShaderLineType lineType)
    {
        switch (lineType)
        {
            case ShaderLineType.MULTI_COMPILE:
                return true;
            case ShaderLineType.SHADER_FEATURE:
                return true;
            case ShaderLineType.MULTI_COMPILE_LOCAL:
                return true;
            case ShaderLineType.SHADER_FEATURE_LOCAL:
                return true;
            case ShaderLineType.SKIP_VARIANTS:
                return true;
            default:
                return false;
        }
    }
    public static bool IsSahderFeature(ShaderLineType lineType)
    {
        switch (lineType)
        {
            case ShaderLineType.SHADER_FEATURE:
                return true;
            case ShaderLineType.SHADER_FEATURE_LOCAL:
                return true;
            default:
                return false;
        }
    }
    private static int CaculateShortcutVariants(string[][] variants, StripKeyword[] skipedKeys)
    {
        int count = variants.Length;
        if (skipedKeys != null)
        {
            //所有变体
            for (int i = 0; i < variants.Length; i++)
            {
                bool skip = false;
                //关键字
                string[] keys = variants[i];
                for (int k = 0; k < keys.Length; k++)
                {
                    string key = keys[k];
                    for (int j = 0; j < skipedKeys.Length; j++)
                    {
                        if (key == skipedKeys[j].name)
                        {
                            skip = true;
                            break;
                        }
                    }
                    if (skip)
                        break;
                }
                if (skip)
                {
                    count--;
                }
            }
        }
        return count;
    }
    //返回内置短语的变体数量
    public static int GetShortcutVariantCount(ShaderLineType lineType,StripKeyword[] skipedKeys = null)
    {
        switch (lineType)
        {
            case ShaderLineType.MULTI_COMPILE_FWDBASE:
            {
                return CaculateShortcutVariants(multi_compile_fwdbase_Variants, skipedKeys);
            }
            case ShaderLineType.MULTI_COMPILE_FWDADD:
                return CaculateShortcutVariants(multi_compile_fwdadd_Variants, skipedKeys);
            case ShaderLineType.MULTI_COMPILE_FWDBASE_FULLSHADOWS:
                return CaculateShortcutVariants(multi_compile_fwdadd_fullshadows_Variants, skipedKeys);
            case ShaderLineType.MULTI_COMPILE_FOG:
                return CaculateShortcutVariants(multi_compile_fog_Variants, skipedKeys);
            case ShaderLineType.MULTI_COMPILE_INSTANCING:
                return CaculateShortcutVariants(multi_compile_instancing_Variants, skipedKeys);
            case ShaderLineType.MULTI_COMPILE_SHADOWCASTER:
                return CaculateShortcutVariants(multi_compile_shadowcaster_Variants, skipedKeys);
            default:
                return 1;
        }
    }
    //返回内置短语的关键字集合
    public static string[] GetShortcutKeywords(ShaderLineType lineType)
    {
        switch (lineType)
        {
            case ShaderLineType.MULTI_COMPILE_FWDBASE:
                return multi_compile_fwdbase_KeywordsNames;
            case ShaderLineType.MULTI_COMPILE_FWDADD:
                return multi_compile_fwdadd_KeywordsNames;
            case ShaderLineType.MULTI_COMPILE_FWDBASE_FULLSHADOWS:
                return multi_compile_fwdadd_fullshadows_KeywordsNames;
            case ShaderLineType.MULTI_COMPILE_FOG:
                return multi_compile_fog_KeywordsNames;
            case ShaderLineType.MULTI_COMPILE_INSTANCING:
                return multi_compile_instancing_KeywordsNames;
            case ShaderLineType.MULTI_COMPILE_SHADOWCASTER:
                return multi_compile_shadowcaster_KeywordsNames;
            default:
                return null;
        }
    }
    //判断字符串是那种关键字
    public static ShaderKeywordType GetKeywordType(string key)
    {
        try
        {
            ShaderKeyword shaderKeyword = new ShaderKeyword(key);
            return shaderKeyword.GetKeywordType();
        }
        catch
        {
            return ShaderKeywordType.None;
        }
    }
    //是否是注释行
    public static void IsBlockCommentLine(string line,ref bool blockComment)
    {
        int lineCommentIndex = line.IndexOf("//");
        int blocCommentIndex = line.IndexOf("/*");
        int endCommentIndex = line.IndexOf("*/");
        if (blocCommentIndex > 0 && (lineCommentIndex > blocCommentIndex || lineCommentIndex < 0))
        {
            blockComment = true;
        }
        if (endCommentIndex > blocCommentIndex && (lineCommentIndex > endCommentIndex || lineCommentIndex < 0))
        {
            blockComment = false;
        }
    }
    //注释行位置
    public static int CommentLineIndex(string line,int startPos = 0)
    {
        int lineCommentIndex = line.IndexOf("//",startPos);
        return lineCommentIndex;
    }
    //解析一段shader语句的类型
    public static void ParseShaderLineType(string line,ref ShaderLineType lineType, ref int start, ref int end)
    {
        lineType = ShaderLineType.NONE;
        start = -1;
        end = -1;
        Array linetypes = Enum.GetValues(typeof(ShaderLineType));
        for (int i = 0; i < linetypes.Length; i++)
        {
            ShaderLineType curtype = (ShaderLineType)linetypes.GetValue(i);
            if (curtype == ShaderLineType.NONE)
                continue;
            else
            {
                string findKey = GetShaderLineTypeString(curtype);
                start = line.IndexOf(findKey);
                if (start >= 0)
                {
                    lineType = GetShaderLineType(findKey);
                    end = start + findKey.Length;
                    return;
                }
            }
        }
    }
    //解析一段shader语句的编辑类型
    public static void ParseEditorLineType(string line , ref EditorLineType lineType, ref int start, ref int end)
    {
        lineType = EditorLineType.NONE;
        start = -1;
        end = -1;
        Array linetypes = Enum.GetValues(typeof(EditorLineType));
        for (int i = 0; i < linetypes.Length; i++)
        {
            EditorLineType curtype = (EditorLineType)linetypes.GetValue(i);
            if (curtype == EditorLineType.NONE)
                continue;
            else
            {
                string findKey = GetEditorLineTypeString(curtype);
                start = line.IndexOf(findKey);
                if (start >= 0)
                {
                    lineType = GetEditorLineType(findKey);
                    end = start + findKey.Length;
                }
            }
        }
    }
    
    //解析shader文本
    public static void ScanShader(ref StripShader shader)
    {
        //重置shader
        shader.passes.Clear();
        shader.keywords.Clear();
        shader.hasBackup = File.Exists(shader.path + BACKUP_SUFFIX);
        shader.pendingChanges = false;
        shader.editedByShaderControl = false;

        //读取shader文本
        string[] shaderLines = File.ReadAllLines(shader.path);
        string[] separator = new string[] { " " };
        StripShaderPass currentPass = new StripShaderPass();
        StripShaderPass basePass = null;
        int pass = -1;
        bool blockComment = false;
        StripKeywordLine keywordLine = new StripKeywordLine();
        for (int k = 0; k < shaderLines.Length; k++)
        {
            //删除字符串头部和尾部的空格
            string line = shaderLines[k].Trim();
            if (line.Length == 0)
                continue;
            //是否是注释行
            IsBlockCommentLine(line, ref blockComment);
            if (blockComment)
                continue;
            string lineUPPER = line.ToUpper();
            if (lineUPPER.Equals("PASS") || lineUPPER.StartsWith("PASS "))
            {
                if (pass >= 0)
                {
                    currentPass.pass = pass;
                    if (basePass != null)
                        currentPass.Add(basePass.keywordLines);
                    shader.Add(currentPass);
                }
                else if (currentPass.keywordCount > 0)
                {
                    basePass = currentPass;
                }
                currentPass = new StripShaderPass();
                pass++;
                continue;
            }

            //第一个注释符号 //的位置
            int lineCommentIndex = CommentLineIndex(line);
            //编辑器添加的语句行信息
            EditorLineType editorLine = EditorLineType.NONE;
            int editorStartIndex = -1;
            int editorEndIndex = -1;
            ParseEditorLineType(line, ref editorLine, ref editorStartIndex,ref editorEndIndex);

            //该行是注释行 并且不是被主动编辑的 跳过
            if (lineCommentIndex == 0 && editorLine == EditorLineType.NONE)
            {
                continue; // 跳过注释
            }

            //shader关键字语句行信息
            ShaderLineType shaderLine = ShaderLineType.NONE;
            int keyStartIndex = -1;
            int keyEndIndex = -1;
            ParseShaderLineType(line, ref shaderLine, ref keyStartIndex,ref keyEndIndex);
            //并非是被程序编辑过的
            if (editorLine == EditorLineType.NONE)
            {
                //如果是手写输入的关键字
                if (IsInputShortcut(shaderLine))
                {
                    keyStartIndex = Mathf.Max(0, keyStartIndex);
                    keyEndIndex = Mathf.Max(0, keyEndIndex);
                    //检查PRAGMA 关键字之后的字符串是否包含注释
                    int commentindex = CommentLineIndex(line, keyStartIndex + keyEndIndex);
                    if (commentindex < 0)
                    {
                        commentindex = line.Length;
                    }
                    int length = commentindex - (keyStartIndex + keyEndIndex);
                    //用空格分割 PRAGMA语句到//注释符号之间的语句
                    string[] splitedKeys = line.Substring(keyStartIndex + keyEndIndex, length).Split(separator, StringSplitOptions.RemoveEmptyEntries);
                    if (splitedKeys.Length > 0)
                    {
                        keywordLine = new StripKeywordLine();
                        //是剔除字段
                        bool isSkip = shaderLine == ShaderLineType.SKIP_VARIANTS;
                        currentPass.hasSkipKeyword = isSkip?isSkip:currentPass.hasSkipKeyword;
                        
                        //添加手写关键字
                        for (int i = 0; i < splitedKeys.Length; i++)
                        {
                            splitedKeys[i] = splitedKeys[i].Trim();
                            StripKeyword keyword = keywordLine.GetKeyword(splitedKeys[i]);
                            if (keyword == null)
                                keyword = new StripKeyword(splitedKeys[i]);
                            keyword.enabled = true;
                            keyword.isSkip = isSkip;
                            keywordLine.Add(keyword);
                        }
                        keywordLine.editorLineType = editorLine;
                        keywordLine.shaderLineType = shaderLine;
                        currentPass.Add(keywordLine);
                    }
                }
                else  //添加缩写marco的内置关键字
                if (IsBuiltinShortcut(shaderLine))
                {
                    keywordLine = new StripKeywordLine();
                    string[] buildinkeys = GetShortcutKeywords(shaderLine);
                    for (int i = 0; i < buildinkeys.Length; i++)
                    {
                        StripKeyword keyword = keywordLine.GetKeyword(buildinkeys[i]);
                        if (keyword == null)
                            keyword = new StripKeyword(buildinkeys[i]);
                        keyword.enabled = true;
                        keyword.isSkip = false;
                        keywordLine.Add(keyword);
                    }
                    keywordLine.editorLineType = editorLine;
                    keywordLine.shaderLineType = shaderLine;
                    currentPass.Add(keywordLine);
                }
            }
        }
        currentPass.pass = Mathf.Max(pass, 0);
        if (basePass != null)
            currentPass.Add(basePass.keywordLines);
        shader.Add(currentPass);
        shader.UpdateVariantCount();
    }

    public static void UpdateShader(ref StripShader shader)
    {
        if (shader.isReadOnly)
        {
            EditorUtility.DisplayDialog("Locked file", "Shader file " + shader.name + " is read-only.", "Ok");
            return;
        }
        try
        {
            // Create backup
            string backupPath = shader.path + BACKUP_SUFFIX;
            if (!File.Exists(backupPath))
            {
                AssetDatabase.CopyAsset(shader.path, backupPath);
                shader.hasBackup = true;
            }

            // Reads and updates shader from disk
           // string[] shaderLines = File.ReadAllLines(shader.path);
            //string[] separator = new string[] { " " };
      
            // Writes modified shader
            //File.WriteAllText(shader.path, sb.ToString());
            AssetDatabase.Refresh();
            
        }
        catch (Exception ex)
        {
            Debug.LogError("Unexpected exception caught while updating shader: " + ex.Message);
        }
    }
    public static void RestoreShader(ref StripShader shader)
    {
        try
        {
            string shaderBackupPath = shader.path + BACKUP_SUFFIX;
            if (!File.Exists(shaderBackupPath))
            {
                EditorUtility.DisplayDialog("Restore shader", "Shader backup is missing!", "OK");
                return;
            }
            File.Copy(shaderBackupPath, shader.path, true);
            File.Delete(shaderBackupPath);
            if (File.Exists(shaderBackupPath + ".meta"))
                File.Delete(shaderBackupPath + ".meta");
            AssetDatabase.Refresh(); 
        }
        catch (Exception ex)
        {
            Debug.LogError("Unexpected exception caught while restoring shader: " + ex.Message);
        }
    }
}
