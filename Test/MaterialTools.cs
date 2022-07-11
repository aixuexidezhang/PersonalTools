using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MaterialTools
{
    static List<Material> mats = new List<Material>();
    static List<Texture2D> texs = new List<Texture2D>();
    static bool isover = false;

    [MenuItem("Assets/My Tools/AutoSetMaterial", false)]
    public static void AutoSetMaterial() 
    {
        try
        {
            mats.Clear();

            foreach (var item in Selection.objects)
            {
                if (item.GetType() == typeof(Material))
                {
                    var m = item as Material;
                    if (m != null)
                    {
                        mats.Add(m);
                    }
                    else
                    {
                        Debug.LogError(item.name + ":出错!!");
                    }
                }
                else if (item.GetType() == typeof(Texture2D))
                {
                    var t = item as Texture2D;
                    if (t != null)
                    {
                        texs.Add(t);
                    }
                    else
                    {
                        Debug.LogError(item.name + ":出错!!");
                    }
                }
            }

            foreach (var item in mats)
            {
                SetMap(item);
            }

        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
            throw;
        }
        if (isover)
        {
            Debug.LogError("完成");
        }
    }


 

    private static void SetMap(Material mat)
    {

        var m1 = GetTexture(mat.name + "_BaseColor");
        var m2 = GetTexture(mat.name + "_Metallic");
        var m3 = GetTexture(mat.name + "_Normal");
        var m4 = GetTexture(mat.name + "_Roughness");



        mat.SetTexture("_MainTex", m1);
        mat.SetTexture("_MetallicGlossMap", m2);
        mat.SetTexture("_BumpMap", m3);
        mat.SetTexture("_DetailMask", m4);
        mat.color = Color.white;

        if (!m1 || !m2 || !m3 || !m4)
        {
            string none="";
            if (!m1)
            {
                none += "BaseColor--"; 
            }
            if (!m2)
            {
                none += "Metallic--";
            }
            if (!m3)
            {
                none += "Normal--";
            }
            if (!m4)
            {
                none += "Roughness--";
            }
            Debug.LogError(mat.name+"贴图缺失: "+ none);
            isover = false;
        }
        else
        {
            isover = true;
        }
    }

    private static Texture GetTexture(string tstr)
    {
        foreach (var item in texs)
        {
            if (item.name.Equals(tstr))
            {
                return item;
            }
        }
        return null;
    }
}
