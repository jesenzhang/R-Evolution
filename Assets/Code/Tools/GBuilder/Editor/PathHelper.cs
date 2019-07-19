using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
/*
public class PathHelper
{
    private static string m_AbPath = Application.dataPath + "/../Build/BundleData";
    private static string m_AbVerPath = m_AbPath + "/version.txt";
    private static string m_AbComparePath = m_AbPath + "/CompareResult";
    private static string m_CodelistPath = m_AbPath + "/_codelist.txt";
    private static string m_AbStreamAssetPath = Application.dataPath + "/StreamingAssets/bundles";

    private static string GetAppName(BuildTarget buildTarget, string appVerion, string resVerion, string svnVersion)
    {
        string sdate = DateTime.Now.ToString("yyyy_MM_dd_HH_mm");
        string app = ".apk";
        if (buildTarget == BuildTarget.iOS)
        {
            app = ".ipa";
        }
        else if (buildTarget == BuildTarget.Android)
        {
            app = ".apk";
        }
        else
        {
            app = ".exe";
        }
        return string.Format("ldj_{0}_{1}_{2}_{3}{4}", sdate, svnVersion, appVerion, resVerion, app);
    }
    private static string GetAppUpdateUrl(string serverRoot, BuildTarget buildTarget, string name)
    {
        string targetDir = string.Empty;
        if (buildTarget == BuildTarget.iOS)
        {
            targetDir = "ios";
        }
        else if (buildTarget == BuildTarget.Android)
        {
            targetDir = "android";
        }
        else
        {
            targetDir = "windows";
        }
        return string.Format("{0}/{1}/{2}", serverRoot, targetDir, name);
    }
    private static string PlatformPath(BuildTarget buildTarget,string headPath)
    {
        string targetDir;
        if (buildTarget == BuildTarget.iOS)
        {
            targetDir = "iOS";
        }
        else if (buildTarget == BuildTarget.Android)
        {
            targetDir = "Android";
        }
        else
        {
            targetDir = "Windows";
        }
        string path = string.Format("{0}/{1}", headPath, targetDir);
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        return path;
    }
    private static string GetConfigurePath()
    {
        return string.Format("{0}/{1}", m_AbPath, "buildconfigure.json");
    }
    private static string GetVersionPath(BuildTarget buildTarget)
    {
        return string.Format("{0}/{1}", PlatformPath(buildTarget, GBuilderConfigure.Configure.bundlePath), "version.txt");
    }
    private static string GetSvnVersionPath(BuildTarget buildTarget)
    {
        return string.Format("{0}/{1}", PlatformPath(buildTarget), "svnversion.txt");
    }
    private static string GetCodeListPath(BuildTarget buildTarget)
    {
        return string.Format("{0}/{1}", PlatformPath(buildTarget), "_codelist.txt");
    }
    private static string GetBundlePath(BuildTarget buildTarget, string verion)
    {
        return string.Format("{0}/{1}", PlatformPath(buildTarget), verion);
    }
    private static string GetBundleVersionPath(BuildTarget buildTarget, string verion)
    {
        return string.Format("{0}/{1}/version.txt", PlatformPath(buildTarget), verion);
    }
    private static string GetComparePath(BuildTarget buildTarget)
    {
        return string.Format("{0}/{1}", PlatformPath(buildTarget), "CompareResult");
    }
    private static void ClearAssetBundles(bool clearCurVersion = false)
    {
        if (Directory.Exists(m_AbStreamAssetPath)) Directory.Delete(m_AbStreamAssetPath, true);
        Directory.CreateDirectory(m_AbStreamAssetPath);
        string curVersionABPath = m_AbPath + "/" + GBuilderConfigure.Configure.resVersion.ToString();
        if (clearCurVersion && Directory.Exists(curVersionABPath)) Directory.Delete(curVersionABPath, true);
    }
    public static void CopyResAppToSharePath()
    {
        string targetDir = string.Empty;
        BuildTarget buildTarget = GBuilderConfigure.Configure.buildTarget;
        if (buildTarget == BuildTarget.iOS)
        {
            targetDir = "iOS";
        }
        else if (buildTarget == BuildTarget.Android)
        {
            targetDir = "Android";
        }
        else
        {
            targetDir = "Windows";
        }
        string version = GBuilderConfigure.Configure.resVersion.ToString();
        string packageName = GetAppName(GBuilderConfigure.Configure.buildTarget, GBuilderConfigure.Configure.codeVersion.ToString(), GBuilderConfigure.Configure.resVersion.ToString(), GBuilderConfigure.Configure.svnVersion.ToString());
        string resSrc = string.Format("{0}/{1}/{2}", GBuilderConfigure.Configure.bundlePath, targetDir, version);
        string resDst = string.Format("{0}/{1}/{2}", GBuilderConfigure.Configure.resExportPath, targetDir, version);
        string packageSrc = string.Format("{0}/Build/{1}", GBuilderConfigure.Configure.projectPath, packageName);
        string packageDst = string.Format("{0}/{1}/{2}", GBuilderConfigure.Configure.packageExportPath, targetDir, packageName);

        CopyFiles(resSrc, resDst, null);

        if (File.Exists(packageSrc))
        {
            File.Copy(packageSrc, packageDst, true);
        }
    }
    private static void CopyAssetBundles(BuildTarget buildTarget)
    {
        DeleteMainfest(buildTarget);
        string srcBundlePath = GetBundlePath(buildTarget, m_CurrAbVersion);
        string dstBundlePath = m_AbStreamAssetPath;
        CopyFiles(srcBundlePath, dstBundlePath, ".signature");
    }
    private static void DeleteMainfest(BuildTarget buildTarget)
    {
        string srcBundlePath = GetBundlePath(buildTarget, m_CurrAbVersion);
        string[] files = Directory.GetFiles(srcBundlePath, "*.manifest", SearchOption.AllDirectories);
        //当前目录
        foreach (var file in files)
        {
            string path = file.Replace("\\", "/");
            File.Delete(path);
        }
    }
    private static void CopyFiles(string src, string dst, string filter)
    {
        try
        {
            if (Directory.Exists(src))
            {
                if (!Directory.Exists(dst)) Directory.CreateDirectory(dst);
                List<string> files = new List<string>();
                if (filter != null)
                {
                    files = Directory.GetFiles(src, "*", SearchOption.TopDirectoryOnly).Where(s => !s.EndsWith(filter)).ToList();
                }
                else
                {
                    files = Directory.GetFiles(src, "*", SearchOption.TopDirectoryOnly).ToList();
                }
                //当前目录
                foreach (var file in files)
                {
                    File.Copy(file, dst + "/" + Path.GetFileName(file), true);
                }
                //子目录
                foreach (var dir in Directory.GetDirectories(src, "*", SearchOption.TopDirectoryOnly))
                {
                    CopyFiles(dir, dst + "/" + Path.GetFileName(dir), filter);
                }
            }
            else
            {
                //拷贝文件
                File.Copy(src, dst + "/" + Path.GetFileName(src), true);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("copy res error : " + e.Message);
        }
    }
    private static string[] SearchFilesFilter(string src, string[] pattern)
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
}
*/