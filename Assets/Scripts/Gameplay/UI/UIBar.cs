using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.BossRoom.Gameplay.UI
{
    /// <summary>
    /// UI object that visually represents an object's stat. Visuals are updated when NetworkVariable is modified.
    /// </summary>
    public class UIBar : MonoBehaviour
    {
        [SerializeField]
        Slider m_Slider;

        NetworkVariable<int> m_NetworkedStat;

        public void Initialize(NetworkVariable<int> networkedStat, int maxValue)
        {
            m_NetworkedStat = networkedStat;

            m_Slider.minValue = 0;
            m_Slider.maxValue = maxValue;
            StatChanged(maxValue, maxValue);

            m_NetworkedStat.OnValueChanged += StatChanged;
        }

        void StatChanged(int previousValue, int newValue)
        {
            m_Slider.value = newValue;
            // disable slider when we're at full health!
            m_Slider.gameObject.SetActive(m_Slider.value != m_Slider.maxValue);
        }

        void OnDestroy()
        {
            m_NetworkedStat.OnValueChanged -= StatChanged;
        }
    }
}
