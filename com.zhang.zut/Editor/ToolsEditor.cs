using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ToolsEditor : Editor
{
    #region SVN相关

    [MenuItem("Tools/SVN/提交", false, 1)]
	static void SVNCommit()
	{
		List<string> pathList = new List<string>();
		string basePath = SVNProjectPath + "/Assets";
		pathList.Add(basePath);
		pathList.Add(SVNProjectPath + "/ProjectSettings");

		string commitPath = string.Join("*", pathList.ToArray());
		ProcessCommand("TortoiseProc.exe", "/command:commit /path:" + commitPath);
	}
	[MenuItem("Tools/SVN/更新", false, 2)]
	static void SVNUpdate()
	{
		ProcessCommand("TortoiseProc.exe", "/command:update /path:" + SVNProjectPath + " /closeonend:0");
	}
	[MenuItem("Tools/SVN/日志", false, 5)]
	static void SVNLog()
	{
		ProcessCommand("TortoiseProc.exe", "/command:log /path:" + SVNProjectPath);
	}
	static string SVNProjectPath
	{
		get
		{
			System.IO.DirectoryInfo parent = System.IO.Directory.GetParent(Application.dataPath);
			return parent.ToString();
		}
	}
	static void ProcessCommand(string command, string argument)
	{
		System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo(command);
		info.Arguments = argument;
		info.CreateNoWindow = false;
		info.ErrorDialog = true;
		info.UseShellExecute = true;

		if (info.UseShellExecute)
		{
			info.RedirectStandardOutput = false;
			info.RedirectStandardError = false;
			info.RedirectStandardInput = false;
		}
		else
		{
			info.RedirectStandardOutput = true;
			info.RedirectStandardError = true;
			info.RedirectStandardInput = true;
			info.StandardOutputEncoding = System.Text.UTF8Encoding.UTF8;
			info.StandardErrorEncoding = System.Text.UTF8Encoding.UTF8;
		}

		System.Diagnostics.Process process = System.Diagnostics.Process.Start(info);

		if (!info.UseShellExecute)
		{
			Debug.Log(process.StandardOutput);
			Debug.Log(process.StandardError);
		}

		process.WaitForExit();
		process.Close();
	}

	#endregion

	#region

	//[MenuItem("Tools/LocalText/打开Loc_Text", false, 1)]
	//static void OpenLocText()
	//{
 //       Application.OpenURL(Application.dataPath + "/Resources/loc_text.csv");
	//}

	#endregion
}
