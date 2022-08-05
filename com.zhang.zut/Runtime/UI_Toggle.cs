using MyTool.UnityComponents;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace MyTool.UnityComponents
{
    public class UI_Toggle : UI_Selectable, IPointerClickHandler
    {
        [SerializeField] GameObject m_Sign;
        [SerializeField] UI_ToggleGroup m_Group;
        [SerializeField] bool m_isOn = false;
        [SerializeField] bool m_Reversal = false;
        [SerializeField] ShortcutsCellbackBoolean m_Shortcuts;
        Action<bool> onClick;
        public bool IsOn { get { return trueIsOn; } }
        public ShortcutsCellbackBoolean Shortcuts { get => m_Shortcuts; }
        public bool Reversal
        {
            get { return m_Reversal; }
            set
            {
                m_Reversal = value;
                OnSetSign();
            }
        }
        public GameObject Sign { get => m_Sign; set { m_Sign = value; OnSetSign(); } }
        public Action<bool> OnClick { get => onClick; set => onClick = value; }
        public UI_ToggleGroup Group { get => m_Group; set => m_Group = value; }

        bool trueIsOn;
        bool init = true;
        protected override void Reset()
        {
            init = false;
        }
        protected override void OnDisable()
        {
            if (m_Group) m_Group.Remove(this);
        }

        protected override void OnEnable()
        {
            if (m_Group) m_Group.Add(this);
            OnSetSign();
            base.OnEnable();
        }
        protected override void OnValidate()
        {
            if (init)
            {
                init = false;
                m_isOn = false;
                Set(false);
                if (m_Group) m_Group.Add(this);
                return;
            }

            if (m_Group)
            {
                m_Group.Add(this);
                m_Group.SetToggleOn(this, m_isOn);
            }
            else
            {
                Set(m_isOn);
            }
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);
            if (m_Group != null) m_Group.SetToggleOn(this, !IsOn); else SetIsOn(!IsOn);
        }
        public void SetOnClick(Action<bool> b)
        {
            onClick = b;
        }
        private void Set(bool value)
        {
            if (m_Group != null && m_Group.isActiveAndEnabled && IsActive())
            {
                m_Group.SetToggleOn(this, value);
            }
            else
            {
                SetIsOn(value);
            }
        }

        private void OnSetSign()
        {
            if (m_Sign != null)
            {
                if (m_Reversal)
                {
                    m_Sign.SetActive(!trueIsOn);
                }
                else
                {
                    m_Sign.SetActive(trueIsOn);
                }
            }
        }

        public void SetIsOn(bool v)
        {
            trueIsOn = v;

            m_Shortcuts?.Invoke(trueIsOn);

            onClick?.Invoke(trueIsOn);

            OnSetSign();
        }

        public void SetShowIsOn(bool v)
        {
            m_isOn = v;
        }
    }
}