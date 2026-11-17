using System;
using UnityEngine;
using UnityEngine.Events;
using VehicleFollow.Core;

namespace VehicleFollow.Unity
{
    /// <summary>
    /// MonoBehaviour orchestrator connecting an <see cref="IFollowTarget"/> and <see cref="IVehicleDrive"/>
    /// to <see cref="FollowStateMachine"/> and <see cref="FollowSteeringCalculator"/>.
    /// </summary>
    public class VehicleFollower : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MonoBehaviour driveBehaviour;
        [SerializeField] private MonoBehaviour targetBehaviour;

        [Header("Settings")]
        [SerializeField] private FollowSettings settings = new FollowSettings();

        [Header("Lifecycle Events")]
        [SerializeField] private UnityEvent onAligned;
        [SerializeField] private UnityEvent onFollowStarted;
        [SerializeField] private UnityEvent onFollowFinished;
        [SerializeField] private UnityEvent onDeparted;

        private readonly FollowStateMachine _stateMachine = new FollowStateMachine();
        private IVehicleDrive _drive;
        private IFollowTarget _target;

        public FollowState CurrentState => _stateMachine.CurrentState;
        public FollowStepMetrics LastMetrics { get; private set; }
        public DriveCommand LastCommand { get; private set; }
        public FollowSettings Settings => settings;

        public event Action<FollowState, FollowState> OnStateChanged;
        public event Action OnAligned;
        public event Action OnFollowStarted;
        public event Action OnFollowFinished;
        public event Action OnDeparted;

        private void Awake()
        {
            _drive = driveBehaviour as IVehicleDrive ?? GetComponent<IVehicleDrive>();
            _target = targetBehaviour as IFollowTarget;

            _stateMachine.OnStateChanged += (prev, next) => OnStateChanged?.Invoke(prev, next);
            _stateMachine.OnAligned += () =>
            {
                OnAligned?.Invoke();
                onAligned?.Invoke();
            };
            _stateMachine.OnFollowStarted += () =>
            {
                OnFollowStarted?.Invoke();
                onFollowStarted?.Invoke();
            };
            _stateMachine.OnFollowFinished += () =>
            {
                OnFollowFinished?.Invoke();
                onFollowFinished?.Invoke();
            };
            _stateMachine.OnDeparted += () =>
            {
                OnDeparted?.Invoke();
                onDeparted?.Invoke();
            };
        }

        public void SetDrive(IVehicleDrive drive) => _drive = drive;
        public void SetTarget(IFollowTarget target) => _target = target;

        private void FixedUpdate()
        {
            if (_drive == null || _target == null)
            {
                return;
            }

            FollowPose followerPose = transform.ToFollowPose();
            FollowPose anchorPose = _target.AnchorPose;
            FollowVector3 localAnchor = followerPose.WorldToLocalPoint(anchorPose.Position);
            float distToAnchor = FollowVector3.Distance(followerPose.Position, anchorPose.Position);

            FollowPose? departPose = _target.DepartPose;
            bool hasDepart = departPose.HasValue;
            float distToDepart = hasDepart
                ? FollowVector3.Distance(followerPose.Position, departPose.Value.Position)
                : float.MaxValue;

            var input = new FollowStateInput(
                _target.IsCallActive,
                _target.IsUnloading,
                distToAnchor,
                localAnchor.X,
                hasDepart,
                distToDepart);

            FollowState state = _stateMachine.Step(input, settings);

            DriveCommand command;
            if (state == FollowState.Idle)
            {
                command = DriveCommand.Stop;
                LastMetrics = default;
            }
            else if (state == FollowState.Depart && hasDepart)
            {
                command = FollowSteeringCalculator.Calculate(
                    followerPose,
                    _drive.CurrentSpeed,
                    departPose.Value,
                    FollowVector3.Zero,
                    settings,
                    out FollowStepMetrics departMetrics);
                LastMetrics = departMetrics;
            }
            else
            {
                command = FollowSteeringCalculator.Calculate(
                    followerPose,
                    _drive.CurrentSpeed,
                    anchorPose,
                    _target.Velocity,
                    settings,
                    out FollowStepMetrics followMetrics);
                LastMetrics = followMetrics;
            }

            LastCommand = command;
            _drive.ApplyDrive(command);
        }
    }
}
