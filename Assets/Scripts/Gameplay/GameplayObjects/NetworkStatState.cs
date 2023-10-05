using System;
using Unity.Netcode;
using UnityEngine;

namespace Unity.BossRoom.Gameplay.GameplayObjects
{
    /// <summary>
    /// MonoBehaviour containing only one NetworkVariable which represents this object's generic type stat.
    /// </summary>
    public abstract class NetworkStatState<T> : NetworkBehaviour where T: IComparable
    {
        [HideInInspector]
        public NetworkVariable<T> Stat = new NetworkVariable<T>();

        // public subscribable event to be invoked when stat has been fully depleted
        public event Action StatDepleted;

        // public subscribable event to be invoked when stat has been replenished
        public event Action StatReplenished;

        void OnEnable()
        {
            Stat.OnValueChanged += StatChanged;
        }

        void OnDisable()
        {
            Stat.OnValueChanged -= StatChanged;
        }

        void StatChanged(T previousValue, T newValue)
        {
            if (previousValue.CompareTo(default(T)) > 0 && newValue.CompareTo(default(T)) <= 0)
            {
                // newly reached 0 stat
                StatDepleted?.Invoke();
            }
            else if (previousValue.CompareTo(default(T)) <= 0 && newValue.CompareTo(default(T)) > 0)
            {
                // newly replenished
                StatReplenished?.Invoke();
            }
        }
    }
}
