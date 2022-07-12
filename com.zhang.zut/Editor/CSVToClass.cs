using System;
using System.Text;
using MyTool;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.CodeDom;
using System.Reflection;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;

public class CSVToClass : Editor
{
    static string datapath = "Tables/";
    static string GetCSVPath = Path.Combine(Application.streamingAssetsPath, datapath);
    static string GetClassPath = string.Concat(Application.dataPath, "/Scripts/Datas/");

    private static int currIndex = 0;
    private static int fileCount = 0;

    [MenuItem("CSV/生成类", false, 1)]
    static void GetCsvCreateClassOrEnum()
    {
        ClearAllClass();
        //准备一个代码编译器单元
        CodeCompileUnit unit = new CodeCompileUnit();
        //设置命名空间（这个是指要生成的类的空间）
        CodeNamespace myNamespace = new CodeNamespace();

        //导入必要的命名空间引用
        myNamespace.Imports.Add(new CodeNamespaceImport("System"));


        DirectoryInfo folder = new DirectoryInfo(GetCSVPath);

        FileInfo[] infos = folder.GetFiles("*.csv");

        fileCount = infos.Length;
        foreach (FileInfo file in infos)
        {
            currIndex++;
            string filePath = file.FullName;
            Encoding _conding = GetType(filePath); //获取文件编码类型
            FileStream _fileStream = new FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);

            string fileName = Path.GetFileNameWithoutExtension(filePath); //返回带扩展名的文件名

            StreamReader _sreamReader = new StreamReader(_fileStream, _conding);


            EditorUtility.DisplayProgressBar("CreateClass", $"Curr Create Class :{fileName}",
                (currIndex / fileCount));

            if (fileName.Contains("enum") || fileName.Contains("Enum"))
            {
                //生成枚举
                myNamespace.Types.Add(CreateEnum(_sreamReader, fileName));
            }
            else
            {
                //生成类
                myNamespace.Types.Add(CreateClass(_sreamReader, fileName));
            }

            _fileStream.Close();
            _sreamReader.Close();
        }


        //把该命名空间加入到编译器单元的命名空间集合中
        unit.Namespaces.Add(myNamespace);

        //生成C#脚本("VisualBasic"：VB脚本)
        CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");
        CodeGeneratorOptions options = new CodeGeneratorOptions();

        //代码风格:大括号的样式{}
        options.BracingStyle = "C";

        //是否在字段、属性、方法之间添加空白行
        options.BlankLinesBetweenMembers = true;

        //输出文件路径
        string outputFile = string.Concat(GetClassPath, "database", ".cs");

        if (!Directory.Exists(GetClassPath))
        {
            Directory.CreateDirectory(GetClassPath);
            //保存
            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(outputFile))
            {
                //为指定的代码文档对象模型(CodeDOM) 编译单元生成代码并将其发送到指定的文本编写器，使用指定的选项。(官方解释)
                //将自定义代码编译器(代码内容)、和代码格式写入到sw中
                provider.GenerateCodeFromCompileUnit(unit, sw, options);
                EditorUtility.ClearProgressBar();
            }
        }
        else
        {
            //保存
            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(outputFile))
            {
                //为指定的代码文档对象模型(CodeDOM) 编译单元生成代码并将其发送到指定的文本编写器，使用指定的选项。(官方解释)
                //将自定义代码编译器(代码内容)、和代码格式写入到sw中
                provider.GenerateCodeFromCompileUnit(unit, sw, options);
                EditorUtility.ClearProgressBar();
            }
        }
        Debug.Log("加载完成");
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 删除之前生成的所有类    //TODO 这个地方是不是太耗费性能了
    /// </summary>
    static void ClearAllClass()
    {
        try
        {
            DirectoryInfo dir = new DirectoryInfo(GetClassPath);
            if (dir.Exists)
            {
                FileSystemInfo[] fileinfo = dir.GetFileSystemInfos(); //返回目录中所有文件和子目录
                foreach (FileSystemInfo i in fileinfo)
                {
                    if (i is DirectoryInfo) //判断是否文件夹
                    {
                        DirectoryInfo subdir = new DirectoryInfo(i.FullName);
                        subdir.Delete(true); //删除子目录和文件
                    }
                    else
                    {
                        File.Delete(i.FullName); //删除指定文件
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Error:" + ex);
        }
    }

    public static CodeTypeDeclaration CreateEnum(StreamReader streamReader, string fileName)
    {
        List<string> enumNames = new List<string>();

        //每行的数据
        string strLine = "";
        while ((strLine = streamReader.ReadLine()) != null)
        {
            var arrLien = strLine.Split(',');
            enumNames.Add(arrLien[0]);
        }

        return BuildEnumClass(fileName, enumNames);
    }

    /// <summary>
    /// 生成变量名称 变量类型
    /// </summary>
    /// <param name="streamReader"></param>
    /// <param name="fileName"></param>
    public static CodeTypeDeclaration CreateClass(StreamReader streamReader, string fileName)
    {
        Dictionary<string, string> _calssData = new Dictionary<string, string>();

        var _tableHead = streamReader.ReadLine().Split(','); //读取一行的记录数

        var columnCount = _tableHead.Length;
        for (int i = 0; i < columnCount; i++)
        {
            try
            {
                string[] _varInfo = _tableHead[i].Split(':');

                string _varType = _varInfo[1];
                string _varName = _varInfo[0];
                _calssData.Add(_varName, _varType);
            }
            catch (IndexOutOfRangeException ex)
            {
                //超出索引
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error:{ex}");
            }
        }

        return BuildClass(fileName, _calssData);
    }

    /// <summary>
    /// 生成枚举类
    /// </summary>
    /// <param name="className">枚举类名称</param>
    /// <param name="typeValue">枚举参数名称/</param>
    public static CodeTypeDeclaration BuildEnumClass(string className, List<string> typeValue)
    {
        //Code:代码体
        CodeTypeDeclaration myClass = new CodeTypeDeclaration(className);

        //指定为类
        myClass.IsClass = true;
        //设置类的访问类型
        myClass.TypeAttributes = TypeAttributes.Public; // | TypeAttributes.Sealed;
        myClass.CustomAttributes.Add(new CodeAttributeDeclaration("Serializable"));
        myClass.IsEnum = true;
        myClass.TypeAttributes = TypeAttributes.Public;

        //添加枚举值
        foreach (var item in typeValue)
        {
            CodeMemberField field = new CodeMemberField(typeof(System.Enum), item);
            field.Attributes = MemberAttributes.Public;
            myClass.Members.Add(field);
        }

        return myClass;
    }

    /// <summary>
    /// 生成类
    /// </summary>
    /// <param name="className">类名</param>
    /// <param name="classData">参数名称/参数类型</param>
    public static CodeTypeDeclaration BuildClass(string className, Dictionary<string, string> classData)
    {
        //Code:代码体
        CodeTypeDeclaration myClass = new CodeTypeDeclaration(className);
        //指定为类
        myClass.IsClass = true;
        //设置类的访问类型
        myClass.TypeAttributes = TypeAttributes.Public;
        myClass.CustomAttributes.Add(new CodeAttributeDeclaration("Serializable"));
        foreach (var item in classData)
        {
            myClass.Members.Add(BuildClassVar(item.Key, item.Value)); // 添加字段
        }

        return myClass;
    }

    /// <summary>
    /// 添加字段
    /// </summary>
    /// <param name="varName">类名</param>
    /// <param name="varType">类型</param>
    public static CodeMemberField BuildClassVar(string varName, string varType)
    {
        CodeMemberField field = new CodeMemberField(GetVarType(varType), varName);

        field.Attributes = MemberAttributes.Public;

        return field;
    }

    /// <summary>
    /// 获取CSV 数据类型
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static CodeTypeReference GetVarType(string type)
    {
        CodeTypeReference _codeTypeReference = null;
        switch (type)
        {
            case "string":
            case "String":
                _codeTypeReference = new CodeTypeReference(typeof(string));
                break;
            case "int":
            case "Int32":
                _codeTypeReference = new CodeTypeReference(typeof(int));
                break;
            case "long":
            case "Int64":
                _codeTypeReference = new CodeTypeReference(typeof(long));
                break;
            case "double":
            case "Double":
                _codeTypeReference = new CodeTypeReference(typeof(double));
                break;
            case "float":
            case "Single":
                _codeTypeReference = new CodeTypeReference(typeof(float));
                break;
            case "bool":
            case "Boolean":
                _codeTypeReference = new CodeTypeReference(typeof(bool));
                break;
            case "short":
            case "Int16":
                _codeTypeReference = new CodeTypeReference(typeof(short));
                break;
            case "byte":
            case "Byte":
                _codeTypeReference = new CodeTypeReference(typeof(byte));
                break;
            case "ushort":
            case "UInt16":
                _codeTypeReference = new CodeTypeReference(typeof(ushort));
                break;
            case "enum": //这里要做特殊处理
                _codeTypeReference = new CodeTypeReference(typeof(System.Enum));
                break;
            default:
                _codeTypeReference = new CodeTypeReference(type);
                break;
        }

        return _codeTypeReference;
    }


    public static Encoding GetType(string FILE_NAME)
    {
        FileStream fs = new FileStream(FILE_NAME, FileMode.Open,
            FileAccess.Read);
        Encoding r = GetType(fs);
        fs.Close();
        return r;
    }

    /// 通过给定的文件流，判断文件的编码类型
    /// <param name="fs">文件流</param>
    /// <returns>文件的编码类型</returns>
    public static Encoding GetType(FileStream fs)
    {
        byte[] Unicode = new byte[] { 0xFF, 0xFE, 0x41 };
        byte[] UnicodeBIG = new byte[] { 0xFE, 0xFF, 0x00 };
        byte[] UTF8 = new byte[] { 0xEF, 0xBB, 0xBF }; //带BOM
        Encoding reVal = Encoding.Default;

        BinaryReader r = new BinaryReader(fs, Encoding.Default);
        int i;
        int.TryParse(fs.Length.ToString(), out i);
        byte[] ss = r.ReadBytes(i);
        if (IsUTF8Bytes(ss) || (ss[0] == 0xEF && ss[1] == 0xBB && ss[2] == 0xBF))
        {
            reVal = Encoding.UTF8;
        }
        else if (ss[0] == 0xFE && ss[1] == 0xFF && ss[2] == 0x00)
        {
            reVal = Encoding.BigEndianUnicode;
        }
        else if (ss[0] == 0xFF && ss[1] == 0xFE && ss[2] == 0x41)
        {
            reVal = Encoding.Unicode;
        }

        r.Close();
        return reVal;
    }

    /// 判断是否是不带 BOM 的 UTF8 格式
    /// <param name="data"></param>
    /// <returns></returns>
    private static bool IsUTF8Bytes(byte[] data)
    {
        int charByteCounter = 1; //计算当前正分析的字符应还有的字节数
        byte curByte; //当前分析的字节.
        for (int i = 0; i < data.Length; i++)
        {
            curByte = data[i];
            if (charByteCounter == 1)
            {
                if (curByte >= 0x80)
                {
                    //判断当前
                    while (((curByte <<= 1) & 0x80) != 0)
                    {
                        charByteCounter++;
                    }

                    //标记位首位若为非0 则至少以2个1开始 如:110XXXXX...........1111110X　
                    if (charByteCounter == 1 || charByteCounter > 6)
                    {
                        return false;
                    }
                }
            }
            else
            {
                //若是UTF-8 此时第一位必须为1
                if ((curByte & 0xC0) != 0x80)
                {
                    return false;
                }

                charByteCounter--;
            }
        }

        if (charByteCounter > 1)
        {
            throw new Exception("非预期的byte格式");
        }

        return true;
    }
}