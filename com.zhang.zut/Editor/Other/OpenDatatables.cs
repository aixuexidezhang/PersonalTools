using System;
using System.IO;
using UnityEditor;
using UnityEngine;


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


		[MenuItem("Tools/SQLite/创建SQLite", false, 2)]
		public static void NewSQLite()
		{
			string name = "Data.db";
			string path = Application.streamingAssetsPath;
			if (string.IsNullOrEmpty(path)) return;
			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);
			path = Path.Combine(path, name);

			if (!File.Exists(path))
			{
				SQLiteHelper.NewDbFile(path);
			}
			else
			{
				if (EditorUtility.DisplayDialog("创建失败", "已经存在Data.db", "覆盖", "取消"))
					SQLiteHelper.NewDbFile(Path.Combine(Application.streamingAssetsPath, "Data.db"));
			}
		}

		[MenuItem("Tools/SQLite/打开SQLite", false, 1)]
		public static void OpenSQLite()
		{
			string name = "Data.db";
			string path = Application.streamingAssetsPath;
			path = Path.Combine(path, name);
			if (File.Exists(path))
			{
				Application.OpenURL(path);
			}
			else
			{
				Debug.LogError("没有找到Data.db");
			}
		}
	}
}
#endif