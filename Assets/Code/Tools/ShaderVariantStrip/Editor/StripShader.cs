using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class StripShader
{
    //着色器文件名
    public string name = "";
    //unity着色器名称 如LDJ/PBRStand
    public string shaderName = "";
    public string path = "";
    public string GUID = "";
    //包含的pass
    public List<StripShaderPass> passes = new List<StripShaderPass>();
    //包含的关键字
    public List<StripKeyword> keywords = new List<StripKeyword>();
    //被引用的材质
    public List<StripMaterial> materials = new List<StripMaterial>();
    //是否有引用的材质
    public bool isMatUseKeyword;
    //全部变体数量（包括未被使用的关键字） 实际编译变体数 enabled关键字
    public int totalVariantCount, actualBuildVariantCount, keywordEnabledCount;

    //用于记录编辑器窗口是否打开
    public bool foldout;
    public bool showMaterials;
    public bool pendingChanges;
    public bool editedByShaderControl;
    public bool hasBackup;
    public bool isReadOnly;
    public bool inStripFilterList;
    public bool showing;
    public bool selected;

    public void Add(StripShaderPass pass)
    {
        passes.Add(pass);
        UpdateKeywords();
    }

    public void AddKeywordsByName(string[] names)
    {
        bool changes = false;
        for (int k = 0; k < names.Length; k++)
        {
            int kwCount = keywords.Count;
            bool repeated = false;
            for (int j = 0; j < kwCount; j++)
            {
                if (keywords[j].name.Equals(names[k]))
                {
                    repeated = true;
                    break;
                }
            }
            if (repeated) continue;
            StripKeyword keyword = new StripKeyword(names[k]);
            keywords.Add(keyword);
            changes = true;
        }
        if (changes)
        {
            keywords.Sort(delegate (StripKeyword k1, StripKeyword k2) { return k1.name.CompareTo(k2.name); });
        }
    }

    public void RemoveKeyword(string name)
    {
        for (int k = 0; k < keywords.Count; k++)
        {
            StripKeyword keyword = keywords[k];
            if (keyword.name.Equals(name))
            {
                if (keyword.enabled) keywordEnabledCount--;
                keywords.Remove(keyword);
                return;
            }
        }
    }

    public void EnableKeywords()
    {
        keywords.ForEach((StripKeyword keyword) => keyword.enabled = true);
    }

    public List<string> enabledKeywords
    {
        get
        {
            List<string> kk = new List<string>(keywords.Count);
            keywords.ForEach(kw => { if (kw.enabled) kk.Add(kw.name); });
            return kk;
        }
    }

    public bool hasSource
    {
        get { return path.Length > 0; }
    }

    void UpdateKeywords()
    {
        for (int i = 0; i < passes.Count; i++)
        {
            StripShaderPass pass = passes[i];
            for (int l = 0; l < pass.keywordLines.Count; l++)
            {
                StripKeywordLine line = pass.keywordLines[l];
                for (int k = 0; k < line.keywords.Count; k++)
                {
                    StripKeyword keyword = line.keywords[k];
                    if (!keywords.Contains(keyword))
                    {
                        keywords.Add(keyword);
                    }
                }
            }
        }
    }

    public StripKeyword GetKeyword(string name)
    {
        int kCount = keywords.Count;
        for (int k = 0; k < kCount; k++)
        {
            StripKeyword keyword = keywords[k];
            if (keyword.name.Equals(name))
                return keyword;
        }
        return new StripKeyword(name);
    }

    //更新变体数量 可以通过变体公式计算 另外是否可通过编译出的文件计算？ 
    public void UpdateVariantCount()
    {
        totalVariantCount = 0;
        actualBuildVariantCount = 0;
        //遍历pass
        for (int i = 0; i < passes.Count; i++)
        {
            StripShaderPass pass = passes[i];
            int matCount = materials.Count;
            int pasStripCount = 1;
            int passBuildCount = 1;
            //遍历编译指令行
            for (int l = 0; l < pass.keywordLines.Count; l++)
            {
                StripKeywordLine line = pass.keywordLines[l];
                //kLineEnabledCount 所有enable的keyword  有没有 _ 
                int kLineEnabledCount = line.hasUnderStriporeVariant ? 1 : 0;
                //line.keywords 里不包含 __  有_ 关键字+1
                int kLineCount = kLineEnabledCount;
                //如果是 缩写的关键字
                if (ShaderHelper.IsBuiltinShortcut(line.shaderLineType))
                {
                    StripKeyword[] stripKeyword = null;
                    if (pass.hasSkipKeyword)
                    {
                        stripKeyword = pass.skipKeywords.ToArray();
                    }
                    kLineCount += ShaderHelper.GetShortcutVariantCount(line.shaderLineType, stripKeyword);
                }
                //multi_compile shader_fearure 要除去skip
                else if (ShaderHelper.IsInputShortcut(line.shaderLineType) && line.shaderLineType != ShaderLineType.SKIP_VARIANTS)
                {
                    if (ShaderHelper.IsSahderFeature(line.shaderLineType) && line.keywords.Count == 1)
                    {
                        kLineCount += 2;
                    }
                    else
                    {
                        kLineCount += line.keywords.Count;
                    }
                }

                for (int k = 0; k < line.keywords.Count; k++)
                {
                    StripKeyword keyword = line.keywords[k];
                    if (keyword.enabled)
                    {
                        //如果是 shader feature检查是否有材质使用
                        if (ShaderHelper.IsBuiltinShortcut(line.shaderLineType) || ShaderHelper.IsSahderFeature(line.shaderLineType))
                        {
                            for (int m = 0; m < matCount; m++)
                            {
                                //遍历关联的材质
                                if (materials[m].ContainsKeyword(keyword.name))
                                {
                                    kLineEnabledCount++;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            kLineEnabledCount++;
                        }
                    }
                }

                //实际使用的关键字
                if (kLineEnabledCount > 0)
                {
                    //实际使用的参与编译的 指令行的指令乘积 
                    passBuildCount *= kLineEnabledCount;
                }
                //全部编译指令行的指令乘积 包括没有使用的fearure
                pasStripCount *= kLineCount;
            }
            totalVariantCount += pasStripCount;
            actualBuildVariantCount += passBuildCount;
        }
        //计算enabed关键字数量
        keywordEnabledCount = 0;
        int keywordCount = keywords.Count;
        for (int k = 0; k < keywordCount; k++)
        {
            if (keywords[k].enabled)
                keywordEnabledCount++;
        }
    }

}

//一个Shader Pass
public class StripShaderPass
{
    //pass index
    public int pass;
    public List<StripKeywordLine> keywordLines = new List<StripKeywordLine>();
    public List<StripKeywordLine> skipKeywordLines = new List<StripKeywordLine>();
    public List<StripKeyword> skipKeywords = new List<StripKeyword>();
    public int keywordCount;
    public bool hasSkipKeyword = false;

    public void Add(StripKeywordLine keywordLine)
    {
        if (keywordLine.shaderLineType == ShaderLineType.SKIP_VARIANTS)
        {
            skipKeywordLines.Add(keywordLine);
            foreach (var skipkey in keywordLine.keywords)
            {
                if (!skipKeywords.Contains(skipkey))
                {
                    skipKeywords.Add(skipkey);
                }
            }
            hasSkipKeyword = true;
        }
        else
        {
            keywordLines.Add(keywordLine);
        }
        UpdateKeywordCount();
    }

    public void Add(List<StripKeywordLine> newkeywordLines)
    {
        for (int i = 0; i < newkeywordLines.Count; i++)
        {
            StripKeywordLine keywordLine = newkeywordLines[i];
            if (keywordLine.shaderLineType == ShaderLineType.SKIP_VARIANTS)
            {
                skipKeywordLines.Add(keywordLine);
                foreach (var skipkey in keywordLine.keywords)
                {
                    if (!skipKeywords.Contains(skipkey))
                    {
                        skipKeywords.Add(skipkey);
                    }
                }
                hasSkipKeyword = true;
            }
            else
            {
                keywordLines.Add(keywordLine);
            }
        }
        UpdateKeywordCount();
    }

    void UpdateKeywordCount()
    {
        keywordCount = 0;
        keywordLines.ForEach((StripKeywordLine obj) => keywordCount += obj.keywordCount);
        skipKeywordLines.ForEach((StripKeywordLine obj) => keywordCount -= obj.keywordCount);
    }

    public void Clear()
    {
        keywordCount = 0;
        keywordLines.Clear();
        skipKeywordLines.Clear();
        skipKeywords.Clear();
    }
}

//multi compile 指令
public class StripKeywordLine
{
    public List<StripKeyword> keywords = new List<StripKeyword>();
    //有没有下划线
    public bool hasUnderStriporeVariant;
    //是都是 shader_feature 指令
    public bool isFeature;
    //缩写指令类型
    public ShaderLineType shaderLineType;
    //编辑类型
    public EditorLineType editorLineType;

    public StripKeyword GetKeyword(string name)
    {
        int kc = keywords.Count;
        for (int k = 0; k < kc; k++)
        {
            StripKeyword keyword = keywords[k];
            if (keyword.name.Equals(name))
            {
                return keyword;
            }
        }
        return null;
    }

    public void Add(StripKeyword keyword)
    {
        if (GetKeyword(keyword.name) != null)
            return;
        // 忽略下划线__
        bool goodKeyword = false;
        for (int k = 0; k < keyword.name.Length; k++)
        {
            if (keyword.name[k] != '_')
            {
                goodKeyword = true;
                break;
            }
        }
        if (goodKeyword)
        {
            keywords.Add(keyword);
        }
        else
        {
            keyword.isUnderscoreKeyword = true;
            hasUnderStriporeVariant = true;
        }
    }

    public void DisableKeywords()
    {
        keywords.ForEach((StripKeyword obj) => obj.enabled = false);
    }

    public void Clear()
    {
        keywords.Clear();
    }

    public int keywordCount
    {
        get
        {
            return keywords.Count;
        }
    }

    public int keywordsEnabledCount
    {
        get
        {
            int kCount = keywords.Count;
            int enabledCount = 0;
            for (int k = 0; k < kCount; k++)
            {
                if (keywords[k].enabled)
                    enabledCount++;
            }
            return enabledCount;
        }
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        for (int k = 0; k < keywords.Count; k++)
        {
            if (k > 0)
                sb.Append(" ");
            sb.Append(keywords[k].name);
        }
        return sb.ToString();
    }

}

public class StripKeyword
{
    public string name;
    public bool enabled;
    public bool isUnderscoreKeyword;
    public bool isSkip;

    public StripKeyword(string name)
    {
        this.name = name;
        enabled = true;
    }

    public override bool Equals(object obj)
    {
        if (obj is StripKeyword)
        {
            StripKeyword other = (StripKeyword)obj;
            return name.Equals(other.name);
        }
        return false;
    }

    public override int GetHashCode()
    {
        return name.GetHashCode();
    }

    public override string ToString()
    {
        return name;
    }

}