using System;

namespace VehicleFollow.Core
{
    /// <summary>
    /// Normalized driving command produced by the follow controller.
    /// </summary>
    public readonly struct DriveCommand
    {
        /// <summary>Normalized steering in [-1, 1] (-1 = full left, +1 = full right).</summary>
        public float SteerNormalized { get; }

        /// <summary>Normalized forward throttle in [0, 1].</summary>
        public float Throttle { get; }

        /// <summary>Normalized brake in [0, 1].</summary>
        public float Brake { get; }

        public DriveCommand(float steerNormalized, float throttle, float brake)
        {
            SteerNormalized = Math.Clamp(steerNormalized, -1f, 1f);
            Throttle = Math.Clamp(throttle, 0f, 1f);
            Brake = Math.Clamp(brake, 0f, 1f);
        }

        public static DriveCommand Stop => new DriveCommand(0f, 0f, 1f);
        public static DriveCommand Coast => new DriveCommand(0f, 0f, 0f);
    }

    /// <summary>
    /// Vehicle actuation contract implemented by host physics or WheelCollider adapters.
    /// </summary>
    public interface IVehicleDrive
    {
        /// <summary>Current forward speed in meters per second.</summary>
        float CurrentSpeed { get; }

        /// <summary>Applies steering, throttle, and braking inputs for the current physics step.</summary>
        void ApplyDrive(DriveCommand command);
    }
}
