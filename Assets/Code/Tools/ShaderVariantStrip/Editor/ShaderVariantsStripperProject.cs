using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor.Build;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;
namespace ShaderVariantsStripper
{
    // Simple example of stripping of a shader variants of 'ShaderVariantsStripping' shader.
    class ShaderVariantsStripperProject : IPreprocessShaders
    {
        static string logFile = ShaderVariantsStripperConfigure.LogPath("BeforeShaderVariantStrippingLog.txt");
        static string keywordFile = ShaderVariantsStripperConfigure.LogPath("BeforeShaderKeyWords.txt");
        static string allshaderFile = ShaderVariantsStripperConfigure.LogPath("AllShaders.txt");
        HashSet<string> keySets = new HashSet<string>();
        HashSet<string> shaderSets = new HashSet<string>();
        public static StringBuilder StrBuilder = new StringBuilder();

        static string buildInFile = ShaderVariantsStripperConfigure.LogPath("BuildInKeywords.txt");
        static string buildExtraFile = ShaderVariantsStripperConfigure.LogPath("BuildExtraKeywords.txt");
        static string buildInAutoStripFile = ShaderVariantsStripperConfigure.LogPath("BuildInAutoStripKeywords.txt");
        static string userDefineFile = ShaderVariantsStripperConfigure.LogPath("UserDefineKeywords.txt");

        HashSet<string> buildInKeys = new HashSet<string>();
        HashSet<string> buildInExtraKeys = new HashSet<string>();
        HashSet<string> buildInAutoStripKeys = new HashSet<string>();
        HashSet<string> userDefineKeys = new HashSet<string>();

        public ShaderVariantsStripperProject()
        {
            File.Delete(logFile);
            File.Create(logFile);
            File.Delete(keywordFile);
            File.Create(keywordFile);
            File.Delete(allshaderFile);
            File.Create(allshaderFile);

            File.Delete(buildInFile);
            File.Create(buildInFile);
            File.Delete(buildExtraFile);
            File.Create(buildExtraFile);
            File.Delete(buildInAutoStripFile);
            File.Create(buildInAutoStripFile);
            File.Delete(userDefineFile);
            File.Create(userDefineFile);
        }

        public int callbackOrder { get { return (int)ShaderVariantsStripperOrder.Project; } }

        public bool KeepVariantByConfigure(Shader shader, ShaderSnippetData snippet, ShaderCompilerData shaderVariant)
        {
            List<ShaderVariantsStripperFilter> filters = ShaderVariantsStripperConfigure.Configure.GetFilterList();

            if (shader.name == "LDJ/Standard")
            {
                Debug.Log("ss");
            }
            bool result = true;
            foreach (var filter in filters)
            {
                int filterresult = 0;
                //着色器名称
                if ((filter.mask & MatchLayer.Shader) == MatchLayer.Shader)
                {
                    // MatchLayer.Shader is allowed...
                    int ret = filter.shaderName == shader.name ? (int)(MatchLayer.Shader) : 0;
                    filterresult |= ret;
                }
                //着色器类型
                if ((filter.mask & MatchLayer.ShaderType) == MatchLayer.ShaderType)
                {
                    StripShaderType shaderType = StripTypeConvert.ConvertUnityTypeToStripType(snippet.shaderType);
                    int ret = ((filter.shaderType & shaderType) == shaderType) ? (int)(MatchLayer.ShaderType) : 0;
                    filterresult |= ret;
                }
                //passType
                if ((filter.mask & MatchLayer.PassType) == MatchLayer.PassType)
                {
                    StripPassType passType = StripTypeConvert.ConvertUnityTypeToStripType(snippet.passType);
                    int ret = ((filter.passType & passType) == passType) ? (int)(MatchLayer.PassType) : 0;
                    filterresult |= ret;
                }
                //shaderCompilerPlatform
                if ((filter.mask & MatchLayer.ShaderCompilerPlatform) == MatchLayer.ShaderCompilerPlatform)
                {
                    StripShaderCompilerPlatform shaderCompilerPlatform = StripTypeConvert.ConvertUnityTypeToStripType(shaderVariant.shaderCompilerPlatform);
                    int ret = ((filter.shaderCompilerPlatform & shaderCompilerPlatform) == shaderCompilerPlatform) ? (int)(MatchLayer.ShaderCompilerPlatform) : 0;
                    filterresult |= ret;
                }
                //GraphicsTier
                if ((filter.mask & MatchLayer.GraphicsTier) == MatchLayer.GraphicsTier)
                {
                    StripGraphicsTier graphicsTier = StripTypeConvert.ConvertUnityTypeToStripType(shaderVariant.graphicsTier);
                    int ret = ((filter.graphicsTier & graphicsTier) == graphicsTier) ? (int)(MatchLayer.GraphicsTier) : 0;
                    filterresult |= ret;
                }

                //builtinShaderDefine 黑名单有任意一种enable 就认为不通过
                if ((filter.mask & MatchLayer.BuiltinShaderDefine) == MatchLayer.BuiltinShaderDefine)
                {
                    UnityEngine.Rendering.BuiltinShaderDefine[] builtinShaderDefine = StripTypeConvert.ConvertStripTypeToUnityTypes(filter.builtinShaderDefine);
                    bool has = false;
                    for (int i = 0; i < builtinShaderDefine.Length; i++)
                    {
                        if (shaderVariant.platformKeywordSet.IsEnabled(builtinShaderDefine[i]))
                        {
                            has = true;
                            break;
                        }
                    }
                    int ret = has ? (int)(MatchLayer.BuiltinShaderDefine) : 0;
                    filterresult |= ret;
                }

                //ShaderRequirements
                if ((filter.mask & MatchLayer.ShaderRequirements) == MatchLayer.ShaderRequirements)
                {
                    int ret = (filter.shaderRequirements & shaderVariant.shaderRequirements) == filter.shaderRequirements ? (int)(MatchLayer.ShaderRequirements) : 0;
                    filterresult |= ret;
                }

                //Keywords
                if ((filter.mask & MatchLayer.Keywords) == MatchLayer.Keywords)
                {
                    //使用黑名单中的任意一个就剔除
                    bool ret = false;
                    foreach (var key in filter.keywords)
                    {
                        ShaderKeyword shaderKeyword = new ShaderKeyword(key);
                        ret = ret || shaderVariant.shaderKeywordSet.IsEnabled(shaderKeyword);
                        if (ret)
                            break;
                    }
                    int bitret = ret ? (int)(MatchLayer.Keywords) : 0;
                    filterresult |= bitret;
                }
                if (((int)filter.mask & filterresult) == (int)filter.mask)
                {
                    result = false;
                    break;
                }
                else
                {
                    continue;
                }
            }

            return result;
        }

        public bool KeepVariantByWhitelist(Shader shader, ShaderSnippetData snippet, ShaderCompilerData shaderVariant)
        {
            List<ShaderVariantsStripperFilter> filters = ShaderVariantsStripperConfigure.Configure.GetFilterList();
            bool result = false;
            foreach (var filter in filters)
            {
                int filterresult = 0;
                //着色器名称
                if ((filter.mask & MatchLayer.Shader) == MatchLayer.Shader)
                {
                    int ret = filter.shaderName == shader.name ? (int)(MatchLayer.Shader) : 0;
                    //须要匹配名称 当前过滤条件的名称不符 跳过
                    if (ret == 0)
                    {
                        continue;
                    }
                    filterresult |= ret;
                }
                //着色器类型
                if ((filter.mask & MatchLayer.ShaderType) == MatchLayer.ShaderType)
                {
                    StripShaderType shaderType = StripTypeConvert.ConvertUnityTypeToStripType(snippet.shaderType);
                    int ret = ((filter.shaderType & shaderType) == shaderType) ? (int)(MatchLayer.ShaderType) : 0;
                    filterresult |= ret;
                }
                //passType
                if ((filter.mask & MatchLayer.PassType) == MatchLayer.PassType)
                {
                    StripPassType passType = StripTypeConvert.ConvertUnityTypeToStripType(snippet.passType);
                    int ret = ((filter.passType & passType) == passType) ? (int)(MatchLayer.PassType) : 0;
                    filterresult |= ret;
                }
                //shaderCompilerPlatform
                if ((filter.mask & MatchLayer.ShaderCompilerPlatform) == MatchLayer.ShaderCompilerPlatform)
                {
                    StripShaderCompilerPlatform shaderCompilerPlatform = StripTypeConvert.ConvertUnityTypeToStripType(shaderVariant.shaderCompilerPlatform);
                    int ret = ((filter.shaderCompilerPlatform & shaderCompilerPlatform) == shaderCompilerPlatform) ? (int)(MatchLayer.ShaderCompilerPlatform) : 0;
                    filterresult |= ret;
                }
                //GraphicsTier
                if ((filter.mask & MatchLayer.GraphicsTier) == MatchLayer.GraphicsTier)
                {
                    StripGraphicsTier graphicsTier = StripTypeConvert.ConvertUnityTypeToStripType(shaderVariant.graphicsTier);
                    int ret = ((filter.graphicsTier & graphicsTier) == graphicsTier) ? (int)(MatchLayer.GraphicsTier) : 0;
                    filterresult |= ret;
                }

                //builtinShaderDefine
                if ((filter.mask & MatchLayer.BuiltinShaderDefine) == MatchLayer.BuiltinShaderDefine)
                {
                    //所有enable的key 有一个不在列表里的 就认为不通过
                    UnityEngine.Rendering.BuiltinShaderDefine[] builtinShaderDefine = StripTypeConvert.ConvertStripTypeToUnityTypes(filter.builtinShaderDefine);
                    List<UnityEngine.Rendering.BuiltinShaderDefine> enableList = new List<UnityEngine.Rendering.BuiltinShaderDefine>(builtinShaderDefine);
                    bool through = true;
                    Array array = Enum.GetValues(typeof(UnityEngine.Rendering.BuiltinShaderDefine));
                    foreach (UnityEngine.Rendering.BuiltinShaderDefine v in array)
                    {
                        if (shaderVariant.platformKeywordSet.IsEnabled(v))
                        {
                            if (!enableList.Contains(v))
                            {
                                through = false;
                                break;
                            }
                        }
                    }
                    int ret = through ? (int)(MatchLayer.BuiltinShaderDefine) : 0;
                    filterresult |= ret;
                }

                //ShaderRequirements
                if ((filter.mask & MatchLayer.ShaderRequirements) == MatchLayer.ShaderRequirements)
                {
                    int ret = (filter.shaderRequirements & shaderVariant.shaderRequirements) == filter.shaderRequirements ? (int)(MatchLayer.ShaderRequirements) : 0;
                    filterresult |= ret;
                }

                //Keywords
                if ((filter.mask & MatchLayer.Keywords) == MatchLayer.Keywords)
                {
                    bool ret = true;
                    //当前变体用到的key
                    ShaderKeyword[] allkeys = shaderVariant.shaderKeywordSet.GetShaderKeywords();
                    foreach (ShaderKeyword akey in allkeys)
                    {
                        bool enable = shaderVariant.shaderKeywordSet.IsEnabled(akey);
                        if (enable)
                        {
                            string keyWordName = akey.GetKeywordName();
#if UNITY_2018_3_OR_NEWER
                            keyWordName = akey.GetKeywordName();
#else
                             keyWordName = akey.GetName();
#endif
                            bool include = filter.keywords.Contains(keyWordName);
                            ret = ret && include;
                            //使用了过滤名单以外的自定义key 跳出for 剔除变体
                            if (ret == false)
                            {
                                break;
                            }
                        }
                        else
                        {
                            continue;
                        }
                    }
                    int bitret = ret ? (int)(MatchLayer.Keywords) : 0;
                    filterresult |= bitret;
                }

                //有一个通过就是保留的
                if (((int)filter.mask & filterresult) == (int)filter.mask)
                {
                    result = true;
                    break;
                }
            }

            return result;
        }

        public void OnProcessShader(Shader shader, ShaderSnippetData snippet, IList<ShaderCompilerData> shaderVariants)
        {
            if (!ShaderVariantsStripperConfigure.Configure.useStripper)
            {
                return;
            }

            int inputShaderVariantCount = shaderVariants.Count;
            string prefix = "VARIANT: " + shader.name + " (";

            if (snippet.passName.Length > 0)
                prefix += snippet.passName + ", ";

            prefix += snippet.shaderType.ToString() + ") ";
            if (ShaderVariantsStripperConfigure.Configure.enableLog)
            {
                if (shaderSets.Add(shader.name))
                {
                    File.AppendAllText(allshaderFile, shader.name + "\n");
                }
            }
            for (int i = 0; i < shaderVariants.Count; ++i)
            {
                string log = prefix;

                log += shaderVariants[i].shaderCompilerPlatform.ToString() + "  ";
                log += shaderVariants[i].graphicsTier.ToString() + "  ";

                if (ShaderVariantsStripperConfigure.Configure.enableLog)
                {
                    ShaderKeyword[] keywords = shaderVariants[i].shaderKeywordSet.GetShaderKeywords();
                    for (int labelIndex = 0; labelIndex < keywords.Count(); ++labelIndex)
                    {
                        ShaderKeyword akey = keywords[labelIndex];
                        bool isUserDefined = akey.GetKeywordType() == ShaderKeywordType.UserDefined;
                        bool isBuiltinDefault = akey.GetKeywordType() == ShaderKeywordType.BuiltinDefault;
                        bool isBuiltinExtra = akey.GetKeywordType() == ShaderKeywordType.BuiltinExtra;
                        bool isBuiltinAutoStripped = akey.GetKeywordType() == ShaderKeywordType.BuiltinAutoStripped;

#if UNITY_2018_3_OR_NEWER
                        string keyWordName = akey.GetKeywordName();
#else
                    string keyWordName = akey.GetName();
#endif
                        log += keyWordName + " ";
                        {
                            if (keySets.Add(keyWordName))
                            {
                                File.AppendAllText(keywordFile, keyWordName + "\n");
                            }
                        }

                        if (isBuiltinDefault)
                        {
                            if (!buildInKeys.Contains(keyWordName))
                            {
                                buildInKeys.Add(keyWordName);
                                File.AppendAllText(buildInFile, keyWordName + "\n");
                            }
                        }
                        if (isBuiltinExtra)
                        {
                            if (!buildInExtraKeys.Contains(keyWordName))
                            {
                                buildInExtraKeys.Add(keyWordName);
                                File.AppendAllText(buildExtraFile, keyWordName + "\n");
                            }
                        }
                        if (isBuiltinAutoStripped)
                        {
                            if (!buildInAutoStripKeys.Contains(keyWordName))
                            {
                                buildInAutoStripKeys.Add(keyWordName);
                                File.AppendAllText(buildInAutoStripFile, keyWordName + "\n");
                            }
                        }
                        if (isUserDefined)
                        {
                            if (!userDefineKeys.Contains(keyWordName))
                            {
                                userDefineKeys.Add(keyWordName);
                                File.AppendAllText(userDefineFile, keyWordName + "\n");
                            }
                        }

                    }
                }

                bool keepVariant = true;
                if (ShaderVariantsStripperConfigure.Configure.useWhitelist)
                {
                    keepVariant = KeepVariantByWhitelist(shader, snippet, shaderVariants[i]);
                }
                else
                {
                    keepVariant = KeepVariantByConfigure(shader, snippet, shaderVariants[i]);
                }

                if (ShaderVariantsStripperConfigure.Configure.enableLog)
                {
                    //填写编译结果
                    ShaderCompileReport.Report.AddCompileInfo(shader.name, snippet, shaderVariants[i], !keepVariant);
                }

                if (!keepVariant)
                {
                    shaderVariants.RemoveAt(i);
                    --i;
                }
                if (ShaderVariantsStripperConfigure.Configure.enableLog)
                {
                    File.AppendAllText(logFile, log + "\n");
                }


            }


            if (ShaderVariantsStripperConfigure.Configure.enableLog)
            {
                float percentage = (float)shaderVariants.Count / (float)inputShaderVariantCount * 100f;
                string logresult = "STRIPPING(" + shader.name + " " + snippet.passName + " " + snippet.shaderType.ToString() + ") = Kept / Total = " + shaderVariants.Count + " / " + inputShaderVariantCount + " = " + percentage + "% of the generated shader variants remain in the player data";
                //Debug.Log(BText(logresult));
                File.AppendAllText(logFile, logresult + "\n");
            }

        }

        private string BText(string text) { return "<color=#44f>" + text + "</color>"; }
    }

}