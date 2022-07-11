using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class ToolsModuleControls : EditorWindow
{

    [MenuItem("Tools/Modules")]
    public static void OpenWindow()
    {
        GetWindow<ToolsModuleControls>(false);
    }
    bool StringTools;
    string oldPath = @"C:\Users\23138\my-tool\MyTool\ZTools\Dll\MyTool.dll";
    string destPath = "ZUT/com.zhang.zut/Plugins/MyTool.dll";
    string oldPathX = @"C:\Users\23138\my-tool\MyTool\ZTools\Dll\MyTool.xml";
    string destPathX = "ZUT/com.zhang.zut/Plugins/MyTool.xml";
    private void OnGUI()
    {
        GUILayout.Label("ZTools模块控制(功能暂未实现)");
        GUILayout.Space(5);
        if (GUILayout.Button("重新加载MyTool.dll"))
        {
            ReLoadMyTool();
        }
        return;
        GUILayout.Space(5);
        StringTools = EditorGUILayout.Toggle("StringTools", StringTools);
        GUILayout.Space(5);
    }


    private void ReLoadMyTool()
    {
        try
        {
            FileInfo fi1 = new FileInfo(oldPath);
            fi1.CopyTo(Path.Combine(Application.dataPath, destPath), true);
            FileInfo fi2 = new FileInfo(oldPathX);
            fi2.CopyTo(Path.Combine(Application.dataPath, destPathX), true);
            Debug.LogError("完成");
            //AssetDatabase.Refresh();
        }
        catch (System.Exception)
        {

            throw;
        }

    }
}
