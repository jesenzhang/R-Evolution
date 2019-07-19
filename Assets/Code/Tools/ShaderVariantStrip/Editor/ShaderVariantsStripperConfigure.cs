using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace ShaderVariantsStripper
{
    public class MaskFlagsAttribute : PropertyAttribute { }
    [CustomPropertyDrawer(typeof(MaskFlagsAttribute))]
    public class MaskFlagsDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            /*绘制枚举复选框 ， 0-Nothing，-1-Everything,其他是枚举之和
            枚举值（2的x次幂）：2的0次幂=1，2的1次幂=2，2的2次幂=4，8，16...
            */
            property.intValue= EditorGUI.MaskField(position, property.intValue, property.enumNames);
            /*
            Type enumtype = property.GetType();
            Array values = property.enumNames;
            string[] names = property.enumNames;
            // var maskValue = (int)Enum.Parse(enumtype, maskEnum.ToString());
            int maskValue = property.intValue;
            int select = 0;
            for (int i = 0; i < values.Length; i++)
            {
                int bitvalue = (int)values.GetValue(i);
                if ((maskValue & bitvalue) == bitvalue)
                {
                    select += 1 << i;
                }
            }
            int newselect = EditorGUI.MaskField(position, select, names);
            maskValue = 0;
            for (int i = 0; i < values.Length; i++)
            {
                int bitvalue = 1 << i;
                if ((newselect & bitvalue) == bitvalue)
                {
                    maskValue += (int)values.GetValue(i);
                }
            }
            property.intValue  = maskValue;
            */
        }
    }

    [Flags]
    public enum MatchLayer
    {
        Shader = 1,//匹配shader名
        ShaderType = 2,//匹配ShaderType
        PassType = 4,//匹配PassType
        ShaderCompilerPlatform = 8,//匹配ShaderCompilerPlatform
        GraphicsTier = 16,//GraphicsTier
        BuiltinShaderDefine = 32,//BuiltinShaderDefine
        ShaderRequirements = 64,//ShaderRequirements
        Keywords = 128,//Keywords
    }

    //
    // 摘要: 使用bit flags 替代unity ShaderType 类型 方便复选
    //     Identifies the stage in the rendering pipeline.
    [Flags]
    public enum StripShaderType
    {
        //
        // 摘要:
        //     Identifier for the vertex shader stage.
        Vertex = 2,
        //
        // 摘要:
        //     Identifier for the fragment shader stage.
        Fragment = 4,
        //
        // 摘要:
        //     Identifier for the geometry shader stage.
        Geometry = 8,
        //
        // 摘要:
        //     Identifier for the hull shader stage.
        Hull = 16,
        //
        // 摘要:
        //     Identifier for the domain shader stage.
        Domain = 32
    }
    
    //
    // 摘要:使用bit flags 替代unity PassType 类型 方便复选
    //     Shader pass type for Unity's lighting pipeline.
    [Flags]
    public enum StripPassType
    {
        //
        // 摘要:
        //     Regular shader pass that does not interact with lighting.
        Normal = 1,
        //
        // 摘要:
        //     Legacy vertex-lit shader pass.
        Vertex = 2,
        //
        // 摘要:
        //     Legacy vertex-lit shader pass, with mobile lightmaps.
        VertexLM = 4,
        //
        // 摘要:
        //     Legacy vertex-lit shader pass, with desktop (RGBM) lightmaps.
        VertexLMRGBM = 8,
        //
        // 摘要:
        //     Forward rendering base pass.
        ForwardBase = 16,
        //
        // 摘要:
        //     Forward rendering additive pixel light pass.
        ForwardAdd = 32,
        //
        // 摘要:
        //     Legacy deferred lighting (light pre-pass) base pass.
        LightPrePassBase = 64,
        //
        // 摘要:
        //     Legacy deferred lighting (light pre-pass) final pass.
        LightPrePassFinal = 128,
        //
        // 摘要:
        //     Shadow caster & depth texure shader pass.
        ShadowCaster = 256,
        //
        // 摘要:
        //     Deferred Shading shader pass.
        Deferred = 1024,
        //
        // 摘要:
        //     Shader pass used to generate the albedo and emissive values used as input to
        //     lightmapping.
        Meta = 2048,
        //
        // 摘要:
        //     Motion vector render pass.
        MotionVectors = 4096,
        //
        // 摘要:
        //     Custom scriptable pipeline.
        ScriptableRenderPipeline = 8192,
        //
        // 摘要:
        //     Custom scriptable pipeline when lightmode is set to default unlit or no light
        //     mode is set.
        ScriptableRenderPipelineDefaultUnlit = 16384
    }

    //
    // 摘要:使用bit flags 替代unity ShaderCompilerPlatform 类型 方便复选
    //     Shader compiler used to generate player data shader variants.
    [Flags]
    public enum StripShaderCompilerPlatform
    {
        //
        // 摘要:
        //     Provide a reasonable value for non initialized variables.
        None = 0,
        //
        // 摘要:
        //     Compiler used with Direct3D 11 and Direct3D 12 graphics API on Windows platforms.
        D3D = 16,
        //
        // 摘要:
        //     Compiler used with OpenGL ES 2.0 and WebGL 1.0 graphics APIs on Android, iOS,
        //     Windows and WebGL platforms.
        GLES20 = 32,
        //
        // 摘要:
        //     Compiler used with OpenGL ES 3.x and WebGL 2.0 graphics APIs on Android, iOS,
        //     Windows and WebGL platforms.
        GLES3x = 512,
        //
        // 摘要:
        //     Compiler used on PlayStation 4.
        PS4 = 2048,
        //
        // 摘要:
        //     Compiler used with Direct3D 11 graphics API on XBox One.
        XboxOneD3D11 = 4096,
        //
        // 摘要:
        //     Compiler used with Metal graphics API on macOS, iOS and tvOS platforms.
        Metal = 16384,
        //
        // 摘要:
        //     Compiler used with OpenGL core graphics API on macOS, Linux and Windows platforms.
        OpenGLCore = 32768,
        //
        // 摘要:
        //     Compiler used with Vulkan graphics API on Android, Linux and Windows platforms.
        Vulkan = 262144,
        //
        // 摘要:
        //     Compiler used on Nintendo Switch.
        Switch = 524288,
        //
        // 摘要:
        //     Compiler used with Direct3D 12 graphics API on XBox One.
        XboxOneD3D12 = 1048576
    }

    //
    // 摘要:使用bit flags 替代
    //     Graphics Tier. See Also: Graphics.activeTier.
    [Flags]
    public enum StripGraphicsTier
    {
        //
        // 摘要:
        //     The first graphics tier (Low) - corresponds to shader define UNITY_HARDWARE_TIER1.
        Tier1 = 1,
        //
        // 摘要:
        //     The second graphics tier (Medium) - corresponds to shader define UNITY_HARDWARE_TIER2.
        Tier2 = 2,
        //
        // 摘要:
        //     The third graphics tier (High) - corresponds to shader define UNITY_HARDWARE_TIER3.
        Tier3 = 4
    }

    //
    // 摘要:
    //     Defines set by editor when compiling shaders, depending on target platform and
    //     tier.
    [Flags]
    public enum StripBuiltinShaderDefine
    {
        //
        // 摘要:
        //     UNITY_NO_DXT5nm is set when compiling shader for platform that do not support
        //     DXT5NM, meaning that normal maps will be encoded in RGB instead.
        UNITY_NO_DXT5nm = 1,
        //
        // 摘要:
        //     UNITY_NO_RGBM is set when compiling shader for platform that do not support RGBM,
        //     so dLDR will be used instead.
        UNITY_NO_RGBM = 2,
        UNITY_USE_NATIVE_HDR = 4,
        //
        // 摘要:
        //     UNITY_ENABLE_REFLECTION_BUFFERS is set when deferred shading renders reflection
        //     probes in deferred mode. With this option set reflections are rendered into a
        //     per-pixel buffer. This is similar to the way lights are rendered into a per-pixel
        //     buffer. UNITY_ENABLE_REFLECTION_BUFFERS is on by default when using deferred
        //     shading, but you can turn it off by setting “No support” for the Deferred Reflections
        //     shader option in Graphics Settings. When the setting is off, reflection probes
        //     are rendered per-object, similar to the way forward rendering works.
        UNITY_ENABLE_REFLECTION_BUFFERS =8,
        //
        // 摘要:
        //     UNITY_FRAMEBUFFER_FETCH_AVAILABLE is set when compiling shaders for platforms
        //     where framebuffer fetch is potentially available.
        UNITY_FRAMEBUFFER_FETCH_AVAILABLE = 16,
        //
        // 摘要:
        //     UNITY_ENABLE_NATIVE_SHADOW_LOOKUPS enables use of built-in shadow comparison
        //     samplers on OpenGL ES 2.0.
        UNITY_ENABLE_NATIVE_SHADOW_LOOKUPS = 32,
        //
        // 摘要:
        //     UNITY_METAL_SHADOWS_USE_POINT_FILTERING is set if shadow sampler should use point
        //     filtering on iOS Metal.
        UNITY_METAL_SHADOWS_USE_POINT_FILTERING = 64,
        UNITY_NO_CUBEMAP_ARRAY = 128,
        //
        // 摘要:
        //     UNITY_NO_SCREENSPACE_SHADOWS is set when screenspace cascaded shadow maps are
        //     disabled.
        UNITY_NO_SCREENSPACE_SHADOWS = 256,
        //
        // 摘要:
        //     UNITY_USE_DITHER_MASK_FOR_ALPHABLENDED_SHADOWS is set when Semitransparent Shadows
        //     are enabled.
        UNITY_USE_DITHER_MASK_FOR_ALPHABLENDED_SHADOWS = 512,
        //
        // 摘要:
        //     UNITY_PBS_USE_BRDF1 is set if Standard Shader BRDF1 should be used.
        UNITY_PBS_USE_BRDF1 = 1024,
        //
        // 摘要:
        //     UNITY_PBS_USE_BRDF2 is set if Standard Shader BRDF2 should be used.
        UNITY_PBS_USE_BRDF2 = 2048,
        //
        // 摘要:
        //     UNITY_PBS_USE_BRDF3 is set if Standard Shader BRDF3 should be used.
        UNITY_PBS_USE_BRDF3 = 4096,
        //
        // 摘要:
        //     UNITY_NO_FULL_STANDARD_SHADER is set if Standard shader BRDF3 with extra simplifications
        //     should be used.
        UNITY_NO_FULL_STANDARD_SHADER = 8192,
        //
        // 摘要:
        //     UNITY_SPECCUBE_BLENDING is set if Reflection Probes Box Projection is enabled.
        UNITY_SPECCUBE_BOX_PROJECTION = 16384,
        //
        // 摘要:
        //     UNITY_SPECCUBE_BLENDING is set if Reflection Probes Blending is enabled.
        UNITY_SPECCUBE_BLENDING = 32768,
        //
        // 摘要:
        //     UNITY_ENABLE_DETAIL_NORMALMAP is set if Detail Normal Map should be sampled if
        //     assigned.
        UNITY_ENABLE_DETAIL_NORMALMAP = 65536,
        //
        // 摘要:
        //     SHADER_API_MOBILE is set when compiling shader for mobile platforms.
        SHADER_API_MOBILE = 131072,
        //
        // 摘要:
        //     SHADER_API_DESKTOP is set when compiling shader for "desktop" platforms.
        SHADER_API_DESKTOP = 262144,
        //
        // 摘要:
        //     UNITY_HARDWARE_TIER1 is set when compiling shaders for GraphicsTier.Tier1.
        UNITY_HARDWARE_TIER1 = 524288,
        //
        // 摘要:
        //     UNITY_HARDWARE_TIER2 is set when compiling shaders for GraphicsTier.Tier2.
        UNITY_HARDWARE_TIER2 = 1048576,
        //
        // 摘要:
        //     UNITY_HARDWARE_TIER3 is set when compiling shaders for GraphicsTier.Tier3.
        UNITY_HARDWARE_TIER3 = 2097152,
        //
        // 摘要:
        //     UNITY_COLORSPACE_GAMMA is set when compiling shaders for Gamma Color Space.
        UNITY_COLORSPACE_GAMMA = 4194304,
        //
        // 摘要:
        //     UNITY_LIGHT_PROBE_PROXY_VOLUME is set when Light Probe Proxy Volume feature is
        //     supported by the current graphics API and is enabled in the current Tier Settings(Graphics
        //     Settings).
        UNITY_LIGHT_PROBE_PROXY_VOLUME = 8388608,
        //
        // 摘要:
        //     UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS is set automatically for platforms
        //     that don't require full floating-point precision support in fragment shaders.
        UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS = 16777216,
        //
        // 摘要:
        //     UNITY_LIGHTMAP_DLDR_ENCODING is set when lightmap textures are using double LDR
        //     encoding to store the values in the texture.
        UNITY_LIGHTMAP_DLDR_ENCODING = 33554432,
        //
        // 摘要:
        //     UNITY_LIGHTMAP_RGBM_ENCODING is set when lightmap textures are using RGBM encoding
        //     to store the values in the texture.
        UNITY_LIGHTMAP_RGBM_ENCODING = 67108864,
        //
        // 摘要:
        //     UNITY_LIGHTMAP_FULL_HDR is set when lightmap textures are not using any encoding
        //     to store the values in the texture.
        UNITY_LIGHTMAP_FULL_HDR = 134217728
    }
    [System.Serializable]
    public class ShaderVariantsStripperFilter
    {
        [HideInInspector]
        public string shaderGuid;
      //  [MaskFlags]
        public MatchLayer mask;
        public string shaderName;
      //  [MaskFlags]
        public StripShaderType shaderType;
     //   [MaskFlags]
        public StripPassType passType;
     //   [MaskFlags]
        public StripShaderCompilerPlatform shaderCompilerPlatform;
     //   [MaskFlags]
        public StripGraphicsTier graphicsTier;
    //    [MaskFlags]
        public StripBuiltinShaderDefine builtinShaderDefine;
    //    [MaskFlags]
        public ShaderRequirements shaderRequirements;
        public List<string> keywords;

        public ShaderVariantsStripperFilter()
        {
            keywords = new List<string>();
        }

    }

    [System.Serializable]
    public class ShaderVariantsStripperConfigure : ScriptableObject
    {
        public bool useStripper = false;
        public bool enableLog = false;
        public bool useWhitelist = false;
        public ShaderVariantsStripperFilter defaultShaderVariantsStripperFilter;
        public List<ShaderVariantsStripperFilter> mFilters = new List<ShaderVariantsStripperFilter>();
        public List<ShaderVariantsStripperFilter> mWhitelist = new List<ShaderVariantsStripperFilter>();
        public static string mConfigurepath = string.Empty;
        public static string LogPath(string file)
        {
            string path = Application.dataPath + "/../ShaderStrip";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return GetPlatformPath(path, file);
        }

        public static void SetCurrentBuildTarget(BuildTarget buildTarget)
        {
            mCurrentBuildTarget =  buildTarget;
        }

        //保存的当前的平台
        static BuildTarget mCurrentBuildTarget = BuildTarget.Android;

        //获取配置文件路径 默认是当前活动编辑器的平台
        public static string GetPlatformPath(string head, string tail,bool useActiveBuildTarget=true)
        {
            BuildTarget buildTarget = useActiveBuildTarget ? EditorUserBuildSettings.activeBuildTarget : mCurrentBuildTarget;
            string targetDir = "";
            if (buildTarget == BuildTarget.iOS)
            {
                targetDir = "iOS";
            }
            else if (buildTarget == BuildTarget.Android)
            {
                targetDir = "Android";
            }
            else if (buildTarget == BuildTarget.StandaloneWindows64 || buildTarget == BuildTarget.StandaloneWindows)
            {
                targetDir = "Windows";
            }
            else if (buildTarget == BuildTarget.StandaloneOSX)
            {
                targetDir = "MacOS";
            }
            else
            {
                targetDir = "Default";
            }
            string rootPath = string.Format("{0}/{1}", head, targetDir);

            if (!Directory.Exists(rootPath))
            {
                Directory.CreateDirectory(rootPath);
            }
            rootPath = string.Format("{0}/{1}", rootPath, tail);
            return rootPath;
        }

        //获取配置文件路径 默认是当前活动编辑器的平台 useActiveBuildTarget为true主要适用于打包时
        public static string ConfigureFilePath(bool useActiveBuildTarget = true)
        {
            if (mConfigurepath == string.Empty)
            {
                string path = AssetDatabase.FindAssets("t:Script")
                   .Where(v => Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(v)) == "ShaderVariantsStripperConfigure")
                   .Select(id => AssetDatabase.GUIDToAssetPath(id))
                   .FirstOrDefault()
                   .ToString();
                mConfigurepath = path.Replace("/ShaderVariantsStripperConfigure.cs", "");
            }
            return GetPlatformPath(mConfigurepath, "strippingConfigure.asset", useActiveBuildTarget);
        }

        //编辑配置文件时 获取某个平台的配置文件
        public static ShaderVariantsStripperConfigure GetTargetConfigure(BuildTarget buildTarget)
        {
            if (mCurrentBuildTarget != buildTarget)
            {
                SetCurrentBuildTarget(buildTarget);
                configure = AssetDatabase.LoadAssetAtPath<ShaderVariantsStripperConfigure>(ConfigureFilePath(false));
            }
            if (configure == null)
            {
                configure = AssetDatabase.LoadAssetAtPath<ShaderVariantsStripperConfigure>(ConfigureFilePath(false));
                if (configure == null)
                {
                    configure = ScriptableObject.CreateInstance<ShaderVariantsStripperConfigure>();
                    AssetDatabase.CreateAsset(configure, ConfigureFilePath(false));
                }
            }
            return configure;
        }

        static ShaderVariantsStripperConfigure configure;
        public static ShaderVariantsStripperConfigure Configure
        {
            get
            {
                if (configure == null)
                {
                    configure = AssetDatabase.LoadAssetAtPath<ShaderVariantsStripperConfigure>(ConfigureFilePath());
                    // AssetDatabase.Refresh();
                    Debug.Log("configure configure configure " + configure);
                }
                if (configure == null)
                {
                    ShaderVariantsStripperConfigure configure = ScriptableObject.CreateInstance<ShaderVariantsStripperConfigure>();
                    AssetDatabase.CreateAsset(configure, ConfigureFilePath());
                    // AssetDatabase.Refresh();
                }
                return configure;
            }
        }

        public static void Save()
        {
            EditorUtility.SetDirty(Configure);
            AssetDatabase.SaveAssets();
        }

        public List<ShaderVariantsStripperFilter> GetFilters()
        {
            if (mFilters == null)
                mFilters = new List<ShaderVariantsStripperFilter>();
            return mFilters;
        }

        public List<ShaderVariantsStripperFilter> GetWhitelist()
        {
            if (mWhitelist == null)
                mWhitelist = new List<ShaderVariantsStripperFilter>();
            return mWhitelist;
        }
        public ShaderVariantsStripperConfigure()
        {

        }
    }

    public class StripTypeConvert
    {
        public static StripShaderType ConvertUnityTypeToStripType(UnityEditor.Rendering.ShaderType inType)
        {
            return (StripShaderType)Mathf.Pow(2, (int)inType);
        }

        public static StripPassType ConvertUnityTypeToStripType(UnityEngine.Rendering.PassType inType)
        {
            return (StripPassType)Mathf.Pow(2, (int)inType);
        }

        public static StripShaderCompilerPlatform ConvertUnityTypeToStripType(UnityEditor.Rendering.ShaderCompilerPlatform inType)
        {
            return (StripShaderCompilerPlatform)Mathf.Pow(2, (int)inType);
        }

        public static StripGraphicsTier ConvertUnityTypeToStripType(UnityEngine.Rendering.GraphicsTier inType)
        {
            return (StripGraphicsTier)Mathf.Pow(2, (int)inType);
        }

        public static StripBuiltinShaderDefine ConvertUnityTypeToStripType(UnityEngine.Rendering.BuiltinShaderDefine inType)
        {
            return (StripBuiltinShaderDefine)Mathf.Pow(2, (int)inType);
        }

        public static UnityEngine.Rendering.BuiltinShaderDefine[] ConvertStripTypeToUnityTypes(StripBuiltinShaderDefine inType)
        {
            List<UnityEngine.Rendering.BuiltinShaderDefine> list = new List<UnityEngine.Rendering.BuiltinShaderDefine>();
            Array array =  Enum.GetValues(typeof(StripBuiltinShaderDefine));
            foreach (int v in array)
            {
                if (((int)inType & v) == v)
                {
                    list.Add((UnityEngine.Rendering.BuiltinShaderDefine)Mathf.Log(v, 2));
                }
            }
            return list.ToArray();
        }
    }
}