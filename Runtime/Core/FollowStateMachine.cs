using System;

namespace VehicleFollow.Core
{
    /// <summary>
    /// Snapshot of inputs evaluated each tick by <see cref="FollowStateMachine"/>.
    /// </summary>
    public readonly struct FollowStateInput
    {
        public bool IsCallActive { get; }
        public bool IsUnloading { get; }
        public float DistanceToTarget { get; }
        public float LateralOffset { get; }
        public bool HasDepartTarget { get; }
        public float DistanceToDepart { get; }

        public FollowStateInput(
            bool isCallActive,
            bool isUnloading,
            float distanceToTarget,
            float lateralOffset,
            bool hasDepartTarget = false,
            float distanceToDepart = float.MaxValue)
        {
            IsCallActive = isCallActive;
            IsUnloading = isUnloading;
            DistanceToTarget = distanceToTarget;
            LateralOffset = lateralOffset;
            HasDepartTarget = hasDepartTarget;
            DistanceToDepart = distanceToDepart;
        }
    }

    /// <summary>
    /// State machine governing Idle -> Approach -> Align -> Follow -> Depart transitions with hysteresis.
    /// </summary>
    public sealed class FollowStateMachine
    {
        private bool _hasNotifiedAligned;
        private bool _hasNotifiedFollowStarted;

        public FollowState CurrentState { get; private set; } = FollowState.Idle;

        public event Action<FollowState, FollowState> OnStateChanged;
        public event Action OnAligned;
        public event Action OnFollowStarted;
        public event Action OnFollowFinished;
        public event Action OnDeparted;

        public FollowState Step(FollowStateInput input, FollowSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            switch (CurrentState)
            {
                case FollowState.Idle:
                    if (input.IsCallActive || input.IsUnloading)
                    {
                        TransitionTo(FollowState.Approach);
                    }
                    break;

                case FollowState.Approach:
                    if (!input.IsCallActive && !input.IsUnloading)
                    {
                        TransitionTo(FollowState.Idle);
                    }
                    else if (input.IsUnloading || input.DistanceToTarget <= settings.AlignEnterDistance)
                    {
                        TransitionTo(FollowState.Align);
                    }
                    break;

                case FollowState.Align:
                    if (!input.IsCallActive && !input.IsUnloading)
                    {
                        TransitionTo(input.HasDepartTarget ? FollowState.Depart : FollowState.Idle);
                    }
                    else if (input.IsUnloading ||
                             (input.DistanceToTarget <= settings.FollowEnterDistance &&
                              Math.Abs(input.LateralOffset) <= settings.LateralAlignThreshold))
                    {
                        if (!_hasNotifiedAligned)
                        {
                            _hasNotifiedAligned = true;
                            OnAligned?.Invoke();
                        }

                        TransitionTo(FollowState.Follow);
                        if (!_hasNotifiedFollowStarted)
                        {
                            _hasNotifiedFollowStarted = true;
                            OnFollowStarted?.Invoke();
                        }
                    }
                    else if (input.DistanceToTarget > settings.AlignExitDistance)
                    {
                        TransitionTo(FollowState.Approach);
                    }
                    break;

                case FollowState.Follow:
                    if (!input.IsCallActive && !input.IsUnloading)
                    {
                        OnFollowFinished?.Invoke();
                        TransitionTo(input.HasDepartTarget ? FollowState.Depart : FollowState.Idle);
                        if (CurrentState == FollowState.Idle)
                        {
                            OnDeparted?.Invoke();
                        }
                    }
                    else if (!input.IsUnloading && input.DistanceToTarget > settings.FollowExitDistance)
                    {
                        TransitionTo(FollowState.Align);
                    }
                    break;

                case FollowState.Depart:
                    if (input.IsCallActive || input.IsUnloading)
                    {
                        _hasNotifiedAligned = false;
                        _hasNotifiedFollowStarted = false;
                        TransitionTo(FollowState.Approach);
                    }
                    else if (!input.HasDepartTarget || input.DistanceToDepart <= settings.DepartArrivalDistance)
                    {
                        TransitionTo(FollowState.Idle);
                        OnDeparted?.Invoke();
                    }
                    break;
            }

            return CurrentState;
        }

        public void Reset()
        {
            _hasNotifiedAligned = false;
            _hasNotifiedFollowStarted = false;
            if (CurrentState != FollowState.Idle)
            {
                TransitionTo(FollowState.Idle);
            }
        }

        private void TransitionTo(FollowState next)
        {
            if (next == CurrentState)
            {
                return;
            }

            FollowState previous = CurrentState;
            CurrentState = next;
            if (next == FollowState.Idle)
            {
                _hasNotifiedAligned = false;
                _hasNotifiedFollowStarted = false;
            }

            OnStateChanged?.Invoke(previous, next);
        }
    }
}
