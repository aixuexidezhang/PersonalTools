using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;




namespace MyTool.UnityComponents
{
    public class UI_ToggleGroup : UIBehaviour
    {
        [SerializeField] private bool m_AllowSwitchOff = false;
        public bool allowSwitchOff { get { return m_AllowSwitchOff; } set { m_AllowSwitchOff = value; } }
        protected List<UI_Toggle> m_Toggles = new List<UI_Toggle>();
        bool o_AllowSwitchOff;
        public bool AnyTogglesOn
        {
            get
            {
                return m_Toggles.Find(FindOn) != null;
            }
        }
        private bool FindOn(UI_Toggle x)
        {
            return ((x != null) && (x.isActiveAndEnabled) && (x.Group != null) && (x.Group == this) && (x.IsOn));
        }

        protected override void Awake()
        {
            Clear();
        }
        protected override void OnValidate()
        {
            if (o_AllowSwitchOff == m_AllowSwitchOff) return;
            o_AllowSwitchOff = m_AllowSwitchOff;
            Legality();
            base.OnValidate();
        }

        private void Legality()
        {
            Clear();
            Debug.LogError("Legality");
            if (m_Toggles.Count > 0)
            {
                var count = TogglesOnCount();
                var ao = AnyTogglesOn;
                UI_Toggle t = null;
                for (int i = 0; i < m_Toggles.Count; i++)
                {
                    if (m_Toggles[i].isActiveAndEnabled)
                    {
                        t = m_Toggles[i];
                        break;
                    }
                }

                if (m_AllowSwitchOff)
                {
                    if (ao)
                    {
                        if (count > 1)
                        {
                            SetToggleOn(t, true);
                        }
                    }
                }
                else
                {
                    if (!ao || count > 1)
                    {
                        SetToggleOn(t, true);
                    }
                }
            }
        }

        public void SetToggleOn(UI_Toggle toggle, bool on)
        {
            if (toggle != null && toggle.Group != null && toggle.Group == this)
            {
                if (m_Toggles.Contains(toggle))
                {
                    if (on)
                    {
                        toggle.SetIsOn(true);
                        toggle.SetShowIsOn(true);
                        foreach (var item in m_Toggles)
                        {
                            if (item != null && item.isActiveAndEnabled && item != toggle)
                            {
                                item.SetIsOn(false);
                                item.SetShowIsOn(false);
                            }
                        }
                    }
                    else
                    {
                        if (m_AllowSwitchOff)
                        {
                            toggle.SetIsOn(false);
                            toggle.SetShowIsOn(false);
                        }
                        else
                        {
                            var ao = AnyTogglesOn;
                            toggle.SetIsOn(!ao);
                            toggle.SetShowIsOn(!ao);
                        }
                    }
                }
                else
                {
                    Add(toggle);
                    SetToggleOn(toggle, on);
                }
            }
        }

        private void Clear()
        {
            Debug.LogError("Clear");
            List<UI_Toggle> none = new List<UI_Toggle>();
            foreach (var item in m_Toggles)
            {
                if (item != null && item.isActiveAndEnabled && item.Group != null && item.Group == this) continue;
                if (!none.Contains(item)) none.Add(item);
            }
            foreach (var item in none)
            {
                m_Toggles.Remove(item);
            }
        }

        public void Remove(UI_Toggle toggle)
        {
            if (m_Toggles.Count > 0 && m_Toggles.Contains(toggle))
            {
                Debug.LogError("Remove");
                m_Toggles.Remove(toggle);
                Clear();
            }
        }
        public void Add(UI_Toggle toggle)
        {
            if (!m_Toggles.Contains(toggle))
            {
                Debug.LogError("Add");
                m_Toggles.Add(toggle);
                if (!Application.isPlaying)
                {
                    Legality();
                }
            }
        }
        public int TogglesOnCount()
        {
            var a = m_Toggles.FindAll(FindOn);
            if (a != null) return a.Count; else return 0;
        }

    }
}