using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Reflection;
using System.Threading.Tasks;
using System.IO;
using UnityEngine.Networking;

public class DataMgr
{
    public static IEnumerator Load(Action OverCellback = null)
    {
        yield return null;
        OverCellback?.Invoke();
        Debug.Log("所有的Table加载完毕");
    }

    /// <summary>
    /// 自动加载,所有的表的Key只能是Int的id时才可以使用
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="OverCellback"></param>
    public static async void AutoLoad(Type type, Action OverCellback = null)
    {
        try
        {
            FieldInfo[] tfInfos = type.GetFields(BindingFlags.Public | BindingFlags.Static);
            foreach (var item in tfInfos)
            {
                var d = item.GetValue(type) as IDictionary;
#if UNITY_ANDROID && !UNITY_EDITOR
            var uri = new System.Uri(Path.Combine(Application.streamingAssetsPath, "Tables", item.Name + ".csv"));
            var request = UnityWebRequest.Get(uri.AbsoluteUri);
            var swr = request.SendWebRequest();
            while (!swr.isDone)
            {
                await Task.Delay(10);
            }
            d.LoadDicAndroid(item.Name, request.downloadHandler.text);
            await Task.Delay(10);

#else
                d.LoadDic(item.Name,"Id");
                await Task.Delay(10);
#endif
            }
            Debug.Log("所有的Table加载完毕");
            OverCellback?.Invoke();
        }
        catch (Exception e)
        {

            throw e;
        }
    }
}
