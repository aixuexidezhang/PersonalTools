using MyTool.Tools;
using System;
using UnityEngine;
using UnityEngine.EventSystems;


namespace MyTool.UnityComponents
{
    /// <summary>
    /// 拖拽UI元素
    /// </summary>
    public class UI_Drag : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        [Serializable]
        public class UI_OnDrag
        {
            public ShortcutsCellback StartDrag;
            public ShortcutsCellback Drag;
            public ShortcutsCellback EndDrag;
        }
        [SerializeField] UI_OnDrag Shortcuts;


        //[SerializeField] bool AutomaticCorrection;

        Vector3 offPos;//存储按下鼠标时的图片-鼠标位置差
        Vector3 arragedPos; //保存经过整理后的向量，用于图片移动

        Vector3 startpos;
        /// <summary>
        /// 开始拖拽的时候
        /// </summary>
        /// <param name="eventData"></param>
        public void OnBeginDrag(PointerEventData eventData)
        {
            
            //if (camera == null)
            //{
            //    startpos = transform.position;
            //    camera = Camera.main;
            //}
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(transform.GetComponent<RectTransform>(), Input.mousePosition, eventData.enterEventCamera, out arragedPos))
            {
                offPos = transform.position - arragedPos;
                Shortcuts.StartDrag?.Invoke();
            }
        }
        /// <summary>
        /// 拖拽中
        /// </summary>
        /// <param name="eventData"></param>
        public void OnDrag(PointerEventData eventData)
        {
            transform.position = offPos + Input.mousePosition;
            Shortcuts.Drag?.Invoke();
        }

        /// <summary>
        /// 拖拽完毕
        /// </summary>
        /// <param name="eventData"></param>
        public void OnEndDrag(PointerEventData eventData)
        {
            Shortcuts.EndDrag?.Invoke();
            //if (AutomaticCorrection)
            //{
            //    Detection();
            //}
        }

        void Detection()
        {
            var rts = transform.GetComponent<RectTransform>();
            if (rts != null)
            {
                var v = transform.position;
                if (!JudgmentUiInScreen(rts, ref v))
                {
                    transform.position = startpos;
                }
            }
        }
        bool JudgmentUiInScreen(RectTransform rect, ref Vector3 targetPos)
        {
            bool isInView = false;
            float moveDistance = 0;
            Vector3 worldPos = rect.transform.position;
            float leftX = worldPos.x - rect.sizeDelta.x / 2;
            float rightX = worldPos.x + rect.sizeDelta.x / 2;
            if (leftX >= 0 && rightX <= Screen.width)
            {
                isInView = true;
            }
            else
            {
                if (leftX < 0)//需要右移进入屏幕范围
                {
                    moveDistance = -leftX;
                }
                if (rightX > Screen.width)//需要左移进入屏幕范围
                {
                    moveDistance = Screen.width - rightX;
                }
                targetPos = transform.position + new Vector3(moveDistance, 0, 0);
            }
            return isInView;
        }

    }

}