using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class GitRepoPublishEditorWindow : EditorWindow
{
    GitRepoConfigModel gitRepoConfigModel;
    UnityPackageConfigModel unityPackageConfigModel;
    [SerializeField] PublishSo publishSo;
    int toolbarIndex = 0;

    [MenuItem("Tools/版本管理/UMP")]
    public static void Open()
    {
        GitRepoPublishEditorWindow window = EditorWindow.GetWindow<GitRepoPublishEditorWindow>();
        window.titleContent.text = "Git发布工具";
        window.Show();
    }

    void OnEnable()
    {
        if (gitRepoConfigModel == null)
        {
            gitRepoConfigModel = ReadGitRepoConfigFile();
        }
        if (unityPackageConfigModel == null)
        {
            unityPackageConfigModel = ReadUnityPackageConfigFile();
        }
        if (publishSo == null)
        {
            publishSo = ScriptableObject.CreateInstance<PublishSo>();
        }
    }

    void OnGUI()
    {

        toolbarIndex = GUILayout.Toolbar(toolbarIndex, new string[] { "基本配置", "准备发布", "打包 UnityPackage" });

        if (toolbarIndex == 0)
        {
            GUIToolBarConfig();
        }
        else if (toolbarIndex == 1)
        {
            GUIToolBarPublish();
        }
        else if (toolbarIndex == 2)
        {
            GUIToolBarUnityPackage();
        }

    }

    void GUIToolBarConfig()
    {
        if (gitRepoConfigModel == null)
        {
            if (GUILayout.Button("创建 GitRepo 配置文件"))
            {
                CreateGitRepoConfigFile();
            }
            if (GUILayout.Button("重试 GitRepo 读取配置文件"))
            {
                gitRepoConfigModel = ReadGitRepoConfigFile();
            }
            return;
        }

        GUILayout.Label("CHANGELOG.md 所在路径");
        gitRepoConfigModel.changeLogFilePath = GUILayout.TextField(gitRepoConfigModel.changeLogFilePath);

        GUILayout.Space(10);
        GUILayout.Label("VERSION 所在路径");
        gitRepoConfigModel.versionFilePath = GUILayout.TextField(gitRepoConfigModel.versionFilePath);

        GUILayout.Space(10);
        GUILayout.Label("package.json 所在路径");
        gitRepoConfigModel.packageJsonFilePath = GUILayout.TextField(gitRepoConfigModel.packageJsonFilePath);

        GUILayout.Space(10);
        if (GUILayout.Button("保存"))
        {
            SaveGitRepoConfigFile(gitRepoConfigModel);
        }
    }

    void GUIToolBarPublish()
    {
        if (publishSo.currentVersion == null)
        {
            publishSo.currentVersion = ReadCurrentVersion(gitRepoConfigModel);
        }

        GUILayout.Space(5);
        GUILayout.BeginHorizontal();
        GUILayout.Label($"当前版本号: {publishSo.currentVersion}");
        if (GUILayout.Button("重新获取"))
        {
            publishSo.currentVersion = ReadCurrentVersion(gitRepoConfigModel);
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        GUILayout.Label("待发布版本号(例1.1.0, 前后不加符号)");
        publishSo.semanticVersion = GUILayout.TextField(publishSo.semanticVersion);

        GUILayout.Space(10);
        GUILayout.Label("ChangeLog Added");
        publishSo.changeLogAdded = GUILayout.TextArea(publishSo.changeLogAdded, GUILayout.MinHeight(50));

        GUILayout.Space(10);
        GUILayout.Label("ChangeLog Changed");
        publishSo.changeLogChanged = GUILayout.TextArea(publishSo.changeLogChanged, GUILayout.MinHeight(50));

        GUILayout.Space(10);
        GUILayout.Label("ChangeLog Removed");
        publishSo.changeLogRemoved = GUILayout.TextArea(publishSo.changeLogRemoved, GUILayout.MinHeight(50));

        GUILayout.Space(10);
        GUILayout.Label("ChangeLog Fixed");
        publishSo.changeLogFixed = GUILayout.TextArea(publishSo.changeLogFixed, GUILayout.MinHeight(50));

        GUILayout.Space(10);
        GUILayout.Label("ChangeLog Other");
        publishSo.changeLogOther = GUILayout.TextArea(publishSo.changeLogOther, GUILayout.MinHeight(50));

        if (GUILayout.Button("保存发布"))
        {
            string title = "发布确认";
            string content = $"以下文件将会被修改:\r\n"
                            + $"  {gitRepoConfigModel.changeLogFilePath}\r\n"
                            + $"  {gitRepoConfigModel.versionFilePath}\r\n"
                            + $"  {gitRepoConfigModel.packageJsonFilePath}\r\n"
                            + $"是否确认保存?";
            string ok = "确认";
            string cancel = "取消";
            if (EditorUtility.DisplayDialog(title, content, ok, cancel))
            {
                PlayerSettings.bundleVersion = publishSo.semanticVersion;
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
                SaveChange(gitRepoConfigModel, publishSo);
            }

            if (EditorUtility.DisplayDialog("保存成功", "配置保存成功是否提交", "提交", cancel))
            {
                TortoiseEditor.GitAssetsCommit();
            }
        }
    }

    void GUIToolBarUnityPackage()
    {
        if (unityPackageConfigModel == null)
        {
            if (GUILayout.Button("创建 UnityPackage 配置文件"))
            {
                unityPackageConfigModel = CreateUnityPackageConfigFile();
            }
            return;
        }

        GUILayout.Label($"当前版本号: {publishSo.currentVersion}");

        GUILayout.Space(10);
        GUILayout.Label("源目录");
        unityPackageConfigModel.packageSourceDir = GUILayout.TextField(unityPackageConfigModel.packageSourceDir);

        GUILayout.Space(10);
        GUILayout.Label("导出目录(注: 该地址基于Environment.CurrentDirectory)");
        unityPackageConfigModel.packageOutputDir = GUILayout.TextField(unityPackageConfigModel.packageOutputDir);

        GUILayout.Space(10);
        GUILayout.Label("文件名");
        unityPackageConfigModel.packageName = GUILayout.TextField(unityPackageConfigModel.packageName);

        GUILayout.Space(10);
        unityPackageConfigModel.isAutoVersion = GUILayout.Toggle(unityPackageConfigModel.isAutoVersion, "是否自动附加版本号");

        GUILayout.Space(10);
        if (GUILayout.Button("保存配置"))
        {
            SaveUnityPackageConfigFile(unityPackageConfigModel);
        }

        if (GUILayout.Button("打包"))
        {
            CreateDirIfNorExist(unityPackageConfigModel.packageOutputDir);
            string inputDir = Path.Combine("Assets", unityPackageConfigModel.packageSourceDir);
            UnityEngine.Object[] objs = AssetDatabase.LoadAllAssetsAtPath(inputDir);
            var list = new List<string>();
            for (int i = 0; i < objs.Length; i += 1)
            {
                var obj = objs[i];
                var path = AssetDatabase.GetAssetPath(obj);
                list.Add(path);
            }
            string outputFile = Path.Combine(unityPackageConfigModel.packageOutputDir, unityPackageConfigModel.packageName + ReadCurrentVersion(gitRepoConfigModel) + ".unitypackage");
            AssetDatabase.ExportPackage(list.ToArray(), outputFile, ExportPackageOptions.Recurse);
        }
    }
    public void CreateDirIfNorExist(string _dirPath)
    {
        if (!Directory.Exists(_dirPath))
        {
            Directory.CreateDirectory(_dirPath);
        }
    }

    void SaveFileText(string txt, string path)
    {
        using (StreamWriter sw = File.CreateText(path))
        {
            sw.Write(txt);
        }
    }
    string LoadTextFromFile(string path)
    {

        using (StreamReader sr = new StreamReader(path))
        {
            return sr.ReadToEnd();
        }

    }
    void DeleteAllFilesInDirUnsafe(string _dirPath)
    {

        string[] _files = Directory.GetFiles(_dirPath);
        for (int i = 0; i < _files.Length; i += 1)
        {
            string _path = _files[i];
            File.Delete(_path);
        }

    }
    // ==== 保存发布信息到文件 ====
    void SaveChange(GitRepoConfigModel config, PublishSo publishModel)
    {

        const string NOTICE = "请放置在Assets目录下, 并且路径字符串不需要添加Assets前缀";

        string dir = Application.dataPath;
        string filePath;
        // SAVE VERSION
        //      COVER VERSION
        filePath = Path.Combine(dir, config.versionFilePath.TrimStart('/'));
        if (!File.Exists(filePath))
        {
            EditorUtility.DisplayDialog("错误", $"文件不存在: {filePath}\r\n{NOTICE}", "确认");
            return;
        }
        SaveFileText(publishModel.semanticVersion, filePath);

        // class PackageInfo
        // SAVE PACKAGEJSON:
        //      VERSION
        filePath = Path.Combine(dir, config.packageJsonFilePath.TrimStart('/'));
        if (!File.Exists(filePath))
        {
            EditorUtility.DisplayDialog("错误", $"文件不存在: {filePath}\r\n{NOTICE}", "确认");
            return;
        }
        string jsonStr = LoadTextFromFile(filePath);
        PackageJsonObj json = JsonConvert.DeserializeObject<PackageJsonObj>(jsonStr);
        if (json == null)
        {
            json = new PackageJsonObj();
        }
        json.version = publishModel.semanticVersion;
        jsonStr = JsonConvert.SerializeObject(json, Formatting.Indented);
        SaveFileText(jsonStr, filePath);

        // SAVE CHANGELOG:
        //      VERSION
        //      ADDED/CHANGED/REMOVED/FIXED/OTHER
        filePath = Path.Combine(dir, config.changeLogFilePath.TrimStart('/'));
        if (!File.Exists(filePath))
        {
            EditorUtility.DisplayDialog("错误", $"文件不存在: {filePath}\r\n{NOTICE}", "确认");
            return;
        }
        ChangeLogModifier changeLog = new ChangeLogModifier();
        changeLog.Analyze(File.ReadAllLines(filePath));
        changeLog.AddInfo(publishModel.semanticVersion, ChangeLogElementTag.TAG_ADDED, publishModel.changeLogAdded);
        changeLog.AddInfo(publishModel.semanticVersion, ChangeLogElementTag.TAG_CHANGED, publishModel.changeLogChanged);
        changeLog.AddInfo(publishModel.semanticVersion, ChangeLogElementTag.TAG_REMOVED, publishModel.changeLogRemoved);
        changeLog.AddInfo(publishModel.semanticVersion, ChangeLogElementTag.TAG_FIXED, publishModel.changeLogFixed);
        changeLog.AddInfo(publishModel.semanticVersion, ChangeLogElementTag.TAG_OTHER, publishModel.changeLogOther);
        changeLog.EndEdit();
        SaveFileText(changeLog.ToString(), filePath);

        publishModel.currentVersion = ReadCurrentVersion(gitRepoConfigModel);

    }

    string ReadCurrentVersion(GitRepoConfigModel configModel)
    {
        string dir = Application.dataPath;
        var filePath = Path.Combine(dir, configModel.versionFilePath.TrimStart('/'));
        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"文件不存在: {filePath}");
            return "unknown";
        }
        string version = LoadTextFromFile(filePath);
        return version;
    }

    // ==== 配置文件相关 ====
    // -- GIT REPO
    void CreateGitRepoConfigFile()
    {
        GitRepoConfigModel model = new GitRepoConfigModel();
        CreateDirIfNorExist(Path.Combine(Application.dataPath, "Config"));
        CreateDirIfNorExist(Path.Combine(Application.dataPath, "Config", "ZUT"));
        SaveGitRepoConfigFile(model);
    }

    void SaveGitRepoConfigFile(GitRepoConfigModel model)
    {
        string jsonStr = JsonConvert.SerializeObject(model);
        SaveFileText(jsonStr, GetGitRepoConfigFilePath());
        gitRepoConfigModel = model;
        EditorUtility.DisplayDialog("保存结果", "成功", "确认");
        AssetDatabase.Refresh();
    }

    string GetGitRepoConfigFilePath()
    {
        return Path.Combine(Application.dataPath, "Config", "ZUT", "GitPublishConfig.txt");
    }

    GitRepoConfigModel ReadGitRepoConfigFile()
    {
        if (!File.Exists(GetGitRepoConfigFilePath()))
        {
            Debug.LogWarning("找不到配置文件: " + GetGitRepoConfigFilePath());
            return null;
        }
        string jsonStr = LoadTextFromFile(GetGitRepoConfigFilePath());
        GitRepoConfigModel model = JsonConvert.DeserializeObject<GitRepoConfigModel>(jsonStr);
        return model;
    }

    // -- UNITY PACKAGE
    string GetUnityPackageConfigFile()
    {
        return Path.Combine(Application.dataPath, "UnityPackageConfig.txt");
    }

    UnityPackageConfigModel CreateUnityPackageConfigFile()
    {
        UnityPackageConfigModel model = new UnityPackageConfigModel();
        SaveUnityPackageConfigFile(model);
        return model;
    }

    UnityPackageConfigModel ReadUnityPackageConfigFile()
    {
        if (!File.Exists(GetUnityPackageConfigFile()))
        {
            return null;
        }
        string jsonStr = LoadTextFromFile(GetUnityPackageConfigFile());
        UnityPackageConfigModel model = JsonConvert.DeserializeObject<UnityPackageConfigModel>(jsonStr);
        return model;
    }

    void SaveUnityPackageConfigFile(UnityPackageConfigModel model)
    {
        string jsonStr = JsonConvert.SerializeObject(model);
        SaveFileText(jsonStr, GetUnityPackageConfigFile());
        unityPackageConfigModel = model;
        EditorUtility.DisplayDialog("保存结果", "成功", "确认");
        AssetDatabase.Refresh();
    }

}

public class GitRepoConfigModel
{

    public string changeLogFilePath = "/CHANGELOG.md";
    public string versionFilePath = "/VERSION";
    public string packageJsonFilePath = "/package.json";

}

public class UnityPackageConfigModel
{
    public string packageSourceDir;
    public string packageOutputDir;
    public string packageName;
    public bool isAutoVersion;
}

public class PublishSo : ScriptableObject
{

    public string currentVersion;
    public string semanticVersion = "1.0.0";

    public string changeLogAdded = "";
    public string changeLogChanged = "";
    public string changeLogRemoved = "";
    public string changeLogFixed = "";
    public string changeLogOther = "";

}

public class PackageJsonObj
{

    // REQUIRED PROPERTIES
    public string name = "anonymous";
    public string version = "0.0.0";

    // RECOMMANDED PROPERTIES
    public string description = "anonymous";
    public string displayName = "anonymous";
    public string unity = "";

    // OPTIONAL PROPERTIES
    public AutohrObj author = new AutohrObj();
    public string changelogUrl = "";
    public Dictionary<string, string> dependencies = new Dictionary<string, string>();
    public string documentationUrl = "";
    public bool hideInEditor = false;
    public string[] keywords = new string[0];
    public string license = "";
    public string licensesUrl = "";
    public SampleObj[] samples = new SampleObj[0];
    // public string type = "";
    public string unityRelease = "";

    public class AutohrObj
    {
        public string name = "anonymous";
        public string email = "anonymous@anonymous.com";
        public string url = "http://anonymous.com";
    }

    public class SampleObj
    {
        public string displayName = "anonymous";
        public string description = "anonymous";
        public string path = "/";
    }

}

public class ChangeLogModifier
{

    const string VERSION_LINE_STRATS_WITH = "## ";
    const string VERSION_ELEMENT_STRATS_WITH = "### ";

    List<string> titleLineList;
    List<VersionContainer> versionList;
    VersionContainer editingVersion;
    bool isUseOldVersion = false;

    public ChangeLogModifier()
    {
        this.titleLineList = new List<string>();
        this.versionList = new List<VersionContainer>();
    }

    public void Analyze(string[] changeLogTxtLines)
    {

        VersionContainer curContainer = null;
        VersionElement curElement = null;

        for (int i = 0; i < changeLogTxtLines.Length; i += 1)
        {
            string line = changeLogTxtLines[i];
            if (line.StartsWith(VERSION_LINE_STRATS_WITH))
            {
                string semanticVersion = GetMatchesLettersBetweenTwoChar(line, "[", "]")[0].Value;
                if (curContainer == null || curContainer.semanticVersion != semanticVersion)
                {
                    curContainer = GetOrAddVersion(semanticVersion, line);
                }
            }
            else if (line.StartsWith(VERSION_ELEMENT_STRATS_WITH))
            {
                if (curContainer == null)
                {
                    continue;
                }
                string tag = line.Split(VERSION_ELEMENT_STRATS_WITH.ToCharArray())[1];
                curElement = curContainer.GetOrAddElementTag(tag);
            }
            else
            {
                if (curElement == null)
                {
                    titleLineList.Add(line);
                    continue;
                }
                curContainer.AddElement(curElement.tag, line);
            }
        }

    }

    public void AddInfo(string semanticVersion, string tag, string content)
    {

        if (editingVersion == null)
        {
            editingVersion = versionList.Find(value => value.semanticVersion == semanticVersion);
            if (editingVersion != null)
            {
                isUseOldVersion = true;
            }
            else
            {
                isUseOldVersion = false;
                editingVersion = new VersionContainer(semanticVersion, ToFullVersionFormat(semanticVersion), true);
            }
        }

        VersionElement curEle = null;
        curEle = editingVersion.GetOrAddElementTag(tag);
        string[] lines = content.Split('\n');
        for (int i = 0; i < lines.Length; i += 1)
        {
            string line = lines[i];
            if (curEle != null)
            {
                editingVersion.AddElement(curEle.tag, line);
            }
        }
    }

    public void EndEdit()
    {
        if (isUseOldVersion)
        {
            return;
        }
        int lastVersionIndex = FindLastVersionIndex();
        if (lastVersionIndex == -1)
        {
            versionList.Add(editingVersion);
        }
        else
        {
            versionList.Insert(lastVersionIndex, editingVersion);
        }
    }

    VersionContainer GetOrAddVersion(string semanticVersion, string fullVersionLine)
    {
        int index = versionList.FindIndex(value => value.semanticVersion == semanticVersion);
        if (index == -1)
        {
            var versionContainer = new VersionContainer(semanticVersion, fullVersionLine, false);
            versionList.Add(versionContainer);
            return versionContainer;
        }
        else
        {
            return versionList[index];
        }
    }

    int FindLastVersionIndex()
    {
        int index = versionList.FindIndex(value => value.semanticVersion.Contains("."));
        return index;
    }

    string ToFullVersionFormat(string semanticVersion)
    {
        return $"## [{semanticVersion}] - " + ToDateFormat();
    }

    string ToDateFormat()
    {
        return ToYYYYMMDD(DateTime.Now);
    }

    string ToYYYYMMDD(DateTime t, char splitChar = '-')
    {
        return t.Year.ToString() + splitChar + t.Month.ToString().PadLeft(2, '0') + splitChar + t.Day.ToString().PadLeft(2, '0');
    }

    MatchCollection GetMatchesLettersBetweenTwoChar(string str, string startChar, string endChar)
    {
        Regex reg = new Regex(@"[" + startChar + "]+[a-zA-Z0-9.]+[" + endChar + "]");
        return reg.Matches(str);
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        titleLineList.ForEach(value => {
            sb.AppendLine(value);
        });
        versionList.ForEach(value => {
            sb.AppendLine(value.fullVersion);
            value.elementList.ForEach(ele => {
                if (!ele.HasContent())
                {
                    return;
                }
                string tag = $"### {ele.tag}";
                sb.AppendLine(tag);
                ele.ForEach(content => {
                    if (value.isNewVersion)
                    {
                        sb.AppendLine("- " + content + "  ");
                    }
                    else
                    {
                        sb.AppendLine(content);
                    }
                });
            });
            if (value.isNewVersion)
            {
                sb.AppendLine();
            }
        });
        return sb.ToString();
    }

    class VersionContainer
    {

        public string semanticVersion;
        public string fullVersion;
        public List<VersionElement> elementList;
        public bool isNewVersion;

        public VersionContainer(string semanticVersion, string srcLine, bool isNewVersion)
        {
            this.semanticVersion = semanticVersion;
            this.fullVersion = srcLine;
            this.isNewVersion = isNewVersion;
            this.elementList = new List<VersionElement>();
        }

        public VersionElement GetOrAddElementTag(string tag)
        {
            VersionElement ele = elementList.Find(value => value.tag == tag);
            if (ele == null)
            {
                ele = new VersionElement();
                ele.tag = tag;
                elementList.Add(ele);
            }
            return ele;
        }

        public VersionElement AddElement(string tag, string content)
        {
            VersionElement ele = GetOrAddElementTag(tag);
            ele.AddContent(content);
            return ele;
        }

    }

    class VersionElement
    {

        public string tag;
        List<string> contentList;

        public VersionElement()
        {
            this.contentList = new List<string>();
        }

        public void AddContent(string content)
        {
            this.contentList.Add(content);
        }

        public bool HasContent()
        {
            return contentList.Count > 0 && contentList[0].TrimEnd() != "";
        }

        public void ForEach(Action<string> action)
        {
            contentList.ForEach(action);
        }

    }


}

public static class ChangeLogElementTag
{
    public const string TAG_ADDED = "Added";
    public const string TAG_CHANGED = "Changed";
    public const string TAG_REMOVED = "Removed";
    public const string TAG_FIXED = "Fixed";
    public const string TAG_OTHER = "Other";
}