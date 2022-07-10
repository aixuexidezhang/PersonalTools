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
  public static IEnumerator Load()
    {
        var t = typeof(DataMgr);
        FieldInfo[] tfInfos = t.GetFields(BindingFlags.Public | BindingFlags.Static);

        foreach (var item in tfInfos)
        {
            var d = item.GetValue(t) as IDictionary;

#if UNITY_ANDROID && !UNITY_EDITOR
            var uri = new System.Uri(Path.Combine(Application.streamingAssetsPath, "Tables", item.Name + ".csv"));
            var request = UnityWebRequest.Get(uri.AbsoluteUri);
            yield return request.SendWebRequest();
            d.LoadDicAndroid(item.Name, request.downloadHandler.text);
            yield return null;
#else
            d.LoadDic(item.Name);
            yield return null;
#endif
        }
        Debug.Log("所有的Table加载完毕");
    }
}
