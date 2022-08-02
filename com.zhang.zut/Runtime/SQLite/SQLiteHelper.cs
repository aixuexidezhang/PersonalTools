using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.IO;
using UnityEngine;
using System.Data;
using System.Reflection;
using SQLite4Unity3d;

public static class SQLiteHelper
{
    //需要添加引用--框架，System.Configuration，添加再using，用来读取配置文件的数据库链接字符串
    public static readonly string conStr = "Data Source= " + Path.Combine(Application.streamingAssetsPath, "Data.db");
    //static readonly string conStr = "Data Source = C:/Users/23138/BadGame/BadGame4/Assets/Data.db";
    // private static readonly string conStr = ConfigurationManager.ConnectionStrings["mssql"].ConnectionString;
    /// <summary>
    /// 封装增加、删、改方法，方法名可以随便写，但是为了方便查看，用了和SqlCommand类一一样的方法名
    /// </summary>
    /// <param name="sql">SQL语句</param>
    /// <param name="pms">SQL参数，因为不知道会有多少个参数，所以用可变参数params</param>
    /// <returns>受影响的行数</returns>
    public static int ExecuteNonQuery(string sql, params object[] args)
    {
        using (SQLiteConnection conn = new SQLiteConnection(conStr))
        {
            return conn.Execute(sql, args);
        }

    }

    /// <summary>
    /// 查询单个结果，一般和聚合函数 一起使用
    /// </summary>
    /// <param name="sql">查询的SQL语句</param>
    /// <param name="pms">SQL参数</param>
    /// <returns>返回查询对象，查询结果第一行第一列</returns>
    public static T ExecuteScalar<T>(string sql, params object[] args)
    {

        using (SQLiteConnection conn = new SQLiteConnection(conStr))
        {
            return conn.ExecuteScalar<T>(sql, args);
        }

    }
 



    /// <summary>
    /// 查询多行
    /// </summary>
    /// <param name="sql">SQL语句</param>
    /// <param name="pms">SQL参数</param>
    /// <returns>返回SqlDataReader对象</returns>
    public static List<T> ExcuteReader<T>(string sql, params object[] args) where T : new()
    {
        //这里不能用using，不然在返回SqlDataReader时候会报错，因为返回时候已经在using中关闭了。
        //事实上，在使用数据库相关类中，SqlConnection是必须关闭的，但是其他可以选择关闭，因为CG回自动回收
        SQLiteConnection conn = new SQLiteConnection(conStr);
        return conn.Query<T>(sql, args);
    }


    /// <summary>
    /// 新建数据库文件
    /// </summary>
    /// <param name="dbPath">数据库文件路径及名称</param>
    /// <returns>新建成功，返回true，否则返回false</returns>
    public static bool NewDbFile(string dbPath)
    {
        try
        {
            var ds = new DataService(dbPath);
            ds.CreateDB();
            return true;
        }
        catch (Exception ex)
        {
            throw new Exception("新建数据库文件" + dbPath + "失败：" + ex.Message);
        }
    }
}