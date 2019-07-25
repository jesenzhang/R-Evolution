using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class FileHelper
{
    static List<string> ResultList = new List<string>();

    //找出所有文件夹全目录下的资源路径  AssetDatabase.FindAssets
    public static string[] FindAssets(string filter, string[] searchInFolders = null)
    {
        ResultList.Clear();
        string[] guids = null;
        if (searchInFolders != null)
        {
            foreach (var p in searchInFolders)
            {
                if (System.IO.Directory.Exists(p))
                {
                     guids = AssetDatabase.FindAssets(filter, searchInFolders);
                }
                else
                {
                    Debug.LogFormat("Find Asset Directory {0} is not exist! ", p);
                }
            }
        }
        else
        {
             guids = AssetDatabase.FindAssets(filter);
        }
        if (guids != null)
        {
            string[] paths = new string[guids.Length];
            for (int i = 0; i < guids.Length; ++i)
            {
                string temppath = AssetDatabase.GUIDToAssetPath(guids[i]);
                temppath = temppath.Replace("\\", "/");
                if (File.Exists(temppath) && !ResultList.Contains(temppath))
                {
                    ResultList.Add(temppath);
                }
            }
        }
        return ResultList.ToArray();
    }

  
    /// <summary>
    /// 找出目录下的所有子目录
    /// </summary>
    /// <param name="rootPath">查找目录</param>
    /// <param name="stripDirKeys">剔除的路径关键字</param>
    /// <param name="sp">索目录范围</param>
    /// <returns></returns>
    public static string[] GetDirectorys(string rootPath, string[] stripDirKeys, System.IO.SearchOption sp = System.IO.SearchOption.TopDirectoryOnly)
    {
        ResultList.Clear();
        string[] dirs = System.IO.Directory.GetDirectories(rootPath, "*", sp);
        if (dirs.Length > 0)
        {
            for (int i = 0; i < dirs.Length; i++)
            {
                string childdir = dirs[i];
                childdir = childdir.Replace("\\", "/");
                bool strip = false;
                foreach (var ext in stripDirKeys)
                {
                   string dir = childdir + "/";
                    if (dir.Contains(ext))
                    {
                        strip = true;
                        break;
                    }
                }
                if (strip)
                    continue;
                ResultList.Add(childdir);
            }
        }
        return ResultList.ToArray();

    }

    /// <summary>
    /// 找出所有文件夹全目录下的文件
    /// </summary>
    /// <param name="path">路径</param>
    /// <param name="fileExtlist">要找的文件扩展名类型数组</param>
    /// <param name="searchOption">搜索目录范围</param>
    /// <returns></returns>
    public static string[] GetFiles(string path, string[] fileExtlist, bool strip = false, SearchOption searchOption = SearchOption.TopDirectoryOnly)
    {
       string[] files = System.IO.Directory.GetFiles(path, "*", searchOption);
        ResultList.Clear();
        foreach (var file in files)
        {
            string afile = file.Replace("\\", "/");
            bool next = false;
            foreach (var ext in fileExtlist)
            {
                if (afile.EndsWith(ext) || ext == "*")
                {
                    if (strip)
                    {
                        next = true;
                        break;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            if (next)
                continue;
            ResultList.Add(afile);
        }
        return ResultList.ToArray();
    }

    /// <summary>
    /// 获取文件名
    /// </summary>
    /// <param name="path">文件路径</param>
    /// <param name="suffix">是否带扩展名</param>
    /// <returns></returns>
    public static string GetFileName(string path,bool suffix = true)
    {
        int index = path.LastIndexOf("/");
        if (index >= 0)
        {
            if (suffix)
            {
                return path.Substring(index + 1, path.Length - index - 1);
            }
            else
            {
                int endIndex = path.LastIndexOf('.');
                int length = endIndex >= 0 ? endIndex : path.Length;
                return path.Substring(index + 1, length - index - 1);
            }
        }
        return path.Substring(index + 1, path.Length - index - 1);
    }

    public static string SystemPathToAssetPath(string systemPath)
    {
        return "Assets" + systemPath.Substring(Application.dataPath.Length, systemPath.Length - Application.dataPath.Length);

    }

    //找出所有文件夹全目录下的资源路径 Directory.GetFiles
    public string[] GetFiles(string path, System.IO.SearchOption searchOption = System.IO.SearchOption.TopDirectoryOnly)
    {
        List<string> dirs = new List<string>();
        string[] files = System.IO.Directory.GetFiles(path, "*", searchOption);
        foreach (string file in files)
        {
            if (file.EndsWith(".meta"))
                continue;
            if (file.Contains(".svn"))
                continue;
            if (file.Contains("/Editor/"))
                continue;
            if (file.Contains("/Editor"))
                continue;
            dirs.Add(file.Replace("\\", "/"));
        }
        return dirs.ToArray();
    }

    
      /// <summary>
      /// 拷贝目录
      /// </summary>
      /// <param name="src">源目录</param>
      /// <param name="dst">目标目录</param>
      /// <param name="fileExtlist">拷贝的文件类型列表</param>
      /// <param name="stripDirKeys">忽略的路径关键字</param>
    public static void CopyFiles(string src, string dst, string[] fileExtlist,string[] stripDirKeys)
    {
        try
        {
            if (Directory.Exists(src))
            {
                if (!Directory.Exists(dst)) Directory.CreateDirectory(dst);
           
                var files = GetFiles(src, fileExtlist, SearchOption.TopDirectoryOnly);
       
                //当前目录
                foreach (var file in files)
                {
                    File.Copy(file, Path.Combine(dst,GetFileName(file)), true);
                }
                //子目录
                var childDirs = GetDirectorys(src,stripDirKeys);
                foreach (var dir in childDirs)
                {
                    CopyFiles(dir, Path.Combine(dst, GetFileName(dir)), fileExtlist, stripDirKeys);
                }
            }
            else if (File.Exists(src))
            {
                if (!Directory.Exists(dst)) Directory.CreateDirectory(dst);
                //拷贝文件
                File.Copy(src, Path.Combine(dst,GetFileName(src)), true);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("copy res error : " + e.Message);
        }
    }

    public static string[] SearchFilesFilter(string src, string[] pattern)
    {
        List<string> list = new List<string>();
        try
        {
            if (Directory.Exists(src))
            {
                foreach (var pa in pattern)
                {
                    //当前目录
                    foreach (var path in Directory.GetFiles(src, pa, SearchOption.TopDirectoryOnly).Where(q => !q.EndsWith(".meta")))
                    {
                        list.Add(path);
                    }
                }

                //子目录
                foreach (var dir in Directory.GetDirectories(src, "*", SearchOption.TopDirectoryOnly).Where(q => !q.EndsWith("Editor") && !q.Contains("\\..")))
                {
                    string[] childlist = SearchFilesFilter(dir, pattern);
                    list.AddRange(childlist);
                }
            }
            else
            {
                return null;
            }
            return list.ToArray();
        }
        catch (Exception e)
        {

            Debug.LogError("SearchFilesFilter error : " + e.Message);
            return null;
        }
    }

    public static void Rename(string oldpath,string newpath)
    {
        // 改名方法
        FileInfo fi = new FileInfo(oldpath); //xx/xx/aa.rar
        fi.MoveTo(newpath); //xx/xx/xx.rar
    }
}
