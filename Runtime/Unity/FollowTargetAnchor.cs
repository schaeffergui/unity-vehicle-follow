using UnityEngine;
using VehicleFollow.Core;

namespace VehicleFollow.Unity
{
    /// <summary>
    /// Scene-object implementation of <see cref="IFollowTarget"/> tracking a leader's lateral anchor.
    /// </summary>
    public class FollowTargetAnchor : MonoBehaviour, IFollowTarget
    {
        [SerializeField] private Transform anchorTransform;
        [SerializeField] private Transform departTransform;
        [SerializeField] private Rigidbody targetRigidbody;
        [SerializeField] private bool isCallActive;
        [SerializeField] private bool isUnloading;

        private Vector3 _previousPosition;
        private Vector3 _estimatedVelocity;
        private bool _hasSample;

        public FollowPose AnchorPose => (anchorTransform != null ? anchorTransform : transform).ToFollowPose();

        public FollowVector3 Velocity =>
            targetRigidbody != null
                ? targetRigidbody.linearVelocity.ToFollowVector3()
                : _estimatedVelocity.ToFollowVector3();

        public bool IsCallActive
        {
            get => isCallActive;
            set => isCallActive = value;
        }

        public bool IsUnloading
        {
            get => isUnloading;
            set => isUnloading = value;
        }

        public FollowPose? DepartPose =>
            departTransform != null ? departTransform.ToFollowPose() : (FollowPose?)null;

        public void SetAnchor(Transform anchor) => anchorTransform = anchor;
        public void SetDepartAnchor(Transform depart) => departTransform = depart;

        private void FixedUpdate()
        {
            if (targetRigidbody != null)
            {
                return;
            }

            Transform active = anchorTransform != null ? anchorTransform : transform;
            Vector3 currentPos = active.position;
            if (_hasSample && Time.fixedDeltaTime > 1e-5f)
            {
                _estimatedVelocity = (currentPos - _previousPosition) / Time.fixedDeltaTime;
            }

            _previousPosition = currentPos;
            _hasSample = true;
        }
    }
}
