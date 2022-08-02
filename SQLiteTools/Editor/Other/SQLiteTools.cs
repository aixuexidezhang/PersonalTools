using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

public class SQLiteTools
{

    static string GetClassPath = string.Concat(Application.dataPath, "/Scripts/Datas/");

    static int currIndex = 0;

    static int fileCount = 0;

    [MenuItem("Tools/SQLite/转换所有表为类", false, 3)]

    static void SQLiteToClass()
    {
        Dictionary<string, List<Tuple<string, string, bool>>> allTables = new Dictionary<string, List<Tuple<string, string, bool>>>();
        using (SQLiteConnection conn = new SQLiteConnection(SQLiteHelper.Init.Connstring))
        {
            conn.Open();
            var dt = conn.GetSchema("TABLES");
            DataColumn d = dt.Columns["TABLE_NAME"];
            dt.Rows.RemoveAt(0);
            foreach (DataRow row in dt.Rows)
            {
                var tn = row[d].ToString();
                if (allTables.ContainsKey(tn))
                {
                    Debug.LogError(tn + "已经存在!!");
                    continue;
                }
                if (tn.Contains("old"))
                {
                    Debug.LogError($"{tn}是old表,不会被加载成class");
                    continue;
                }
                allTables.Add(tn, GetReaderSchema(tn, conn));
            }
        }
        GetCsvCreateClassOrEnum(allTables);
    }

    static List<Tuple<string, string, bool>> GetReaderSchema(string tableName, SQLiteConnection connection)
    {
        List<Tuple<string, string, bool>> tuples = new List<Tuple<string, string, bool>>();
        IDbCommand cmd = new SQLiteCommand();
        cmd.CommandText = string.Format("select * from [{0}]", tableName);
        cmd.Connection = connection;
        using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.KeyInfo | CommandBehavior.SchemaOnly))
        {
            var dt = reader.GetSchemaTable();
            DataColumn name = dt.Columns["ColumnName"];
            DataColumn type = dt.Columns["DataTypeName"];
            DataColumn key = dt.Columns["IsKey"];
            foreach (DataRow row in dt.Rows)
            {
                tuples.Add(new Tuple<string, string, bool>(row[name].ToString(), row[type].ToString(), row[key].ToString().Equals("True")));
            }
        }
        return tuples;
    }
    static void GetCsvCreateClassOrEnum(Dictionary<string, List<Tuple<string, string, bool>>> allTables)
    {
        ClearAllClass();
        //准备一个代码编译器单元
        CodeCompileUnit unit = new CodeCompileUnit();
        //设置命名空间（这个是指要生成的类的空间）
        CodeNamespace myNamespace = new CodeNamespace();

        //导入必要的命名空间引用
        myNamespace.Imports.Add(new CodeNamespaceImport("System"));


        CodeTypeDeclaration myClass = new CodeTypeDeclaration("SQLiteDatas");

        myClass.IsClass = true;

        myClass.TypeAttributes = TypeAttributes.Public;

        CodeComment com = new CodeComment("该类为数据读取类", true);

        CodeCommentStatement coms = new CodeCommentStatement(com);


        CodeConstructor cc = new CodeConstructor();

        cc.Attributes = MemberAttributes.Public;

        myClass.Members.Add(cc);

        myNamespace.Types.Add(myClass);

        myClass.Comments.Add(coms);

        fileCount = allTables.Count;

        bool ishavekey;

        foreach (var d in allTables)
        {
            string fileName = d.Key;

            currIndex++;

            ishavekey = false;

            foreach (var key in d.Value)
            {
                if (key.Item3)
                {
                    CodeMemberField field = new CodeMemberField($"System.Collections.Generic.Dictionary<{GetType(key.Item2)},{d.Key}> {fileName}_table = new ",
                        $"System.Collections.Generic.Dictionary<{GetType(key.Item2)},{d.Key}>()");
                    field.Attributes = MemberAttributes.Public;

                    myClass.Members.Add(field);

                    string fr = fileName + "_read";

                    string l = "{";

                    string r = "}";

                    string code = string.Concat
                        (
                         $"\r\n\t\tvar {fr} = SQLiteHelper.Init.ExcuteReader(\"SELECT * FROM '{d.Key}'\");\r\n",
                         $"\t\twhile ({fr}.Read())\r\n",
                         $"\t\t{l}\r\n ",
                         $"\t\t\tvar data = new {d.Key}({fr});\r\n ",
                         $"\t\t\t{fileName}_table.Add(data.{key.Item1}, data);\r\n" +
                         $"\t\t{r}"
                         );

                    var v = new CodeVariableDeclarationStatement(code, "");

                    cc.Statements.Add(v);

                    ishavekey = true;
                    continue;
                }

                if (!ishavekey)
                {
                    Debug.LogError($"{d.Key}没有主键,将会使用List储存数据,");

                    CodeMemberField field = new CodeMemberField($"System.Collections.Generic.List<{d.Key}> {fileName}_list = new ", $"System.Collections.Generic.List<{d.Key}>()");

                    field.Attributes = MemberAttributes.Public;

                    myClass.Members.Add(field);

                    string fr = fileName + "_read";

                    string l = "{";

                    string r = "}";

                    string code = string.Concat
                        (
                         $"\r\n\t\tvar {fr} = SQLiteHelper.Init.ExcuteReader(\"SELECT * FROM '{d.Key}'\");\r\n",
                         $"\t\twhile ({fr}.Read())\r\n",
                         $"\t\t{l}\r\n ",
                         $"\t\t\tvar data = new {d.Key}({fr});\r\n ",
                         $"\t\t\t{fileName}_list.Add(data);\r\n" +
                         $"\t\t{r}"
                         );

                    var v = new CodeVariableDeclarationStatement(code, "");

                    cc.Statements.Add(v);
                }
            }





            EditorUtility.DisplayProgressBar("CreateClass", $"Curr Create Class :{fileName}",
                (currIndex / fileCount));

            myNamespace.Types.Add(CreateClass(d.Value, fileName));
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
                var snippetMethod = new CodeSnippetTypeMember(sw.ToString());
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
    /// 删除之前生成的所有类
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
                    if (i.Name.Equals("database.cs"))
                    {
                        File.Delete(i.FullName);
                        break;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Error:" + ex);
        }
    }

    static CodeTypeDeclaration CreateEnum(StreamReader streamReader, string fileName)
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
    static CodeTypeDeclaration CreateClass(List<Tuple<string, string, bool>> t, string fileName)
    {
        Dictionary<string, string> _calssData = new Dictionary<string, string>();

        foreach (var item in t)
        {
            if (_calssData.ContainsKey(item.Item1))
            {
                Debug.LogError($"字段{item.Item1}已经存在");
                continue;
            }
            _calssData.Add(item.Item1, item.Item2);
        }

        return BuildClass(fileName, _calssData);
    }

    /// <summary>
    /// 生成枚举类
    /// </summary>
    /// <param name="className">枚举类名称</param>
    /// <param name="typeValue">枚举参数名称/</param>
    static CodeTypeDeclaration BuildEnumClass(string className, List<string> typeValue)
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
    static CodeTypeDeclaration BuildClass(string className, Dictionary<string, string> classData)
    {
        //Code:代码体
        CodeTypeDeclaration myClass = new CodeTypeDeclaration(className);
        //指定为类
        myClass.IsClass = true;
        //设置类的访问类型
        myClass.TypeAttributes = TypeAttributes.Public;

        myClass.CustomAttributes.Add(new CodeAttributeDeclaration("Serializable"));

        CodeConstructor code = new CodeConstructor();

        code.Attributes = MemberAttributes.Public;

        code.Parameters.Add(new CodeParameterDeclarationExpression("Mono.Data.SqliteClient.SqliteDataReader", "sdr"));

        foreach (var item in classData)
        {
            myClass.Members.Add(BuildClassVar(item.Key, item.Value)); // 添加字段
            CodeVariableDeclarationStatement variableDeclaration = new CodeVariableDeclarationStatement($"{item.Key}=", GetConvertType(item.Value, item.Key));
            code.Statements.Add(variableDeclaration);
        }



        myClass.Members.Add(code);

        return myClass;
    }

    /// <summary>
    /// 添加字段
    /// </summary>
    /// <param name="varName">类名</param>
    /// <param name="varType">类型</param>
    static CodeMemberField BuildClassVar(string varName, string varType)
    {
        CodeMemberField field = new CodeMemberField(GetVarType(varType), varName);

        field.Attributes = MemberAttributes.Public;

        return field;
    }

    /// <summary>
    /// 获取数据类型
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    static CodeTypeReference GetVarType(string type)
    {
        CodeTypeReference _codeTypeReference = null;
        switch (type)
        {
            case "string":
            case "String":
            case "STRING":
                _codeTypeReference = new CodeTypeReference(typeof(string));
                break;
            case "int":
            case "INT":
            case "Int32":
                _codeTypeReference = new CodeTypeReference(typeof(int));
                break;
            case "long":
            case "LONG":
            case "Int64":
                _codeTypeReference = new CodeTypeReference(typeof(long));
                break;
            case "double":
            case "Double":
            case "DOUBLE":
                _codeTypeReference = new CodeTypeReference(typeof(double));
                break;
            case "float":
            case "Single":
            case "FLOAT":
                _codeTypeReference = new CodeTypeReference(typeof(float));
                break;
            case "bool":
            case "BOOL":
            case "Boolean":
            case "BOOLEAN":
                _codeTypeReference = new CodeTypeReference(typeof(bool));
                break;
            case "short":
            case "Int16":
            case "SHORT":
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
                _codeTypeReference = new CodeTypeReference(typeof(Enum));
                break;
            default:
                _codeTypeReference = new CodeTypeReference(type);
                break;
        }

        return _codeTypeReference;
    }


    static string GetType(string type)
    {
        Debug.Log(type);
        switch (type)
        {
            case "string":
            case "String":
            case "STRING":
                return "string";
            case "int":
            case "INT":
            case "Int32":
                return "int";
            case "long":
            case "LONG":
            case "Int64":
                return "long";
            case "double":
            case "Double":
            case "DOUBLE":
                return "double";
            case "float":
            case "Single":
            case "FLOAT":
                return "float";
            case "bool":
            case "BOOL":
            case "Boolean":
            case "BOOLEAN":
                return "bool";
            case "short":
            case "Int16":
            case "SHORT":
                return "short";
            case "byte":
            case "Byte":
                return "byte";
            case "ushort":
            case "UInt16":
                return "ushort";
            case "enum": //这里要做特殊处理
                return type;
            default:
                return type;
        }
    }


    static string GetConvertType(string type, string key)
    {
        string t = "";
        switch (type)
        {
            case "string":
            case "String":
            case "STRING":
                t = "ToString";
                break;
            case "int":
            case "INT":
            case "Int32":
                t = "ToInt32";
                break;
            case "long":
            case "LONG":
            case "Int64":
                t = "ToInt64";
                break;
            case "double":
            case "Double":
            case "DOUBLE":
                t = "ToDouble";
                break;
            case "float":
            case "Single":
            case "FLOAT":
                t = "ToSingle";
                break;
            case "bool":
            case "BOOL":
            case "Boolean":
            case "BOOLEAN":
                t = "ToBoolean";
                break;
            case "short":
            case "Int16":
            case "SHORT":
                t = "ToInt16";
                break;
            case "byte":
            case "Byte":
                t = "ToByte";
                break;
            case "ushort":
            case "UInt16":
                t = "ToUInt16";
                break;
            default:
                return $"sdr[\"{key}\"].To{type}()";
        }

        return $"Convert.{t}(sdr[\"{key}\"])";
    }

    /// 通过给定的文件流，判断文件的编码类型
    /// <param name="fs">文件流</param>
    /// <returns>文件的编码类型</returns>
    static Encoding GetType(FileStream fs)
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
    static bool IsUTF8Bytes(byte[] data)
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
