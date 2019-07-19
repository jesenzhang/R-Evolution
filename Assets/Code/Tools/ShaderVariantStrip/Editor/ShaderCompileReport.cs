using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;
using System;

namespace ShaderVariantsStripper
{
    [System.Serializable]
    public class StripShaderSnippetData
    {
        public StripShaderType shaderType;
        public StripPassType passType;
        public string passName;

        public StripShaderSnippetData()
        {

        }

        public StripShaderSnippetData(UnityEditor.Rendering.ShaderSnippetData snippet)
        {
            shaderType = StripTypeConvert.ConvertUnityTypeToStripType(snippet.shaderType);
            passType = StripTypeConvert.ConvertUnityTypeToStripType(snippet.passType);
            passName = snippet.passName;
        }
    }

    [System.Serializable]
    public class StripShaderKeyword
    {
        public string keywordName;
        public UnityEngine.Rendering.ShaderKeywordType keywordType;

        public StripShaderKeyword(UnityEngine.Rendering.ShaderKeyword shaderKeyword)
        {
            keywordName = shaderKeyword.GetKeywordName();
            keywordType = shaderKeyword.GetKeywordType();
        }
        
    }
    

    [System.Serializable]
    public class StripShaderCompilerData
    {
        public List<StripShaderKeyword> shaderKeywordSet;
        [MaskFlags]
        public StripBuiltinShaderDefine platformKeywordSet;
        [MaskFlags]
        public UnityEditor.Rendering.ShaderRequirements shaderRequirements;
        public UnityEngine.Rendering.GraphicsTier graphicsTier;
        public UnityEditor.Rendering.ShaderCompilerPlatform shaderCompilerPlatform;

        //是否被剔除
        public bool isStripped;

        public StripShaderCompilerData()
        {
            if (shaderKeywordSet == null)
                shaderKeywordSet = new List<StripShaderKeyword>();
        }

        public StripShaderCompilerData(UnityEditor.Rendering.ShaderCompilerData shaderCompilerData, bool isStripped=false)
        {
            if (shaderKeywordSet == null)
                shaderKeywordSet = new List<StripShaderKeyword>();
            UnityEngine.Rendering.ShaderKeyword[] keywordSet = shaderCompilerData.shaderKeywordSet.GetShaderKeywords();
            for (int i = 0; i < keywordSet.Length; i++)
            {
                StripShaderKeyword shaderKeyword = new StripShaderKeyword(keywordSet[i]);
                shaderKeywordSet.Add(shaderKeyword);
            }
            Array array = Enum.GetValues(typeof(UnityEngine.Rendering.BuiltinShaderDefine));
            foreach (UnityEngine.Rendering.BuiltinShaderDefine v in array)
            {
                if (shaderCompilerData.platformKeywordSet.IsEnabled(v))
                {
                    StripBuiltinShaderDefine newdefine = StripTypeConvert.ConvertUnityTypeToStripType(v);
                    platformKeywordSet = platformKeywordSet | newdefine;
                }
            }
            shaderRequirements = shaderCompilerData.shaderRequirements;
            graphicsTier = shaderCompilerData.graphicsTier;
            shaderCompilerPlatform = shaderCompilerData.shaderCompilerPlatform;
           this.isStripped = isStripped;
        }

        public bool IsEquip(UnityEditor.Rendering.ShaderCompilerData shaderCompilerData)
        {
            bool equip = true;
            equip = equip && shaderRequirements == shaderCompilerData.shaderRequirements;
            if (!equip) return equip;
            equip = equip && graphicsTier == shaderCompilerData.graphicsTier;

            if (!equip) return equip;
            equip = equip && shaderCompilerPlatform == shaderCompilerData.shaderCompilerPlatform;

            if (!equip) return equip;
            if (equip)
                return equip;
            return false;
        }
        
        public List<string> builtinDefaultList = new List<string>();
        public List<string> builtinExtraList = new List<string>();
        public List<string> builtinAutoStrippedList = new List<string>();
        public List<string> userDefinedList = new List<string>();

        public void Summary()
        {
            if (builtinDefaultList == null)
                builtinDefaultList = new List<string>();
            builtinDefaultList.Clear();

            if (builtinExtraList == null)
                builtinExtraList = new List<string>();
            builtinExtraList.Clear();

            if (builtinAutoStrippedList == null)
                builtinAutoStrippedList = new List<string>();
            builtinAutoStrippedList.Clear();

            if (userDefinedList == null)
                userDefinedList = new List<string>();
            userDefinedList.Clear();

            foreach (var key in shaderKeywordSet)
            {
                if (key.keywordType == UnityEngine.Rendering.ShaderKeywordType.BuiltinDefault)
                {
                    if (!builtinDefaultList.Contains(key.keywordName))
                        builtinDefaultList.Add(key.keywordName);
                }
                if (key.keywordType == UnityEngine.Rendering.ShaderKeywordType.BuiltinExtra)
                {
                    if (!builtinExtraList.Contains(key.keywordName))
                        builtinExtraList.Add(key.keywordName);
                }
                if (key.keywordType == UnityEngine.Rendering.ShaderKeywordType.BuiltinAutoStripped)
                {
                    if (!builtinAutoStrippedList.Contains(key.keywordName))
                        builtinAutoStrippedList.Add(key.keywordName);
                }
                if (key.keywordType == UnityEngine.Rendering.ShaderKeywordType.UserDefined)
                {
                    if (!userDefinedList.Contains(key.keywordName))
                        userDefinedList.Add(key.keywordName);
                }
            }
        }
    }

    [System.Serializable]
    public class SnippetComileDataTuple
    {
        public StripShaderSnippetData snippet;
        public List<StripShaderCompilerData> shaderVariants;

        public SnippetComileDataTuple()
        {
        }

        public SnippetComileDataTuple(UnityEditor.Rendering.ShaderSnippetData snippet)
        {
            this.snippet = new StripShaderSnippetData(snippet);
            this.shaderVariants = new List<StripShaderCompilerData>();
        }

        public SnippetComileDataTuple(UnityEditor.Rendering.ShaderSnippetData snippet, UnityEditor.Rendering.ShaderCompilerData[] ashaderVariants)
        {
            this.snippet = new StripShaderSnippetData(snippet);
            int n = ashaderVariants.Length;
            this.shaderVariants = new List<StripShaderCompilerData>();
            for (int i = 0; i < n; i++)
            {
                this.shaderVariants.Add(new StripShaderCompilerData(ashaderVariants[i]));
            }
        }

        public SnippetComileDataTuple(UnityEditor.Rendering.ShaderSnippetData snippet, UnityEditor.Rendering.ShaderCompilerData ashaderVariant,bool isStripped = false)
        {
            this.snippet = new StripShaderSnippetData(snippet);
            this.shaderVariants = new List<StripShaderCompilerData>();
            this.shaderVariants.Add(new StripShaderCompilerData(ashaderVariant, isStripped));
        }
        
        public StripShaderCompilerData GetStripShaderCompilerData(UnityEditor.Rendering.ShaderCompilerData shaderCompilerData)
        {
            if (this.shaderVariants != null)
            {
                int n = this.shaderVariants.Count;
                for (int i = 0; i < n; i++)
                {
                    if (this.shaderVariants[i].IsEquip(shaderCompilerData))
                    {
                        return this.shaderVariants[i];
                    }
                }
            }
            return null;
        }

        public void AddStripShaderCompilerData(StripShaderCompilerData stripShaderCompilerData)
        {
            if (this.shaderVariants != null)
            {
                if (!this.shaderVariants.Contains(stripShaderCompilerData))
                {
                    this.shaderVariants.Add(stripShaderCompilerData);
                }
            }
        }

        public void AddShaderCompilerData(UnityEditor.Rendering.ShaderCompilerData shaderCompilerData,bool isStripped = false)
        {
            if (this.shaderVariants != null)
            {
                StripShaderCompilerData a = new StripShaderCompilerData(shaderCompilerData);
                a.isStripped = isStripped;
                this.shaderVariants.Add(a);
            }
        }
    }

    [System.Serializable]
    public class ShaderCompileVariantInfo
    {
        public string shaderName;
        //关键字总数
        public int keywordsCount;
        //变体总数
        public int totalVaraints;
        //剔除的变体数
        public int strippedVaraints;

        public List<SnippetComileDataTuple> snippetComileDatas;

        public ShaderCompileVariantInfo(string shaderName)
        {
            this.shaderName = shaderName;
            snippetComileDatas = new List<SnippetComileDataTuple>();
            keyWordList = new List<string>();
        }

        public List<string> keyWordList;
        public List<string> builtinDefaultList = new List<string>();
        public List<string> builtinExtraList = new List<string>();
        public List<string> builtinAutoStrippedList = new List<string>();
        public List<string> userDefinedList = new List<string>();

        public void Summary()
        {
            keywordsCount = 0; 
            totalVaraints=0;
            strippedVaraints=0;
            if(keyWordList==null)
                keyWordList = new List<string>();
            keyWordList.Clear();

            if (builtinDefaultList == null)
                builtinDefaultList = new List<string>();
            builtinDefaultList.Clear();

            if (builtinExtraList == null)
                builtinExtraList = new List<string>();
            builtinExtraList.Clear();

            if (builtinAutoStrippedList == null)
                builtinAutoStrippedList = new List<string>();
            builtinAutoStrippedList.Clear();

            if (userDefinedList == null)
                userDefinedList = new List<string>();
            userDefinedList.Clear();

            foreach (var tup in snippetComileDatas)
            {
                foreach (var variant in tup.shaderVariants)
                {
                    totalVaraints++;
                    if (variant.isStripped)
                        strippedVaraints++;
                    foreach (var key in variant.shaderKeywordSet)
                    {
                        variant.Summary();
                        if (key.keywordType == UnityEngine.Rendering.ShaderKeywordType.BuiltinDefault)
                        {
                            if(!builtinDefaultList.Contains(key.keywordName))
                                builtinDefaultList.Add(key.keywordName);
                        }
                        if (key.keywordType == UnityEngine.Rendering.ShaderKeywordType.BuiltinExtra)
                        {
                            if (!builtinExtraList.Contains(key.keywordName))
                                builtinExtraList.Add(key.keywordName);
                        }
                        if (key.keywordType == UnityEngine.Rendering.ShaderKeywordType.BuiltinAutoStripped)
                        {
                            if (!builtinAutoStrippedList.Contains(key.keywordName))
                                builtinAutoStrippedList.Add(key.keywordName);
                        }
                        if (key.keywordType == UnityEngine.Rendering.ShaderKeywordType.UserDefined)
                        {
                            if (!userDefinedList.Contains(key.keywordName))
                                userDefinedList.Add(key.keywordName);
                        }
                        if (!keyWordList.Contains(key.keywordName))
                        {
                            keyWordList.Add(key.keywordName);
                        }
                    }
                }
            }
            keywordsCount = keyWordList.Count;
        }

        public SnippetComileDataTuple GetSnippetComileDataTuple(UnityEditor.Rendering.ShaderSnippetData snippet)
        {
            if (snippetComileDatas != null)
            {
                for (int i = 0; i < snippetComileDatas.Count; i++)
                {
                    SnippetComileDataTuple t = snippetComileDatas[i];
                    if (t != null)
                    {
                        if (t.snippet.passName == snippet.passName && t.snippet.passType == StripTypeConvert.ConvertUnityTypeToStripType(snippet.passType) && t.snippet.shaderType == StripTypeConvert.ConvertUnityTypeToStripType(snippet.shaderType))
                        {
                            return t;
                        }
                    }
                }
            }
            return null;
        }

        public void AddSnippetComileDataTuple(SnippetComileDataTuple tuple)
        {
            if (snippetComileDatas != null)
            {
                if (!snippetComileDatas.Contains(tuple))
                {
                    snippetComileDatas.Add(tuple);
                }
            }
        }

        public void AddSnippetComileDataTuple(UnityEditor.Rendering.ShaderSnippetData snippet, UnityEditor.Rendering.ShaderCompilerData[] shaderVariants)
        {
            SnippetComileDataTuple tuple = new SnippetComileDataTuple(snippet, shaderVariants);
            AddSnippetComileDataTuple(tuple);
        }

        public void AddSnippetComileDataTuple(UnityEditor.Rendering.ShaderSnippetData snippet, UnityEditor.Rendering.ShaderCompilerData shaderVariant, bool isStripped = false)
        {
            SnippetComileDataTuple tuple =  GetSnippetComileDataTuple(snippet);
            if (tuple != null)
            {
                tuple.AddShaderCompilerData(shaderVariant, isStripped);
            }
            else
            {
                tuple = new SnippetComileDataTuple(snippet, shaderVariant, isStripped);
                AddSnippetComileDataTuple(tuple);
            }
        }
    }

    [System.Serializable]
    public class ShaderCompileReport : ScriptableObject
    {
        public static string mConfigurepath = string.Empty;
        //保存的当前的平台
        static BuildTarget mCurrentBuildTarget = BuildTarget.Android;
        //获取配置文件路径 默认是当前活动编辑器的平台
        public static string GetPlatformPath(string head, string tail, bool useActiveBuildTarget = true)
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
        public static string ConfigureFilePath(bool useActiveBuildTarget = true)
        {
            if (mConfigurepath == string.Empty)
            {
                string path = AssetDatabase.FindAssets("t:Script")
                   .Where(v => Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(v)) == "ShaderCompileReport")
                   .Select(id => AssetDatabase.GUIDToAssetPath(id))
                   .FirstOrDefault()
                   .ToString();
                string head = path.Replace("/ShaderCompileReport.cs", "");

                mConfigurepath = GetPlatformPath(head, "shadercompilereport.asset");
            }
            return mConfigurepath;
        }
        public static void SetCurrentBuildTarget(BuildTarget buildTarget)
        {
            mCurrentBuildTarget = buildTarget;
        }
        //编辑配置文件时 获取某个平台的配置文件
        public static ShaderCompileReport GetTargetConfigure(BuildTarget buildTarget)
        {
            if (mCurrentBuildTarget != buildTarget)
            {
                mCurrentBuildTarget = buildTarget;
                report = AssetDatabase.LoadAssetAtPath<ShaderCompileReport>(ConfigureFilePath(false));
            }
            if (report == null)
            {
                report = AssetDatabase.LoadAssetAtPath<ShaderCompileReport>(ConfigureFilePath(false));
                if (report == null)
                {
                    report = ScriptableObject.CreateInstance<ShaderCompileReport>();
                    AssetDatabase.CreateAsset(report, ConfigureFilePath(false));
                }
            }
            return report;
        }
        static ShaderCompileReport report;
        public static ShaderCompileReport Report
        {
            get
            {
                if (report == null)
                {
                    report = AssetDatabase.LoadAssetAtPath<ShaderCompileReport>(ConfigureFilePath());
                }
                if (report == null)
                {
                    ShaderCompileReport report = ScriptableObject.CreateInstance<ShaderCompileReport>();
                    AssetDatabase.CreateAsset(report, ConfigureFilePath());
                }
                return report;
            }
        }
        
        public List<ShaderCompileVariantInfo> infos;

        public void AddCompileInfo(string shaderName, UnityEditor.Rendering.ShaderSnippetData snippet, UnityEditor.Rendering.ShaderCompilerData[] shaderVariants)
        {
            if (infos == null)
            {
                infos = new List<ShaderCompileVariantInfo>();
            }
            for (int i = 0; i < infos.Count; i++)
            {
                if (infos[i].shaderName == shaderName)
                {
                    infos[i].AddSnippetComileDataTuple(snippet, shaderVariants);
                    return;
                }
            }
            ShaderCompileVariantInfo info = new ShaderCompileVariantInfo(shaderName);
            info.AddSnippetComileDataTuple(snippet, shaderVariants);
            infos.Add(info);
        }

        public void AddCompileInfo(string shaderName, UnityEditor.Rendering.ShaderSnippetData snippet, UnityEditor.Rendering.ShaderCompilerData shaderVariant, bool isStripped = false)
        {
            if (infos == null)
            {
                infos = new List<ShaderCompileVariantInfo>();
            }
            for (int i = 0; i < infos.Count; i++)
            {
                if (infos[i].shaderName == shaderName)
                {
                    infos[i].AddSnippetComileDataTuple(snippet, shaderVariant, isStripped);
                    return;
                }
            }
            ShaderCompileVariantInfo info = new ShaderCompileVariantInfo(shaderName);
            info.AddSnippetComileDataTuple(snippet,  shaderVariant, isStripped);
            infos.Add(info);
        }

        public ShaderCompileVariantInfo GetInfo(string shaderName)
        {
            if (infos == null)
            {
                return null;
            }
            for (int i = 0; i < infos.Count; i++)
            {
                if (infos[i].shaderName == shaderName)
                {
                    return infos[i];
                }
            }
            return null;
        }

        public static void Save()
        {
            EditorUtility.SetDirty(report); 
            AssetDatabase.SaveAssets();
        }
    }
}