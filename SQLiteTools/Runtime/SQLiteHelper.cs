using System;
using System.IO;
using UnityEngine;
using System.Data;
using Mono.Data.SqliteClient;
using System.Collections;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;

public class SQLiteHelper
{
    static SQLiteHelper init;

    /// <summary>
    /// 单例模式,安卓平台使用之前需要初始化,运行Init函数
    /// </summary>
    public static SQLiteHelper Init { get { if (init == null) init = new SQLiteHelper(); return init; } }

    /// <summary>
    /// 安卓平台使用之前需要运行此函数
    /// </summary>
    /// <param name="over"></param>
    /// <returns></returns>
    public async void OnInit(Action over = null) 
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            var readPath = Path.Combine(Application.persistentDataPath, "Data.db");

            string sql_DirPath = Path.Combine(Application.streamingAssetsPath, "Data.db");//找到streamingAssets下的数据库文件位置

            if (File.Exists(readPath))
            {
                File.Delete(readPath);
                Debug.Log("更新数据库文件");
            }

            Uri ri = new Uri(sql_DirPath);

            UnityWebRequest wr = UnityWebRequest.Get(ri);

            wr.SendWebRequest();

            while (!wr.isDone)
            {
                await UniTask.Yield(PlayerLoopTiming.Update);
            }

            if (wr.isDone)
            {
                File.Create(readPath).Dispose();
                File.WriteAllBytes(readPath, wr.downloadHandler.data);
                Debug.Log("写入完成:" + readPath);
            }
            over?.Invoke();
        }
    }


    /// <summary>连接字符串</summary>
    public string Connstring
    {
        get 
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                return "URI=file:" + Path.Combine(Application.persistentDataPath, "Data.db");
            }
            else
            {
                return "URI=file:" + Path.Combine(Application.streamingAssetsPath, "Data.db");
            }
        }
    }
 
    /// <summary>
    /// 查询多行
    /// </summary>
    /// <param name="sql">SQL语句</param>
    /// <param name="pms">SQL参数</param>
    /// <returns>返回SqlDataReader对象</returns>
    public SqliteDataReader ExcuteReader(string sql, params object[] args)
    {
        SqliteConnection conn = new SqliteConnection(Connstring);
        using (SqliteCommand _command = new SqliteCommand(sql, conn))
        {
            return _command.ExecuteReader();
        }
    }


    /// <summary>
    /// 新建数据库文件
    /// </summary>
    /// <param name="dbPath">数据库文件路径及名称</param>
    /// <returns>新建成功，返回true，否则返回false</returns>
    public bool NewDbFile(string dbPath)
    {
        try
        {
            
            return true;
        }
        catch (Exception ex)
        {
            throw new Exception("新建数据库文件" + dbPath + "失败：" + ex.Message);
        }
    }
}