using MyTool.UnityComponents;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static MyTool.Tools.Data;

[CanEditMultipleObjects]
[CustomEditor(typeof(SpriteDatas))]
public class SpriteDatasEditor : Editor
{
    public override void OnInspectorGUI()
    {
        if (targets.Length == 1)
        {
            SpriteDatas db = target as SpriteDatas;
            if (db.Datas != null)
                GUILayout.Label("成功加载" + db.Datas.Count + "个Sprite");
        }
        if (GUILayout.Button("刷新", GUILayout.Height(30f)))
        {
            for (int i = 0; i < targets.Length; i++)
            {
                var t = targets[i];
                SpriteDatas db = t as SpriteDatas;
                Refresh(db);
                EditorUtility.SetDirty(db);
            }
            AssetDatabase.SaveAssets();
        }
        base.OnInspectorGUI();
    }


    public static void Refresh(SpriteDatas db)
    {
        Debug.Log("刷新 SpriteDatabase:  \"" + db.name + "\"");
        string path = AssetDatabase.GetAssetPath(db);
        db.Datas?.Clear();
        path = MyTool.Tools.FileTools.GetAbsolutePath(path);
        path = path.Replace(Path.GetFileName(path), "");
        List<FileInfo> files = new List<FileInfo>();
        if (db.TaggetPaths != null && db.TaggetPaths.Length > 0)
        {
            foreach (var p in db.TaggetPaths)
            {
                try
                {
                    var f = MyTool.Tools.FileTools.FindFileSearch(string.Concat(Application.dataPath.Remove(Application.dataPath.IndexOf("Assets")), p), "*");
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
            files = MyTool.Tools.FileTools.FindFileSearch(path, "*");
        }
        for (int i = 0; i < files.Count; i++)
        {
            var f = files[i];
            var assets = AssetDatabase.LoadAllAssetsAtPath(MyTool.Tools.FileTools.GetRelativePath(f.FullName));
            for (int j = 0; j < assets.Length; j++)
            {
                var a = assets[j];
                Sprite s = a as Sprite;
                if (s == null)
                    continue;
                db.Datas.Add(s);
                Debug.Log("添加 \"" + s.name + "\"");
                EditorUtility.DisplayProgressBar("refreshing...", s.name, (float)i / files.Count);
            }
            EditorUtility.ClearProgressBar();
        }

    }
}


