using System;
using System.Collections.Generic;
using NUnit.Framework;
using VehicleFollow.Core;

namespace VehicleFollow.Tests
{
    public class FollowStateMachineTests
    {
        private FollowSettings _settings;
        private FollowStateMachine _sm;

        [SetUp]
        public void SetUp()
        {
            _settings = new FollowSettings
            {
                AlignEnterDistance = 6.0f,
                AlignExitDistance = 8.0f,
                FollowEnterDistance = 2.2f,
                FollowExitDistance = 3.5f,
                LateralAlignThreshold = 0.85f,
                DepartArrivalDistance = 2.5f
            };
            _sm = new FollowStateMachine();
        }

        [Test]
        public void FullLifecycle_IdleToApproachToAlignToFollowToDepartToIdle_FiresEventsOnce()
        {
            var transitions = new List<(FollowState prev, FollowState next)>();
            int alignedCount = 0;
            int followStartedCount = 0;
            int followFinishedCount = 0;
            int departedCount = 0;

            _sm.OnStateChanged += (p, n) => transitions.Add((p, n));
            _sm.OnAligned += () => alignedCount++;
            _sm.OnFollowStarted += () => followStartedCount++;
            _sm.OnFollowFinished += () => followFinishedCount++;
            _sm.OnDeparted += () => departedCount++;

            Assert.That(_sm.CurrentState, Is.EqualTo(FollowState.Idle));

            // Call activates -> Approach
            Assert.That(_sm.Step(new FollowStateInput(true, false, 20f, 0f, true, 30f), _settings), Is.EqualTo(FollowState.Approach));

            // Enters align radius -> Align
            Assert.That(_sm.Step(new FollowStateInput(true, false, 5.5f, 1.5f, true, 30f), _settings), Is.EqualTo(FollowState.Align));

            // Within follow distance and lateral threshold -> Follow
            Assert.That(_sm.Step(new FollowStateInput(true, false, 2.0f, 0.3f, true, 30f), _settings), Is.EqualTo(FollowState.Follow));
            // Repeated follow ticks do not re-fire events
            Assert.That(_sm.Step(new FollowStateInput(true, true, 1.8f, 0.2f, true, 30f), _settings), Is.EqualTo(FollowState.Follow));

            // Call and unloading end with depart pose -> Depart
            Assert.That(_sm.Step(new FollowStateInput(false, false, 2.0f, 0.2f, true, 25f), _settings), Is.EqualTo(FollowState.Depart));

            // Reaches depart pose -> Idle
            Assert.That(_sm.Step(new FollowStateInput(false, false, 20f, 0f, true, 2.0f), _settings), Is.EqualTo(FollowState.Idle));

            Assert.That(alignedCount, Is.EqualTo(1));
            Assert.That(followStartedCount, Is.EqualTo(1));
            Assert.That(followFinishedCount, Is.EqualTo(1));
            Assert.That(departedCount, Is.EqualTo(1));
            Assert.That(transitions, Is.EqualTo(new[]
            {
                (FollowState.Idle, FollowState.Approach),
                (FollowState.Approach, FollowState.Align),
                (FollowState.Align, FollowState.Follow),
                (FollowState.Follow, FollowState.Depart),
                (FollowState.Depart, FollowState.Idle)
            }));
        }

        [Test]
        public void Hysteresis_PreventsChatterBetweenAlignAndApproach()
        {
            _sm.Step(new FollowStateInput(true, false, 15f, 0f), _settings);
            Assert.That(_sm.Step(new FollowStateInput(true, false, 5.9f, 2.0f), _settings), Is.EqualTo(FollowState.Align));

            // Distance drifts to 7.0 (between AlignEnter=6.0 and AlignExit=8.0) -> stays Align
            Assert.That(_sm.Step(new FollowStateInput(true, false, 7.0f, 2.0f), _settings), Is.EqualTo(FollowState.Align));

            // Exceeds AlignExit=8.0 -> drops back to Approach
            Assert.That(_sm.Step(new FollowStateInput(true, false, 8.5f, 2.0f), _settings), Is.EqualTo(FollowState.Approach));
        }

        [Test]
        public void Hysteresis_PreventsChatterBetweenFollowAndAlign()
        {
            _sm.Step(new FollowStateInput(true, false, 15f, 0f), _settings);
            _sm.Step(new FollowStateInput(true, false, 5.0f, 0.2f), _settings);
            Assert.That(_sm.Step(new FollowStateInput(true, false, 2.0f, 0.2f), _settings), Is.EqualTo(FollowState.Follow));

            // Distance drifts to 3.0 (between FollowEnter=2.2 and FollowExit=3.5) -> stays Follow
            Assert.That(_sm.Step(new FollowStateInput(true, false, 3.0f, 0.2f), _settings), Is.EqualTo(FollowState.Follow));

            // Exceeds FollowExit=3.5 while not unloading -> returns to Align
            Assert.That(_sm.Step(new FollowStateInput(true, false, 3.8f, 0.2f), _settings), Is.EqualTo(FollowState.Align));
        }

        [Test]
        public void UnloadingFlag_KeepsStateInFollowEvenBeyondExitDistance()
        {
            _sm.Step(new FollowStateInput(true, true, 1.5f, 0f), _settings); // Approach
            _sm.Step(new FollowStateInput(true, true, 1.5f, 0f), _settings); // Align
            _sm.Step(new FollowStateInput(true, true, 1.5f, 0f), _settings); // Follow

            Assert.That(_sm.Step(new FollowStateInput(true, true, 5.0f, 0f), _settings), Is.EqualTo(FollowState.Follow));
        }

        [Test]
        public void FollowFinishedWithoutDepartTarget_TransitionsDirectlyToIdleAndFiresDeparted()
        {
            int departedCount = 0;
            _sm.OnDeparted += () => departedCount++;

            _sm.Step(new FollowStateInput(true, false, 10f, 0f), _settings);
            _sm.Step(new FollowStateInput(true, false, 5f, 0f), _settings);
            _sm.Step(new FollowStateInput(true, false, 2f, 0f), _settings);

            Assert.That(_sm.Step(new FollowStateInput(false, false, 2f, 0f, hasDepartTarget: false), _settings), Is.EqualTo(FollowState.Idle));
            Assert.That(departedCount, Is.EqualTo(1));
        }

        [Test]
        public void Reset_ReturnsToIdle()
        {
            _sm.Step(new FollowStateInput(true, false, 10f, 0f), _settings);
            Assert.That(_sm.CurrentState, Is.EqualTo(FollowState.Approach));

            _sm.Reset();
            Assert.That(_sm.CurrentState, Is.EqualTo(FollowState.Idle));
        }

        [Test]
        public void NullSettings_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _sm.Step(new FollowStateInput(true, false, 1f, 0f), null));
        }
    }
}
