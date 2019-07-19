using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;
namespace ShaderVariantsStripper
{
    class ShaderVariantsStripperLog : IPreprocessShaders
    {
        static bool enableLogOnly = false;

        static string logFile = ShaderVariantsStripperConfigure.LogPath("AfterShaderVariantStrippingLog.txt");
        static string keywordFile = ShaderVariantsStripperConfigure.LogPath("AfterShaderKeyWords.txt");
        HashSet<string> keySets = new HashSet<string>();

        public ShaderVariantsStripperLog()
        {
            File.Delete(logFile);
            File.Create(logFile);
            File.Delete(keywordFile);
            File.Create(keywordFile);

        }

        public int callbackOrder { get { return (int)ShaderVariantsStripperOrder.Log; } }

        public void OnProcessShader(Shader shader, ShaderSnippetData snippet, IList<ShaderCompilerData> data)
        {
            if (!ShaderVariantsStripperConfigure.Configure.useStripper)
            {
                return;
            }
            string prefix = "VARIANT: " + shader.name + " (";
            if (snippet.passName.Length > 0)
                prefix += snippet.passName + ", ";

            prefix += snippet.shaderType.ToString() + ") ";

            for (int i = 0; i < data.Count; ++i)
            {
                string log = prefix;
                log += data[i].shaderCompilerPlatform.ToString() + "  ";
                log += data[i].graphicsTier.ToString() + "  ";

                ShaderKeyword[] keywords = data[i].shaderKeywordSet.GetShaderKeywords();
                for (int labelIndex = 0; labelIndex < keywords.Count(); ++labelIndex)
                {
#if UNITY_2018_3_OR_NEWER
                    string keyWordName = keywords[labelIndex].GetKeywordName();
#else
                string keyWordName = keywords[labelIndex].GetName();
#endif
                    log += keyWordName + " ";
                    if (ShaderVariantsStripperConfigure.Configure.enableLog)
                    {
                        if (keySets.Add(keyWordName))
                        {
                            File.AppendAllText(keywordFile, keyWordName + "\n");
                        }
                    }
                }
                if (ShaderVariantsStripperConfigure.Configure.enableLog)
                {
                    // Debug.Log(GText(log));
                    File.AppendAllText(logFile, log + "\n");
                }
            }
            if (enableLogOnly)
                data.Clear();

        }

        private string GText(string text) { return "<color=#0f0>" + text + "</color>"; }
        private string YText(string text) { return "<color=#ff0>" + text + "</color>"; }
        private string RText(string text) { return "<color=#f80>" + text + "</color>"; }
    }
}