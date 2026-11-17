using System;

namespace VehicleFollow.Core
{
    /// <summary>
    /// Diagnostic metrics computed alongside a <see cref="DriveCommand"/>.
    /// </summary>
    public readonly struct FollowStepMetrics
    {
        public float Distance { get; }
        public float LateralOffset { get; }
        public float ForwardOffset { get; }
        public float DesiredSpeed { get; }
        public float ClosingSpeed { get; }

        public FollowStepMetrics(
            float distance,
            float lateralOffset,
            float forwardOffset,
            float desiredSpeed,
            float closingSpeed)
        {
            Distance = distance;
            LateralOffset = lateralOffset;
            ForwardOffset = forwardOffset;
            DesiredSpeed = desiredSpeed;
            ClosingSpeed = closingSpeed;
        }
    }

    /// <summary>
    /// Pure C# steering and longitudinal speed controller using proportional steering
    /// and distance + closing-speed longitudinal regulation.
    /// </summary>
    public static class FollowSteeringCalculator
    {
        public static DriveCommand Calculate(
            FollowPose followerPose,
            float followerSpeed,
            FollowPose targetPose,
            FollowVector3 targetVelocity,
            FollowSettings settings,
            out FollowStepMetrics metrics)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            FollowVector3 delta = targetPose.Position - followerPose.Position;
            float distance = delta.Length;
            FollowVector3 localTarget = followerPose.WorldToLocalPoint(targetPose.Position);

            float horizontalDist = (float)Math.Sqrt(localTarget.X * localTarget.X + localTarget.Z * localTarget.Z);
            float steerNormalized = 0f;

            if (distance > settings.StopDistance && horizontalDist > 1e-4f)
            {
                float rawSteer = (localTarget.X / horizontalDist) * settings.SteeringGain;
                steerNormalized = Math.Clamp(rawSteer, -1f, 1f);
            }

            // Project target velocity along the follower's forward axis
            float targetForwardSpeed = Math.Max(0f, FollowVector3.Dot(targetVelocity, followerPose.Forward));

            // Closing speed: positive when follower is closing the gap faster than target moves away
            FollowVector3 dirToTarget = distance > 1e-4f ? delta * (1f / distance) : followerPose.Forward;
            FollowVector3 followerVelocity = followerPose.Forward * followerSpeed;
            float closingSpeed = FollowVector3.Dot(followerVelocity - targetVelocity, dirToTarget);

            // Distance-based desired speed ramp
            float desiredSpeed = 0f;
            if (distance > settings.StopDistance)
            {
                float effectiveSlowdown = Math.Max(settings.SlowdownDistance, settings.StopDistance + 0.1f);
                float t = Math.Clamp((distance - settings.StopDistance) / (effectiveSlowdown - settings.StopDistance), 0f, 1f);
                desiredSpeed = targetForwardSpeed + t * settings.MaxApproachSpeed;
            }
            else
            {
                // Within stop distance: match target speed if moving, otherwise stop
                desiredSpeed = targetForwardSpeed;
            }

            float speedError = desiredSpeed - followerSpeed;
            float throttle = 0f;
            float brake = 0f;

            if (distance <= settings.StopDistance && targetForwardSpeed < 0.2f)
            {
                brake = 1f;
            }
            else if (speedError > settings.SpeedDeadband)
            {
                throttle = Math.Clamp(speedError * settings.SpeedMatchGain, 0f, 1f);
            }
            else if (speedError < -settings.SpeedDeadband || (closingSpeed > 1f && distance < settings.SlowdownDistance))
            {
                float overspeedBrake = Math.Max(0f, -speedError) * settings.SpeedMatchGain;
                float closingBrake = Math.Max(0f, closingSpeed) * settings.ClosingSpeedBrakeGain;
                brake = Math.Clamp(Math.Max(overspeedBrake, closingBrake), 0f, 1f);
            }

            metrics = new FollowStepMetrics(
                distance,
                localTarget.X,
                localTarget.Z,
                desiredSpeed,
                closingSpeed);

            return new DriveCommand(steerNormalized, throttle, brake);
        }
    }
}
