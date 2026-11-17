using System;
using UnityEngine;
using UnityEngine.Events;

namespace VehicleFollow.Unity
{
    /// <summary>
    /// Optional trigger-zone component for detecting when a follower enters or leaves an unloading area.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class UnloadZoneTrigger : MonoBehaviour
    {
        [SerializeField] private string targetTag = "Vehicle";
        [SerializeField] private bool singleUse;
        [SerializeField] private UnityEvent onZoneEntered;
        [SerializeField] private UnityEvent onZoneExited;

        private bool _hasTriggered;

        public event Action<Collider> ZoneEntered;
        public event Action<Collider> ZoneExited;

        public bool IsOccupied { get; private set; }

        public void ResetTrigger()
        {
            _hasTriggered = false;
            IsOccupied = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (singleUse && _hasTriggered)
            {
                return;
            }

            if (!string.IsNullOrEmpty(targetTag) && !other.CompareTag(targetTag))
            {
                return;
            }

            _hasTriggered = true;
            IsOccupied = true;
            ZoneEntered?.Invoke(other);
            onZoneEntered?.Invoke();
        }

        private void OnTriggerExit(Collider other)
        {
            if (!string.IsNullOrEmpty(targetTag) && !other.CompareTag(targetTag))
            {
                return;
            }

            IsOccupied = false;
            ZoneExited?.Invoke(other);
            onZoneExited?.Invoke();
        }
    }
}
