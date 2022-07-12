using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyTool.UnityComponents;
using ZUI;

namespace ZUI
{
    public interface ZUIBase<in TParam>
    {
        void OnInit(TParam value);
    }
    public interface ZUIBase
    {
        void OnInit();
    }
    public abstract class ZUIWindowBase : MonoBehaviour 
    {
    }
}
public abstract class ZUIWindow : ZUIWindowBase, ZUIBase
{
    public abstract void OnInit();
}
public abstract class ZUIWindow<TParam> : ZUIWindowBase, ZUIBase<TParam>
{
    public abstract void OnInit(TParam value);
}

