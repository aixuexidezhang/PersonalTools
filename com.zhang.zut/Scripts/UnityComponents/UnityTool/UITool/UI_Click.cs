using DG.Tweening;
using MyTool.Tools;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using UnityEngine.Events;
using DG.Tweening.Core;

namespace MyTool.UnityComponents
{
    public class UI_Click : UIBehaviour,
        IPointerClickHandler,
        IPointerDownHandler,
        IPointerUpHandler,
        IUIClickable
    {

        [Serializable]
        public class UI_OnClick
        {
            public ShortcutsCellback OnClick;
            public ShortcutsCellback OnClickUp;
            public ShortcutsCellback OnClickDown;
        }

        [SerializeField] bool _Interactable = true;
        [SerializeField] bool _Effect = true;
        [SerializeField] bool _Music = false;
        [SerializeField] bool _ClickColor = false;

        [ConditionalHide("_ClickColor", true)]
        [SerializeField] Color _Color = Color.white;




        [SerializeField] UI_OnClick Shortcuts;

        [SerializeField] AudioSource _CustomAudioSource;
        [SerializeField] AudioClip _CustomAudioClip;

        public PointerEventData PointerEventData { get; set; }


        private Graphic Graphic;
        private Vector3 InitScale;
        private Vector3 TweenToScale;
        private Tweener ScaleTweener;
        private Tweener ColorTweener;
        private Transform ZoomTargetTrans;
        private Color StartColor;
        private Action OnClick;
        private Action OnClickUp;
        private Action OnClickDown;

        protected override void Awake()
        {
            base.Awake();
            Graphic = Graphic == null ? gameObject.GetComponent<Graphic>() : Graphic;
            StartColor = Graphic.color;
            ZoomTargetTrans = transform;
            if (_Music) _CustomAudioSource = _CustomAudioSource == null ? gameObject.AddComponent<AudioSource>() : _CustomAudioSource;
            StateDisplay();
        }


        private void StateDisplay()
        {
            if (Graphic == null) return;
            if (!_Interactable)
                Graphic.color = Color.Lerp(StartColor, new Color(0, 0, 0, 0.6f), 0.8f);
            else
                Graphic.color = StartColor;
        }

#if UNITY_EDITOR
    protected virtual void Reset()
    {
        Graphic = GetComponent<Graphic>();
        if (!Graphic)
        {
            Debug.LogError("没有可点击的对象！！！");
        }
    }
#endif

        protected virtual void Start()
        {
            ResetScale();
        }

        /// <summary>
        /// 重置比例
        /// </summary>
        public void ResetScale()
        {
            InitScale = transform.localScale;
            TweenToScale = InitScale * 1.1f;
        }

        /// <summary>
        /// 设置是否禁用
        /// </summary>
        /// <param name="interactable"></param>
        public virtual void SetInteractable(bool interactable)
        {
            this._Interactable = interactable;
            StateDisplay();
        }
        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (!_Interactable) return;
            this.PointerEventData = eventData;

            if (eventData.dragging) return;

            if (_Music) _CustomAudioSource.PlayOneShot(_CustomAudioClip);


            OnClick?.Invoke();
            Shortcuts.OnClick?.Invoke();
        }
        public virtual void OnPointerDown(PointerEventData eventData)
        {
            if (!_Interactable) return;
            this.PointerEventData = eventData;
            OnClickDown?.Invoke();
            Shortcuts.OnClickDown?.Invoke();

            if (_ClickColor)
            {
                ColorTweener?.Kill();
                ColorTweener = Graphic.Color(_Color, 0.15f);
            }
            if (_Effect)
            {
                ScaleTweener?.Kill();
                ScaleTweener = ZoomTargetTrans.DOScale(TweenToScale, 0.15f).SetUpdate(true);
            }
        }
        public virtual void OnPointerUp(PointerEventData eventData)
        {
            if (!_Interactable) return;
            this.PointerEventData = eventData;
            OnClickUp?.Invoke();
            Shortcuts.OnClickUp?.Invoke();

            if (_ClickColor)
            {
                ColorTweener?.Kill();
                ColorTweener = Graphic.Color(StartColor, 0.15f);
            }
            if (_Effect)
            {
                ScaleTweener?.Kill();
                ScaleTweener = ZoomTargetTrans.DOScale(InitScale, 0.15f).SetUpdate(true);
            }
        }

        public virtual void SetOnClick(Action callback)
        {
            OnClick = callback;
        }


        public void SetOnDown(Action callback)
        {
            OnClickDown = callback;
        }

        public void SetOnUp(Action callback)
        {
            OnClickUp = callback;
        }
    }

    public interface IUIClickable
    {
        /// <summary>
        /// 设置点击事件
        /// </summary>
        /// <param name="callback"></param>
        void SetOnClick(Action callback);
        /// <summary>
        /// 设置按下事件
        /// </summary>
        /// <param name="callback"></param>
        void SetOnDown(Action callback);
        /// <summary>
        /// 设置抬起事件
        /// </summary>
        /// <param name="callback"></param>
        void SetOnUp(Action callback);
    }
}