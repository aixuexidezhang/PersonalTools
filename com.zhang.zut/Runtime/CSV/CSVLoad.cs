using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
/// <summary>
/// 读取CSV内容
/// </summary>
public static class CSVLoad
{
    /// <summary>
    /// 加载数据到dic
    /// </summary>
    /// <param name="t">目标dic</param>
    /// <param name="recName">表名</param>
    /// <param name="key"></param>
    public static void LoadDic(this IDictionary t, string name, string key = "id")
    {
        try
        {
            IList recs = DataTableLoader.loadTables(name);
            System.Type recType = Assembly.GetCallingAssembly().GetType(name);
            FieldInfo kfInfo = recType.GetField(key);
            foreach (var r in recs)
            {
                try
                {
                    t.Add(kfInfo.GetValue(r), r);
                }
                catch (ArgumentException e)
                {
                    Debug.LogError(e);
                    Debug.LogError(string.Format("表名:{0}\t键:" + kfInfo.GetValue(r), name));
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    /// <summary>
    /// 加载数据到dic
    /// </summary>
    /// <param name="t">目标dic</param>
    /// <param name="recName">表名</param>
    /// <param name="key"></param>
    public static void LoadDicAndroid(this IDictionary t, string name, string value, string key = "id")
    {
        try
        {
            IList recs = DataTableLoader.loadTablesAndroid(name, value);
            System.Type recType = Assembly.GetCallingAssembly().GetType(name);
            FieldInfo kfInfo = recType.GetField(key);
            foreach (var r in recs)
            {
                try
                {
                    t.Add(kfInfo.GetValue(r), r);
                }
                catch (ArgumentException e)
                {
                    Debug.LogError(e);
                    Debug.LogError(string.Format("表名:{0}\t键:" + kfInfo.GetValue(r), name));
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    /// <summary>
    /// 读取csv的内容并且保存成List
    /// </summary>
    /// <param name="ListName">list的变量名</param>
    //public static void LoadList(string ListName)
    //{
    //    FieldInfo tfInfo = GetType().GetField(ListName);
    //    IList t = tfInfo.GetValue(this) as IList;
    //    IList recs = DataTableLoader.loadRecords(ListName);
    //    for (int i = 0; i < recs.Count; i++)
    //        t.Add(recs[i]);
    //}

    public static class DataTableLoader
    {

        struct field
        {
            public string Type;
            public string Name;
            public int Length;
            public field(string s)
            {
                this.Length = -1;
                this.Name = s.Split(':')[0];
                this.Type = s.Split(':')[1];
                if (this.Type.IndexOf("string(") > -1)
                {
                    var m = Regex.Match(this.Type, @"string\((?<len>\d+)\)");
                    this.Type = "string";
                    this.Length = int.Parse(m.Groups["len"].Value);
                }
            }
        }

        public static IList loadTables(string name)
        {
            var recName = name + ".csv";
            string datapath = "Tables";
            string strategyDir = Path.Combine(Application.streamingAssetsPath, datapath);
            string filePath = Path.Combine(strategyDir, recName);
            System.Type recType = Assembly.GetCallingAssembly().GetType(name);
            FieldInfo[] fieldInfos = recType.GetFields();
            List<object> recList = new List<object>();
            List<field> fields_ = new List<field>();

            FastCSVParser.ParseFile(filePath, Encoding.UTF8, delegate (int index, List<string> line)
            {
                if (index > 0)
                {

                    var recObj = System.Activator.CreateInstance(recType);
                    for (int i = 0; i < fieldInfos.Length; i++)
                    {
                        try
                        {
                            if (fieldInfos[i].FieldType.Equals(typeof(string)) && fields_[i].Length > -1 && fields_[i].Length < line[i].Length)
                                throw new FormatException("string too long !!!!!!!");
                            object value = default;
                            if (!fieldInfos[i].FieldType.IsEnum)
                            {
                                value = Convert.ChangeType(line[i], fieldInfos[i].FieldType);
                            }
                            else
                            {
                                if (int.TryParse(line[i], out int eunmcode))
                                {
                                    value = Enum.ToObject(fieldInfos[i].FieldType, eunmcode);
                                }
                                else
                                {
                                    value = Enum.Parse(fieldInfos[i].FieldType, line[i]);
                                }

                            }
                            fieldInfos[i].SetValue(recObj, value);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError(e);
                            Debug.LogError(string.Format("数据表:{0} 内容格式不正确!!!", recName));

                            if (line.Count != fieldInfos.Length)
                            {
                                Debug.LogError("字段数与数据列数不符");
                                return;
                            }

                            Debug.LogError(string.Format("行:{0}\t列:{1}\t类型:{2}\t值:{3}", index, i + 1, fields_[i].Type, line[i]));
                        }
                    }
                    recList.Add(recObj);
                }
                else
                {
                    try
                    {
                        foreach (var v in line)
                        {
                            fields_.Add(new field(v));
                        }
                    }
                    catch (Exception)
                    {
                        Debug.LogError(filePath + "格式错误");
                    }

                }

            });
            return recList;
        }
        public static IList loadTablesAndroid(string name, string value)
        {

            System.Type recType = Assembly.GetCallingAssembly().GetType(name);
            FieldInfo[] fieldInfos = recType.GetFields();
            List<object> recList = new List<object>();
            List<field> fields_ = new List<field>();

            FastCSVParser.ParseFileAndroid(value, Encoding.UTF8, delegate (int index, List<string> line)
            {
                if (index > 0)
                {

                    var recObj = System.Activator.CreateInstance(recType);
                    for (int i = 0; i < fieldInfos.Length; i++)
                    {
                        try
                        {
                            if (fieldInfos[i].FieldType.Equals(typeof(string)) && fields_[i].Length > -1 && fields_[i].Length < line[i].Length)
                                throw new FormatException("string too long !!!!!!!");

                            fieldInfos[i].SetValue(recObj, Convert.ChangeType(line[i], fieldInfos[i].FieldType));
                        }
                        catch (Exception e)
                        {
                            Debug.LogError(e);
                            Debug.LogError(string.Format("数据表:{0} 内容格式不正确!!!", name));

                            if (line.Count != fieldInfos.Length)
                            {
                                Debug.LogError("字段数与数据列数不符");
                                return;
                            }

                            Debug.LogError(string.Format("行:{0}\t列:{1}\t类型:{2}\t值:{3}", index, i + 1, fields_[i].Type, line[i]));
                        }
                    }
                    recList.Add(recObj);
                }
                else
                {
                    try
                    {
                        foreach (var v in line)
                        {
                            fields_.Add(new field(v));
                        }
                    }
                    catch (Exception)
                    {
                        Debug.LogError(name);
                    }

                }

            });
            return recList;
        }

    }

    public static class FastCSVParser
    {
        static string content_;
        static int lineIndex_;
        static int curPos_;

        public delegate void ReadLineDelegate(int lineIndex, List<string> line);

        public static void ParseFile(string file, Encoding encoding, ReadLineDelegate callback)
        {
            string content = "";
            var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var reader = new StreamReader(stream, encoding);
            content = reader.ReadToEnd();
            reader.Dispose();
            stream.Dispose();
            ParseContent(content, callback);
        }


        public static void ParseFileAndroid(string file, Encoding encoding, ReadLineDelegate callback)
        {
            ParseContent(file, callback);
        }
        public static void ParseContent(string content, ReadLineDelegate callback)
        {
            content_ = content;
            curPos_ = 0;
            lineIndex_ = 0;
            while (curPos_ < content_.Length)
            {
                var line = parseLine();
                callback(lineIndex_, line);
                curPos_++;
                lineIndex_++;
            }
        }
        static List<string> parseLine()
        {
            List<string> line = new List<string>();
            do
            {
                line.Add(field());
                curPos_++;
                if (curPos_ < content_.Length)
                {
                    char ahead = content_[curPos_];
                    switch (ahead)
                    {
                        case '\r':
                            if (++curPos_ < content_.Length && content_[curPos_] == '\n')
                                return line;
                            else
                                throw new Exception(string.Format("syntax error in line:{0}", lineIndex_));
                        case '\n':
                            return line;
                        case ',':
                            curPos_++;
                            // todo: 2017/2/23 wangweining
                            if (curPos_ == content_.Length)  // 已经到了EOF, field = *TEXTDATA
                            {
                                line.Add("");
                                return line;
                            }
                            continue;
                        default:
                            throw new Exception(string.Format("syntax error in line:{0}", lineIndex_));
                    }
                }
            } while (curPos_ < content_.Length);
            return line;
        }
        static string field()
        {
            switch (content_[curPos_])
            {
                case '"':
                    return escaped();
                default:
                    return non_escaped();
            }
        }

        static string escaped()
        {
            StringBuilder builder = new StringBuilder();
            char c = content_[curPos_];
            if (c != '"')
                throw new Exception(string.Format("syntax error in line:{0}", lineIndex_));
            for (curPos_++; curPos_ < content_.Length; curPos_++)
            {
                c = content_[curPos_];
                switch (c)
                {
                    case '"':
                        if (curPos_ + 1 >= content_.Length)
                            return builder.ToString();
                        char ahead = content_[++curPos_];
                        if (ahead == '"')
                            builder.Append(c);
                        else
                        {
                            curPos_--;
                            return builder.ToString();
                        }
                        break;
                    default:
                        builder.Append(c);
                        break;
                }
            }
            return builder.ToString();
        }
        static string non_escaped()
        {
            StringBuilder builder = new StringBuilder();
            char c;
            for (; curPos_ < content_.Length; curPos_++)
            {
                c = content_[curPos_];
                switch (c)
                {
                    case ',':
                        curPos_--;
                        return builder.ToString();
                    case '\n':
                        curPos_--;
                        return builder.ToString();
                    case '\r':
                        curPos_--;
                        return builder.ToString();
                    default:
                        builder.Append(c);
                        break;
                }
            }

            return builder.ToString();
        }
    }

}