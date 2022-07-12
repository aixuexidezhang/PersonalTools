using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ZUI
{
    public class ZUIRoot : MonoBehaviour
    {
        public Canvas RootCanvas;
        public CanvasGroup RootCanvasGroup;
        public CanvasScaler RootCanvasScaler;
        public GraphicRaycaster RootGraphicRaycaster;
        public void Destroy()
        {
            Destroy(gameObject);
        }
    }
}