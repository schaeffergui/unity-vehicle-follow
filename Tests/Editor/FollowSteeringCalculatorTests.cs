using System;
using NUnit.Framework;
using VehicleFollow.Core;

namespace VehicleFollow.Tests
{
    public class FollowSteeringCalculatorTests
    {
        private FollowSettings _settings;
        private FollowPose _originPose;

        [SetUp]
        public void SetUp()
        {
            _settings = new FollowSettings
            {
                StopDistance = 1.5f,
                SlowdownDistance = 8.0f,
                MaxApproachSpeed = 10.0f,
                SteeringGain = 1.2f,
                SpeedMatchGain = 0.45f,
                ClosingSpeedBrakeGain = 0.35f,
                SpeedDeadband = 0.25f
            };
            _originPose = new FollowPose(FollowVector3.Zero, FollowVector3.Forward, FollowVector3.Up);
        }

        [Test]
        public void TargetStraightAhead_ProducesZeroSteerAndPositiveThrottle()
        {
            var target = new FollowPose(new FollowVector3(0f, 0f, 12f), FollowVector3.Forward, FollowVector3.Up);

            DriveCommand cmd = FollowSteeringCalculator.Calculate(
                _originPose, 0f, target, FollowVector3.Zero, _settings, out FollowStepMetrics metrics);

            Assert.That(cmd.SteerNormalized, Is.EqualTo(0f).Within(1e-4f));
            Assert.That(cmd.Throttle, Is.GreaterThan(0.5f));
            Assert.That(cmd.Brake, Is.EqualTo(0f));
            Assert.That(metrics.Distance, Is.EqualTo(12f).Within(1e-4f));
        }

        [Test]
        public void TargetToRight_ProducesPositiveSteer()
        {
            var target = new FollowPose(new FollowVector3(4f, 0f, 8f), FollowVector3.Forward, FollowVector3.Up);

            DriveCommand cmd = FollowSteeringCalculator.Calculate(
                _originPose, 2f, target, FollowVector3.Zero, _settings, out _);

            Assert.That(cmd.SteerNormalized, Is.GreaterThan(0f));
            Assert.That(cmd.SteerNormalized, Is.LessThanOrEqualTo(1f));
        }

        [Test]
        public void TargetToLeft_ProducesNegativeSteerAndClamps()
        {
            var target = new FollowPose(new FollowVector3(-10f, 0f, 1f), FollowVector3.Forward, FollowVector3.Up);

            DriveCommand cmd = FollowSteeringCalculator.Calculate(
                _originPose, 2f, target, FollowVector3.Zero, _settings, out _);

            Assert.That(cmd.SteerNormalized, Is.EqualTo(-1f).Within(1e-4f));
        }

        [Test]
        public void WithinStopDistanceWithStationaryTarget_AppliesFullBrakeAndZeroSteer()
        {
            var target = new FollowPose(new FollowVector3(0.3f, 0f, 0.8f), FollowVector3.Forward, FollowVector3.Up);

            DriveCommand cmd = FollowSteeringCalculator.Calculate(
                _originPose, 2f, target, FollowVector3.Zero, _settings, out _);

            Assert.That(cmd.SteerNormalized, Is.EqualTo(0f));
            Assert.That(cmd.Throttle, Is.EqualTo(0f));
            Assert.That(cmd.Brake, Is.EqualTo(1f));
        }

        [Test]
        public void ZeroDistance_DoesNotThrowOrProduceNaN()
        {
            DriveCommand cmd = FollowSteeringCalculator.Calculate(
                _originPose, 0f, _originPose, FollowVector3.Zero, _settings, out FollowStepMetrics metrics);

            Assert.That(float.IsNaN(cmd.SteerNormalized), Is.False);
            Assert.That(float.IsNaN(cmd.Throttle), Is.False);
            Assert.That(float.IsNaN(cmd.Brake), Is.False);
            Assert.That(metrics.Distance, Is.EqualTo(0f));
        }

        [Test]
        public void HighClosingSpeedNearTarget_AppliesBrake()
        {
            var target = new FollowPose(new FollowVector3(0f, 0f, 3f), FollowVector3.Forward, FollowVector3.Up);

            DriveCommand cmd = FollowSteeringCalculator.Calculate(
                _originPose, 9f, target, FollowVector3.Zero, _settings, out FollowStepMetrics metrics);

            Assert.That(metrics.ClosingSpeed, Is.GreaterThan(8f));
            Assert.That(cmd.Throttle, Is.EqualTo(0f));
            Assert.That(cmd.Brake, Is.GreaterThan(0.5f));
        }

        [Test]
        public void MovingTargetWithinStopDistance_MatchesTargetSpeedWithoutFullStop()
        {
            var target = new FollowPose(new FollowVector3(0f, 0f, 1.0f), FollowVector3.Forward, FollowVector3.Up);
            var targetVel = new FollowVector3(0f, 0f, 4f);

            DriveCommand cmd = FollowSteeringCalculator.Calculate(
                _originPose, 1.0f, target, targetVel, _settings, out FollowStepMetrics metrics);

            Assert.That(metrics.DesiredSpeed, Is.EqualTo(4f).Within(1e-4f));
            Assert.That(cmd.Throttle, Is.GreaterThan(0f));
            Assert.That(cmd.Brake, Is.EqualTo(0f));
        }

        [Test]
        public void NullSettings_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                FollowSteeringCalculator.Calculate(
                    _originPose, 0f, _originPose, FollowVector3.Zero, null, out _));
        }
    }
}
