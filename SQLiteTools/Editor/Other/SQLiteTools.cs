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

    [MenuItem("Tools/SQLite/ת�����б�Ϊ��", false, 3)]

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
                    Debug.LogError(tn + "�Ѿ�����!!");
                    continue;
                }
                if (tn.Contains("old"))
                {
                    Debug.LogError($"{tn}��old��,���ᱻ���س�class");
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
        //׼��һ�������������Ԫ
        CodeCompileUnit unit = new CodeCompileUnit();
        //���������ռ䣨�����ָҪ���ɵ���Ŀռ䣩
        CodeNamespace myNamespace = new CodeNamespace();

        //�����Ҫ�������ռ�����
        myNamespace.Imports.Add(new CodeNamespaceImport("System"));


        CodeTypeDeclaration myClass = new CodeTypeDeclaration("SQLiteDatas");

        myClass.IsClass = true;

        myClass.TypeAttributes = TypeAttributes.Public;

        CodeComment com = new CodeComment("����Ϊ���ݶ�ȡ��", true);

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
                    Debug.LogError($"{d.Key}û������,����ʹ��List��������,");

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

        //�Ѹ������ռ���뵽��������Ԫ�������ռ伯����
        unit.Namespaces.Add(myNamespace);

        //����C#�ű�("VisualBasic"��VB�ű�)
        CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");
        CodeGeneratorOptions options = new CodeGeneratorOptions();

        //������:�����ŵ���ʽ{}
        options.BracingStyle = "C";

        //�Ƿ����ֶΡ����ԡ�����֮����ӿհ���
        options.BlankLinesBetweenMembers = true;

        //����ļ�·��
        string outputFile = string.Concat(GetClassPath, "database", ".cs");

        if (!Directory.Exists(GetClassPath))
        {
            Directory.CreateDirectory(GetClassPath);
            //����
            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(outputFile))
            {
                //Ϊָ���Ĵ����ĵ�����ģ��(CodeDOM) ���뵥Ԫ���ɴ��벢���䷢�͵�ָ�����ı���д����ʹ��ָ����ѡ�(�ٷ�����)
                //���Զ�����������(��������)���ʹ����ʽд�뵽sw��
                provider.GenerateCodeFromCompileUnit(unit, sw, options);
                var snippetMethod = new CodeSnippetTypeMember(sw.ToString());
                EditorUtility.ClearProgressBar();
            }
        }
        else
        {
            //����
            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(outputFile))
            {
                //Ϊָ���Ĵ����ĵ�����ģ��(CodeDOM) ���뵥Ԫ���ɴ��벢���䷢�͵�ָ�����ı���д����ʹ��ָ����ѡ�(�ٷ�����)
                //���Զ�����������(��������)���ʹ����ʽд�뵽sw��
                provider.GenerateCodeFromCompileUnit(unit, sw, options);
                EditorUtility.ClearProgressBar();
            }
        }
        Debug.Log("�������");
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// ɾ��֮ǰ���ɵ�������
    /// </summary>
    static void ClearAllClass()
    {
        try
        {
            DirectoryInfo dir = new DirectoryInfo(GetClassPath);
            if (dir.Exists)
            {
                FileSystemInfo[] fileinfo = dir.GetFileSystemInfos(); //����Ŀ¼�������ļ�����Ŀ¼
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

        //ÿ�е�����
        string strLine = "";
        while ((strLine = streamReader.ReadLine()) != null)
        {
            var arrLien = strLine.Split(',');
            enumNames.Add(arrLien[0]);
        }

        return BuildEnumClass(fileName, enumNames);
    }

    /// <summary>
    /// ���ɱ������� ��������
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
                Debug.LogError($"�ֶ�{item.Item1}�Ѿ�����");
                continue;
            }
            _calssData.Add(item.Item1, item.Item2);
        }

        return BuildClass(fileName, _calssData);
    }

    /// <summary>
    /// ����ö����
    /// </summary>
    /// <param name="className">ö��������</param>
    /// <param name="typeValue">ö�ٲ�������/</param>
    static CodeTypeDeclaration BuildEnumClass(string className, List<string> typeValue)
    {
        //Code:������
        CodeTypeDeclaration myClass = new CodeTypeDeclaration(className);

        //ָ��Ϊ��
        myClass.IsClass = true;
        //������ķ�������
        myClass.TypeAttributes = TypeAttributes.Public; // | TypeAttributes.Sealed;
        myClass.CustomAttributes.Add(new CodeAttributeDeclaration("Serializable"));
        myClass.IsEnum = true;
        myClass.TypeAttributes = TypeAttributes.Public;

        //���ö��ֵ
        foreach (var item in typeValue)
        {
            CodeMemberField field = new CodeMemberField(typeof(System.Enum), item);
            field.Attributes = MemberAttributes.Public;
            myClass.Members.Add(field);
        }

        return myClass;
    }

    /// <summary>
    /// ������
    /// </summary>
    /// <param name="className">����</param>
    /// <param name="classData">��������/��������</param>
    static CodeTypeDeclaration BuildClass(string className, Dictionary<string, string> classData)
    {
        //Code:������
        CodeTypeDeclaration myClass = new CodeTypeDeclaration(className);
        //ָ��Ϊ��
        myClass.IsClass = true;
        //������ķ�������
        myClass.TypeAttributes = TypeAttributes.Public;

        myClass.CustomAttributes.Add(new CodeAttributeDeclaration("Serializable"));

        CodeConstructor code = new CodeConstructor();

        code.Attributes = MemberAttributes.Public;

        code.Parameters.Add(new CodeParameterDeclarationExpression("Mono.Data.SqliteClient.SqliteDataReader", "sdr"));

        foreach (var item in classData)
        {
            myClass.Members.Add(BuildClassVar(item.Key, item.Value)); // ����ֶ�
            CodeVariableDeclarationStatement variableDeclaration = new CodeVariableDeclarationStatement($"{item.Key}=", GetConvertType(item.Value, item.Key));
            code.Statements.Add(variableDeclaration);
        }



        myClass.Members.Add(code);

        return myClass;
    }

    /// <summary>
    /// ����ֶ�
    /// </summary>
    /// <param name="varName">����</param>
    /// <param name="varType">����</param>
    static CodeMemberField BuildClassVar(string varName, string varType)
    {
        CodeMemberField field = new CodeMemberField(GetVarType(varType), varName);

        field.Attributes = MemberAttributes.Public;

        return field;
    }

    /// <summary>
    /// ��ȡ��������
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
            case "enum": //����Ҫ�����⴦��
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
            case "enum": //����Ҫ�����⴦��
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

    /// ͨ���������ļ������ж��ļ��ı�������
    /// <param name="fs">�ļ���</param>
    /// <returns>�ļ��ı�������</returns>
    static Encoding GetType(FileStream fs)
    {
        byte[] Unicode = new byte[] { 0xFF, 0xFE, 0x41 };
        byte[] UnicodeBIG = new byte[] { 0xFE, 0xFF, 0x00 };
        byte[] UTF8 = new byte[] { 0xEF, 0xBB, 0xBF }; //��BOM
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

    /// �ж��Ƿ��ǲ��� BOM �� UTF8 ��ʽ
    /// <param name="data"></param>
    /// <returns></returns>
    static bool IsUTF8Bytes(byte[] data)
    {
        int charByteCounter = 1; //���㵱ǰ���������ַ�Ӧ���е��ֽ���
        byte curByte; //��ǰ�������ֽ�.
        for (int i = 0; i < data.Length; i++)
        {
            curByte = data[i];
            if (charByteCounter == 1)
            {
                if (curByte >= 0x80)
                {
                    //�жϵ�ǰ
                    while (((curByte <<= 1) & 0x80) != 0)
                    {
                        charByteCounter++;
                    }

                    //���λ��λ��Ϊ��0 ��������2��1��ʼ ��:110XXXXX...........1111110X��
                    if (charByteCounter == 1 || charByteCounter > 6)
                    {
                        return false;
                    }
                }
            }
            else
            {
                //����UTF-8 ��ʱ��һλ����Ϊ1
                if ((curByte & 0xC0) != 0x80)
                {
                    return false;
                }

                charByteCounter--;
            }
        }

        if (charByteCounter > 1)
        {
            throw new Exception("��Ԥ�ڵ�byte��ʽ");
        }

        return true;
    }
}
