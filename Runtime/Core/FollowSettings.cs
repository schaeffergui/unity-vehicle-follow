using System;

namespace VehicleFollow.Core
{
    /// <summary>
    /// Tunable thresholds, gains, and hysteresis parameters for following and state transitions.
    /// </summary>
    [Serializable]
    public sealed class FollowSettings
    {
        // Steering & speed control
        public float StopDistance = 1.5f;
        public float SlowdownDistance = 8.0f;
        public float MaxApproachSpeed = 10.0f;
        public float SteeringGain = 1.2f;
        public float SpeedMatchGain = 0.45f;
        public float ClosingSpeedBrakeGain = 0.35f;
        public float SpeedDeadband = 0.25f;

        // State machine thresholds with hysteresis
        public float AlignEnterDistance = 6.0f;
        public float AlignExitDistance = 8.0f;
        public float FollowEnterDistance = 2.2f;
        public float FollowExitDistance = 3.5f;
        public float LateralAlignThreshold = 0.85f;
        public float DepartArrivalDistance = 2.5f;
    }
}
