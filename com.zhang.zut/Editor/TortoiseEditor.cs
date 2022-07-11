using UnityEditor;
using UnityEngine;

public class TortoiseEditor
{
    public static string tortoiseGitPath = @"C:\Program Files\TortoiseGit\bin\TortoiseGitProc.exe";

    [MenuItem("Tools/版本管理/Git/StashSave")]
    public static void GitAssetsStushSave()
    {
        TortoiseGit.GitCommand(GitType.StashSave, Application.dataPath + "/com.zhang.zut/", tortoiseGitPath);
    }

    [MenuItem("Tools/版本管理/Git/StashPop")]
    public static void GitAssetsStushPop()
    {
        TortoiseGit.GitCommand(GitType.StashPop, Application.dataPath + "/com.zhang.zut/", tortoiseGitPath);
    }

    [MenuItem("Tools/版本管理/Git/Push")]
    public static void GitAssetPush()
    {
        TortoiseGit.GitCommand(GitType.Push, Application.dataPath + "/com.zhang.zut", tortoiseGitPath);
    }

    [MenuItem("Tools/版本管理/Git/Log")]
    public static void GitAssetsLog()
    {
        string[] strs = Selection.assetGUIDs;
        if (strs.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(strs[0]);
            TortoiseGit.GitCommand(GitType.Log, path, tortoiseGitPath);
        }
        else
        {
            TortoiseGit.GitCommand(GitType.Log, Application.dataPath + "/com.zhang.zut", tortoiseGitPath);
        }
    }

    [MenuItem("Tools/版本管理/Git/Pull")]
    public static void GitAssetsPull()
    {
        TortoiseGit.GitCommand(GitType.Pull, Application.dataPath + "/com.zhang.zut", tortoiseGitPath);
    }

    [MenuItem("Tools/版本管理/Git/Commit")]
    public static void GitAssetsCommit()
    {
        TortoiseGit.GitCommand(GitType.Commit, Application.dataPath + "/com.zhang.zut", tortoiseGitPath);
    }

    //[MenuItem("TortoiseGit/ProjectSettings/Log")]
    //public static void GitProjectSettingsLog()
    //{
    //    TortoiseGit.GitCommand(GitType.Log, Application.dataPath + "/com.zhang.zut", tortoiseGitPath);
    //}

    //[MenuItem("TortoiseGit/ProjectSettings/Pull")]
    //public static void GitProjectSettingsPull()
    //{
    //    TortoiseGit.GitCommand(GitType.Pull, Application.dataPath + "/com.zhang.zut", tortoiseGitPath);
    //}

    //[MenuItem("TortoiseGit/ProjectSettings/Commit")]
    //public static void GitProjectSettingsCommit()
    //{
    //    TortoiseGit.GitCommand(GitType.Commit, Application.dataPath + "/com.zhang.zut", tortoiseGitPath);
    //}
}
