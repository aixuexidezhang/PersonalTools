using MyTool.UnityComponents;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static MyTool.Tools.Data;

[CanEditMultipleObjects]
[CustomEditor(typeof(PrefabDatas))]
public class PrefabDatasEditor : Editor
{
    public override void OnInspectorGUI()
    {
        if (targets.Length == 1)
        {
            PrefabDatas db = target as PrefabDatas;
            if (db.Datas != null)
                GUILayout.Label("成功加载" + db.Datas.Count + "个预制体");
        }
        if (GUILayout.Button("刷新", GUILayout.Height(30f)))
        {
            for (int i = 0; i < targets.Length; i++)
            {
                var t = targets[i];
                PrefabDatas db = t as PrefabDatas;
                Refresh(db);
                EditorUtility.SetDirty(db);
            }
            AssetDatabase.SaveAssets();
        }
        base.OnInspectorGUI();
    }

    public static void Refresh(PrefabDatas db)
    {
        Debug.Log("刷新 SpriteDatabase: \"" + db.name + "\"");
        string path = AssetDatabase.GetAssetPath(db);
        db.Datas?.Clear();
        path = MyTool.Tools.FileTool.GetAbsolutePath(path);
        path = path.Replace(Path.GetFileName(path), "");
        List<FileInfo> files;
        if (db.TaggetPaths != null && db.TaggetPaths.Length > 0)
        {
            files = new List<FileInfo>();
            foreach (var p in db.TaggetPaths)
            {
                try
                {
                    var f = MyTool.Tools.FileTool.FindFileSearch(string.Concat(Application.dataPath.Remove(Application.dataPath.IndexOf("Assets")), p), "*.prefab");
                    files.AddRange(f);
                }
                catch (System.Exception)
                {
                    Debug.LogError(p + "  出错");
                    throw;
                }
            }
        }
        else
        {
            files = MyTool.Tools.FileTool.FindFileSearch(path, "*.prefab");
        }

        for (int i = 0; i < files.Count; i++)
        {
            var f = files[i];
            var assets = AssetDatabase.LoadAssetAtPath(MyTool.Tools.FileTool.GetRelativePath(f.FullName), typeof(GameObject));
            db.Datas.Add((GameObject)assets);
        }
        EditorUtility.ClearProgressBar();
        Debug.Log("Sprite database \"" + db.name + "\" refreshed:(" + db.Datas.Count + ")");

    }
}
