using DG.Tweening;
using MyTool.Tools.CSharpExtension;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

public class Test : MonoBehaviour
{
    private void Start()
    {
#if true //Switch
Debug.LogError(1);
#endif
        var p = Path.Combine(Application.dataPath, "Test", "Test.cs");
        string text = File.ReadAllText(p).Replace("#if true //Switch", "#if true //Switch");
        File.WriteAllText(p,text);
        //p.
    }

 
}


