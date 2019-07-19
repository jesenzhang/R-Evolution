using System.Diagnostics;
using System;
using System.IO;

public class CmdHelper
{
    private static string CmdPath = @"C:\Windows\System32\cmd.exe";

    private static string ProcCmd(bool showWindows, params string[] cmd)
    {
        Process p = new Process();
        p.StartInfo.FileName = "cmd.exe";
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardInput = true;
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.RedirectStandardError = true;
        p.StartInfo.CreateNoWindow = !showWindows;
        p.Start();
        p.StandardInput.AutoFlush = true;
        for (int i = 0; i < cmd.Length; i++)
        {
            p.StandardInput.WriteLine(cmd[i].ToString());
        }
        p.StandardInput.WriteLine("exit");
        string strRst = p.StandardOutput.ReadToEnd();
        p.WaitForExit();
        p.Close();
        UnityEngine.Debug.Log(strRst);
        return strRst;
    }

    public static void ProcessTool(string toolPath, string arguments)
    {
        using (Process p = new Process())
        {
            p.StartInfo.FileName = toolPath;
            p.StartInfo.UseShellExecute = false;        //是否使用操作系统shell启动
            p.StartInfo.RedirectStandardInput = true;   //接受来自调用程序的输入信息
            p.StartInfo.RedirectStandardOutput = true;  //由调用程序获取输出信息
            p.StartInfo.RedirectStandardError = true;   //重定向标准错误输出
            p.StartInfo.CreateNoWindow = true;          //不显示程序窗口 
                                                        //p.StartInfo.WorkingDirectory = workingDirectory;
            p.StartInfo.Arguments = arguments;
            p.Start();
            //p.StandardInput.WriteLine(commandParams);
            p.StandardInput.AutoFlush = true;
            //获取输出信息
            //p.StandardOutput.ReadToEnd();
            p.WaitForExit();//等待程序执行完退出进程
            p.Close();
        }
    }

    public static void RunCmdDoFlatC(string flatcPath, string schemaPath, string jsonPath, string classPath, string binPath)
    {
        string schemaParams = string.Format("{0} -n --gen-onefile -o \"{1}\" \"{2}\"", flatcPath, Path.GetFullPath(classPath), Path.GetFullPath(schemaPath));
        string binParams = string.Format("{0} -b --allow-non-utf8 -o \"{1}\" \"{2}\" \"{3}\"", flatcPath, Path.GetFullPath(binPath), Path.GetFullPath(schemaPath), Path.GetFullPath(jsonPath));
        ProcCmd(false, new string[] { schemaParams, binParams });
    }

    public static void RunFlatC(string flatcPath, string schemaPath, string jsonPath, string classPath, string binPath)
    {
        string schemaParams = string.Format("-n --gen-onefile -o \"{0}\" \"{1}\"", Path.GetFullPath(classPath), Path.GetFullPath(schemaPath));
        string binParams = string.Format("-b --allow-non-utf8 -o \"{0}\" \"{1}\" \"{2}\"", Path.GetFullPath(binPath), Path.GetFullPath(schemaPath), Path.GetFullPath(jsonPath));
        Process.Start(flatcPath, schemaParams).WaitForExit();
        Process.Start(flatcPath, binParams).WaitForExit();
    }

}
