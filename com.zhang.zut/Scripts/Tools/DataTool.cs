using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using System.Xml.Linq;

namespace MyTool.Tools
{
    /// <summary>
    /// 数据操作工具
    /// </summary>
    public static class Data
    {


        //List保存完成
        #region 保存

        /// <summary>
        /// 当前List保存为Xml
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="ff"></param>
        /// <param name="path">路径</param>
        /// <param name="filename">文件名（包含后缀）</param>
        public static void XmlListSave<T>(this List<T> ff, string path, string filename)
        {
            Debug.Log("路径为" + path + @"\" + filename);
            Type type = typeof(List<T>);//获取你要序列化的类型
            XmlSerializer xs = new XmlSerializer(type);//序列化转成XML的格式
            FileStream fs = null;
            try
            {
                //创建新的 有则 覆盖
                fs = new FileStream(string.Concat(path, @"\", filename), FileMode.Create);
                //转码格式 U3D识别UTF8
                StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);

                //开始序列化保存
                xs.Serialize(sw, ff);
            }
            catch (Exception e)
            {

                Debug.Log(e);
            }
            finally
            {
                fs.Close();
            }


        }
        /// <summary>
        /// 当前数据保存为Xml
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="ff"></param>
        /// <param name="path">路径</param>
        /// <param name="filename">文件名（包含后缀）</param>
        public static void XmlSingleSave<T>(this T ff, string path, string filename)
        {
            try
            {
                List<T> my = new List<T>(); //
                my.Add(ff);
                XmlListSave(my, path, filename);
            }
            catch (Exception e)
            {

                throw e;
            }
        }
        /// <summary>
        /// 当前数组保存为Xml
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="ff"></param>
        /// <param name="path">路径</param>
        /// <param name="filename">文件名（包含后缀）</param>
        public static void XmlArraySave<T>(this T[] ff, string path, string filename)
        {
            try
            {
                XmlListSave(ff.ToList(), path, filename);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        #endregion


        //完成
        #region 添加


        /// <summary>
        /// 给当前List更新数据
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="ff"></param>
        /// <param name="path">路径</param>
        /// <param name="filename">文件名（包含后缀）</param>
        public static void XmlListUpdate<T>(this List<T> ff, string path, string filename)
        {
            List<T> myNameScoers = new List<T>(); //

            Type type = typeof(List<T>);//获取你要序列化的类型
            XmlSerializer xs = new XmlSerializer(type);//序列化转成XML的格式
            FileStream fs = null;//声明一个流
            if (File.Exists(path))//判断是否含有该文件
            {
                try
                {
                    //读原来的文件
                    myNameScoers = XmlRead<T>(path, filename);

                    //添加上一个所有数据
                    foreach (T item in ff)
                    {
                        myNameScoers.Add(item);
                    }
                    //选择创建流的模式
                    fs = new FileStream(string.Concat(path, @"\", filename), FileMode.Create);
                    //转码格式 U3D识别UTF8
                    StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);
                    //这个格式进行序列化
                    xs.Serialize(sw, myNameScoers);
                }
                catch (Exception e)
                {

                    Debug.Log("添加失败" + e);
                }
                finally
                {
                    fs.Close();
                }
            }
            else
            {
                try
                {
                    //选择创建流的模式
                    fs = new FileStream(string.Concat(path, @"\", filename), FileMode.Create);
                    //转码格式 U3D识别UTF8
                    StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);
                    //这个格式进行序列化
                    xs.Serialize(sw, ff);
                }
                catch (Exception e)
                {

                    Debug.Log("添加时候创建失败" + e);
                }
                finally
                {
                    fs.Close();
                }
            }
        }

        /// <summary>
        /// 添加单个
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="ff"></param>
        /// <param name="path">路径</param>
        /// <param name="filename">文件名（包含后缀）</param>
        public static void XmlSingleUpdate<T>(this T ff, string path, string filename)
        {
            List<T> d = new List<T>();
            d.Add(ff);
            XmlListUpdate(d, path, filename);
        }

        #endregion


        //完成
        #region 读取


        /// <summary>
        ///   /// <summary>
        /// 读取XML文件
        /// </summary>
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="path">路径</param>
        /// <param name="filename">文件名（包含后缀）</param>
        /// <returns></returns>
        public static List<T> XmlRead<T>(string path, string filename)
        {
            List<T> myNameScoers = new List<T>(); //
            Debug.Log($"读取的路径为{string.Concat(path, @"\", filename)}");
            Type type = typeof(List<T>);//获取你要序列化的类型
            XmlSerializer xs = new XmlSerializer(type);//序列化转成XML的格式
            if (File.Exists(string.Concat(path, @"\", filename)))//判断是否含有该文件
            {
                FileStream fs = null;
                try
                {
                    fs = new FileStream(string.Concat(path, @"\", filename), FileMode.Open);//打开该文件

                    myNameScoers = (List<T>)xs.Deserialize(fs);

                }
                catch (Exception e)
                {
                    Debug.LogError("打开错误,或者是类型不对" + e);
                }
                finally
                {
                    //流用完关掉
                    fs.Close();

                }
                return myNameScoers;
            }
            else
            {
                Debug.LogError("地址错误 检测不到文件");
                return null;
            }

        }


        /// <summary>
        /// 读取XML文件
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="path">路径</param>
        /// <param name="filename">文件名（包含后缀）</param>
        /// <param name="ff"></param>
        public static void XmlRead<T>(string path, string filename, out List<T> ff)
        {
            ff = XmlRead<T>(path, filename);
        }
        #endregion


        #region 删除指令

        /// <summary>
        /// 删除XML中某段数据
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="ff"></param>
        /// <param name="path">路径</param>
        /// <param name="filename">文件名（包含后缀）</param>
        public static void XmlListRemove<T>(this List<T> ff, string path, string filename)
        {
            //  M_Serialization_Save(ff,path);

            if (File.Exists(path))
            {
                if (XmlRead<T>(path, filename).GetType() != ff.GetType())
                {
                    Debug.LogError("当前要删除的数据类型和加载数据类型不符，请检查路径");
                }
                else
                {
                    List<T> Quondam_List = XmlRead<T>(path, filename);

                    List<T> Remove_List = new List<T>();

                    foreach (T item in ff)
                    {
                        //反射出要删除类里面的属性和字段
                        FieldInfo[] Remove_Field = item.GetType().GetFields();
                        PropertyInfo[] Remove_Property = item.GetType().GetProperties();
                        foreach (T item1 in Quondam_List)
                        {
                            //反射出原来数据里面类里面的属性和字段
                            FieldInfo[] Quondam_Field = item1.GetType().GetFields();
                            PropertyInfo[] Quondam_Property = item1.GetType().GetProperties();
                            //比较 是否一致 一致就删除
                            if (Judge_Equality(Quondam_Field, Quondam_Property, Remove_Field, Remove_Property))
                            {
                                Remove_List.Add(item1);
                            }

                        }
                    }
                    foreach (T item in Remove_List)
                    {
                        Quondam_List.Remove(item);
                    }
                    //最后保存
                    XmlListSave(Quondam_List, path, filename);

                }
            }
            else
            {
                Debug.LogError("在读取的时候找不到该文件，检查地址");
            }
        }

        /// <summary>
        /// 删除XML中某段数据  单个
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="ff"></param>
        /// <param name="path">路径</param>
        /// <param name="filename">文件名（包含后缀）</param>
        public static void XmlSingleRemove<T>(this T ff, string path, string filename)
        {
            List<T> Remove_List = new List<T>();
            Remove_List.Add(ff);
            XmlListRemove(Remove_List, path, filename);
        }

        /// <summary>
        /// 删除XML中某段数据 数组
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="ff"></param>
        /// <param name="path">路径</param>
        /// <param name="filename">文件名（包含后缀）</param>
        public static void M_Serialization_Remove<T>(T[] ff, string path, string filename)
        {
            List<T> Remove_List = new List<T>();

            foreach (T item in ff)
            {
                Remove_List.Add(item);
            }

            XmlListRemove(Remove_List, path, filename);
        }


        #endregion

        #region 更新数据

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="Quondam_data">之前的数据</param>
        /// <param name="Now_data">要更换的数据</param>
        /// <param name="path">路径</param>
        /// <param name="filename">文件名（包含后缀）</param>
        public static void UpdateData<T>(T Quondam_data, T Now_data, string path, string filename)
        {
            List<T> temp_List = XmlRead<T>(path, filename);
            FieldInfo[] Quondam_Field = Quondam_data.GetType().GetFields();
            PropertyInfo[] Quondam_Property = Quondam_data.GetType().GetProperties();
            //判断是哪个数据
            for (int i = 0; i < temp_List.Count; i++)
            {
                FieldInfo[] temp_Field = temp_List[i].GetType().GetFields();
                PropertyInfo[] temp_Property = temp_List[i].GetType().GetProperties();
                if (Judge_Equality(Quondam_Field, Quondam_Property, temp_Field, temp_Property))
                {
                    temp_List[i] = Now_data;
                    break;
                }
            }
            XmlListSave(temp_List, path, filename);
        }


        ///// <summary>
        ///// 在XML文件中根据名字更改值
        ///// </summary>
        ///// <typeparam name="T">泛型</typeparam>
        ///// <param name="name">通过哪个名字的数据找到他</param>
        ///// <param name="value">他的值多少</param>
        ///// <param name="changeName">要改变的数据叫什么</param>
        ///// <param name="changeValue">改成多少</param>
        ///// <param name="path">路径</param>
        ///// <param name="filename">文件名（包含后缀）</param>
        //public static void UpdateData<T>(string name, string value, string changeName, string changeValue, string path,string filename)
        //{
        //    XElement root = XElement.Load(path+filename);
        //    foreach (XElement item in root.Nodes())
        //    {
        //        if ((item.Element(name).Value).ToString() == value)
        //        {
        //            item.Element(changeName).Value = changeValue;
        //        }
        //    }
        //    root.Save(path);
        //}
        #endregion

        #region 根据名字查找某个值

        /// <summary>
        /// 在XML文件中根据名字查找值
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="changeName"></param>
        /// <param name="path">路径</param>
        /// <param name="filename">文件名（包含后缀）</param>
        public static string FindData<T>(string name, string value, string changeName, string path, string filename)
        {
            string temp = "";
            XElement root = XElement.Load(path + filename);
            foreach (XElement item in root.Nodes())
            {
                if ((item.Element(name).Value).ToString() == value)
                {
                    temp = item.Element(changeName).Value;
                }
            }
            return temp;
        }
        #endregion

        /// <summary>
        /// 获取某个路径下的某个后缀类型的文件
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="suffix"></param>
        /// <returns></returns>
        public static List<FileInfo> LoadAllFiles(string path, string suffix)
        {
            var process = "*." + suffix;
            string Path = path + process;
            string Suffix = process;
            return FindFileSearch(Path, Suffix);
        }

        /// <summary>
        /// 查找指定后缀名的文件
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="pattern">要查找的后缀名如*.txt</param>
        /// <returns></returns>
        public static List<FileInfo> FindFileSearch(string path, string pattern)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return FindFileSearch(new DirectoryInfo(path), pattern);
        }
        /// <summary>
        /// 查找指定后缀名的文件
        /// </summary>
        /// <param name="d">new DirectoryInfo(path)</param>
        /// <param name="pattern">要查找的后缀名如*.txt</param>
        /// <returns></returns>
        public static List<FileInfo> FindFileSearch(DirectoryInfo d, string pattern)
        {

            List<FileInfo> files = d.GetFiles(pattern).ToList();
            DirectoryInfo[] dis = d.GetDirectories();
            foreach (DirectoryInfo di in dis)
                files.AddRange(FindFileSearch(di, pattern));
            return files;
        }

        /// <summary>
        /// 判断里面的属性和字段的值是否都一致
        /// </summary>
        /// <param name="field"></param>
        /// <param name="property"></param>
        /// <param name="field2"></param>
        /// <param name="property2"></param>
        /// <returns></returns>
        public static bool Judge_Equality(FieldInfo[] field, PropertyInfo[] property, FieldInfo[] field2, PropertyInfo[] property2)
        {
            int temp = 0;
            foreach (FieldInfo item in field)
            {
                foreach (FieldInfo item2 in field2)
                {
                    if (item.Name == item2.Name)
                    {
                        temp++;
                    }
                }
            }

            foreach (PropertyInfo item in property)
            {
                foreach (PropertyInfo item2 in property2)
                {
                    if (item.Name == item2.Name)
                    {
                        temp++;
                    }
                }
            }


            return temp == field.Length + property.Length;
        }

        /// <summary>
        /// Unity路径
        /// </summary>
        public static class GetAssetPath
        {
            /// <summary>
            /// 获取绝对路径
            /// </summary>
            /// <param name="pathName"></param>
            /// <returns></returns>
            public static string GetAbsolutePath(string pathName)
            {
                return Application.dataPath + pathName.Replace("Assets", "");
            }
            /// <summary>
            /// 获取相对路径
            /// </summary>
            /// <param name="pathName"></param>
            /// <returns></returns>
            public static string GetRelativePath(string pathName)
            {
                const string forwardSlash = "/";
                const string backSlash = "\\";
                pathName = pathName.Replace(backSlash, forwardSlash);
                return pathName.Replace(Application.dataPath, "Assets");
            }
            /// <summary>
            /// 获取文件名
            /// </summary>
            /// <param name="path"></param>
            /// <returns></returns>
            public static string GetAssetName(string path)
            {
                return Path.GetFileName(path);
            }
        }
    }
}
