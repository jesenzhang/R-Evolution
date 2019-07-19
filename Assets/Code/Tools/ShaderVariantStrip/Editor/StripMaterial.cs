using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//材质包含使用的关键字 每个shader保存关联的材质
 public class StripMaterial
{
    public string name = "";
    public string path = "";
    public string GUID = "";
    public List<StripKeyword> keywords = new List<StripKeyword>();
    public bool pendingChanges;

    HashSet<string> keywordSet = new HashSet<string>();

    public StripMaterial(string name, string path, string GUID)
    {
        this.name = name;
        this.path = path;
        this.GUID = GUID;
    }

    public void SetKeywords(string[] names)
    {
        for (int k = 0; k < names.Length; k++)
        {
            if (!keywordSet.Contains(names[k]))
            {
                keywordSet.Add(names[k]);
                StripKeyword keyword = new StripKeyword(names[k]);
                keywords.Add(keyword);
            }
        }
        keywords.Sort(delegate (StripKeyword k1, StripKeyword k2) { return k1.name.CompareTo(k2.name); });
    }

    public bool ContainsKeyword(string name)
    {
        return keywordSet.Contains(name);
    }

    public void RemoveKeyword(string name)
    {
        for (int k = 0; k < keywords.Count; k++)
        {
            if (keywords[k].name.Equals(name))
            {
                keywords.RemoveAt(k);
                return;
            }
        }
    }
 }
 
