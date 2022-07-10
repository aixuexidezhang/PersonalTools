using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static  class HierachryEditor
{

    [MenuItem("GameObject/Zhang/ClayerAllRaycastTarget", priority = 0)]
    static void Init()
    {
        int count = 0;

        Transform[] transforms = Selection.transforms;
        if (transforms.Length>0)
        {
            foreach (var item in transforms)
            {
                Recurve(item);
            }
            Debug.Log($"一共清除了{count}个RaycastTarget");
        }

        void Clayer(Transform t)
        {
            Graphic g;
            if (t.TryGetComponent(out g))
            {
                count++;
                g.raycastTarget = false;
            }
        }
        void Recurve(Transform item)
        {
            Clayer(item);
            foreach (var i in item)
            {
                var ts = i as Transform;
                Recurve(ts);
            }
        }
    }
}
