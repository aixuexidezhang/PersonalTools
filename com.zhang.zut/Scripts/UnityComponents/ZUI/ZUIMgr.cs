using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyTool.UnityComponents;
using DG.Tweening;
using MyTool.Tools;
using ZUI;
using System;
using System.Linq;
public static class ZUIMgr
{
    /// <summary>
    /// UI打开和关闭的动效
    /// </summary>
    public enum OpenViewType
    {
        /// <summary>
        /// 缩放
        /// </summary>
        Scale, 
        /// <summary>
        /// 渐变
        /// </summary>
        Alpha, 
        /// <summary>
        /// 移动
        /// </summary>
        Move
    }
    static List<ZUIRoot> rootStack;
    static PrefabDatas library;
    static float time = 0.2f;
    static OpenViewType openType = OpenViewType.Alpha;
    static GameObject cr;
    static ZUIRoot rootprefabparent;
    /// <summary>
    /// 是否还有窗口可以关闭
    /// </summary>
    public static bool IsClose => (rootStack != null && rootStack.Count > 0);
    /// <summary>
    /// 初始化UI框架
    /// </summary>
    public static void Init (PrefabDatas datas)
    {
        if (datas == null)
        {
            Debug.LogError("library为空");
            return;
        }
        library = datas;
        rootprefabparent = library.Get("CanvasRoot").GetComponent<ZUIRoot>();
        if (rootprefabparent == null)
        {
            Debug.LogError("CanvasRoot预制体没有找到");
        }
        if (rootStack == null)
        {
            rootStack = new List<ZUIRoot>();
            cr = new GameObject("CanvasRoot");
        }
    }


    /// <summary>
    /// 打开一个有参UI窗口
    /// </summary>
    public static void Open<T>(string name, T value)
    {
        var Tuple = New(name);
        var zui = Tuple.Item2.GetComponent<ZUIWindow<T>>();
        if (zui == null)
        {
            Debug.LogError(string.Concat(typeof(T).Name, "参数类型不对"));
            return;
        }
        Tuple.Item1.gameObject.SetActive(true);
        OpenView(Tuple.Item1);
        zui.OnInit(value);
    }

    /// <summary>
    /// 打开一个无参UI窗口
    /// </summary>
    /// <param name="name"></param>
    public static void Open(string name)
    {
        var Tuple = New(name);
        var zui = Tuple.Item2.GetComponent<ZUIWindow>();
        if (zui == null)
        {
            Debug.LogError(string.Concat(name, "需要参数"));
            return;
        }
        Tuple.Item1.gameObject.SetActive(true);
        OpenView(Tuple.Item1);
        zui.OnInit();

    }

    /// <summary>
    /// 关闭最顶上的UI
    /// </summary>
    public static void Close()
    {
        if (IsClose)
        {
            CloseView(rootStack[rootStack.Count - 1]);
        }
    }
    /// <summary>
    /// 关闭最顶上的UI
    /// </summary>
    public static void Close(string name)
    {
        if (IsClose)
        {
            var f = rootStack.Find(s => s.name.Equals(name + "_Root"));
            if (f != null) CloseView(f);
        }
    }

    static Tuple<ZUIRoot, GameObject> New(string name)
    {
        GameObject prefab = library.Get(name);
        if (prefab == null)
        {
            Debug.LogError(string.Concat(name, "预制体没有找到"));
            return null;
        }
        var root = GameObject.Instantiate(rootprefabparent, cr.transform);
        rootStack.Add(root);
        root.name = name + "_Root";
        root.RootCanvas.sortingOrder = rootStack.Count;
        var obj = GameObject.Instantiate(prefab, root.transform);
        obj.name = prefab.name;
        return new Tuple<ZUIRoot, GameObject>(root, obj);
    }
    static void OpenView(ZUIRoot obj)
    {
        switch (openType)
        {
            case OpenViewType.Scale:
                obj.transform.localScale = Vector3.zero;
                obj.transform.DOScale(Vector3.one, time);
                break;
            case OpenViewType.Alpha:
                obj.RootCanvasGroup.alpha = 0;
                obj.RootCanvasGroup.Fade(1, time);
                break;
            case OpenViewType.Move:
                obj.transform.position = Vector3.up * 2000;
                obj.transform.DOMove(Vector3.zero, time);
                break;
        }
    }
    static void CloseView(ZUIRoot obj)
    {
        rootStack.Remove(obj);
        Tweener tw=null;
        switch (openType)
        {
            case OpenViewType.Scale:
                tw = obj.transform.DOScale(Vector3.zero, time);
                break;
            case OpenViewType.Alpha:
                tw = obj.RootCanvasGroup.Fade(0, time);
                break;
            case OpenViewType.Move:
                tw = obj.transform.DOMove(Vector3.up * 2000, time);
                break;
        }
        tw.onComplete = obj.Destroy;
    }

}
