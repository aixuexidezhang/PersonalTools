using System;
using System.Text;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.CodeDom;
using System.Reflection;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;

#if UNITY_EDITOR

namespace MyTool.UnityComponents
{
	public class OpenDatatables : Editor
	{
		[MenuItem("CSV/打开Databases文件夹", false, 2)]
		public static void OpenDirectory()
		{
			string datapath = "Tables/";
			string path = Path.Combine(Application.streamingAssetsPath, datapath);
			if (string.IsNullOrEmpty(path)) return;
			path = path.Replace("/", "\\");
			if (!Directory.Exists(path))
			{
                Directory.CreateDirectory(path);
			}
			System.Diagnostics.Process.Start("explorer.exe", path);
		}
	}
}
#endif

