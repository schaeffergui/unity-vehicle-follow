# Changelog

All notable changes to this package will be documented in this file.

## [0.1.0] - 2026-11-13
### Added
- Dependency-free `VehicleFollow.Core` assembly with `FollowVector3`, `FollowPose`, `IVehicleDrive`, `IFollowTarget`, `FollowSteeringCalculator`, `FollowState`, and `FollowStateMachine`.
- `VehicleFollow.Unity` assembly with `VehicleFollower`, `WheelColliderVehicleDrive`, `FollowTargetAnchor`, and `UnloadZoneTrigger`.
- EditMode unit tests in `Tests/Editor` covering steering calculations, speed/closing-speed control, state transitions, hysteresis, and lifecycle events.
- Prefab-free `Samples~/WheelColliderDemo` bootstrapper.
