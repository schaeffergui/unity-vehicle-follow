using UnityEngine;
using VehicleFollow.Core;

namespace VehicleFollow.Unity
{
    /// <summary>
    /// Standard Unity <see cref="WheelCollider"/> adapter implementing <see cref="IVehicleDrive"/>.
    /// </summary>
    public class WheelColliderVehicleDrive : MonoBehaviour, IVehicleDrive
    {
        [Header("Wheel Colliders")]
        [SerializeField] private WheelCollider[] steerWheels = System.Array.Empty<WheelCollider>();
        [SerializeField] private WheelCollider[] driveWheels = System.Array.Empty<WheelCollider>();
        [SerializeField] private WheelCollider[] trailerWheels = System.Array.Empty<WheelCollider>();

        [Header("Visual Wheel Meshes (Optional)")]
        [SerializeField] private Transform[] steerMeshes = System.Array.Empty<Transform>();
        [SerializeField] private Transform[] driveMeshes = System.Array.Empty<Transform>();
        [SerializeField] private Transform[] trailerMeshes = System.Array.Empty<Transform>();

        [Header("Actuation Limits")]
        [SerializeField] private float maxSteerAngle = 30f;
        [SerializeField] private float maxMotorTorque = 1500f;
        [SerializeField] private float maxBrakeTorque = 3000f;
        [SerializeField] private Rigidbody vehicleRigidbody;

        public float CurrentSpeed
        {
            get
            {
                if (vehicleRigidbody == null)
                {
                    return 0f;
                }

                return Vector3.Dot(vehicleRigidbody.linearVelocity, transform.forward);
            }
        }

        public void ConfigureWheels(
            WheelCollider[] steer,
            WheelCollider[] drive,
            WheelCollider[] trailer = null,
            Transform[] steerVisuals = null,
            Transform[] driveVisuals = null,
            Transform[] trailerVisuals = null)
        {
            steerWheels = steer ?? System.Array.Empty<WheelCollider>();
            driveWheels = drive ?? System.Array.Empty<WheelCollider>();
            trailerWheels = trailer ?? System.Array.Empty<WheelCollider>();
            steerMeshes = steerVisuals ?? System.Array.Empty<Transform>();
            driveMeshes = driveVisuals ?? System.Array.Empty<Transform>();
            trailerMeshes = trailerVisuals ?? System.Array.Empty<Transform>();
        }

        private void Awake()
        {
            if (vehicleRigidbody == null)
            {
                vehicleRigidbody = GetComponent<Rigidbody>();
            }
        }

        public void ApplyDrive(DriveCommand command)
        {
            float steerAngle = command.SteerNormalized * maxSteerAngle;
            float motor = command.Throttle * maxMotorTorque;
            float brake = command.Brake * maxBrakeTorque;

            for (int i = 0; i < steerWheels.Length; i++)
            {
                if (steerWheels[i] == null) continue;
                steerWheels[i].steerAngle = steerAngle;
                steerWheels[i].brakeTorque = brake;
            }

            for (int i = 0; i < driveWheels.Length; i++)
            {
                if (driveWheels[i] == null) continue;
                driveWheels[i].motorTorque = motor;
                driveWheels[i].brakeTorque = brake;
            }

            for (int i = 0; i < trailerWheels.Length; i++)
            {
                if (trailerWheels[i] == null) continue;
                trailerWheels[i].motorTorque = 0f;
                trailerWheels[i].brakeTorque = 0f;
            }

            SyncWheelMeshes(steerWheels, steerMeshes);
            SyncWheelMeshes(driveWheels, driveMeshes);
            SyncWheelMeshes(trailerWheels, trailerMeshes);
        }

        private static void SyncWheelMeshes(WheelCollider[] colliders, Transform[] meshes)
        {
            int count = Mathf.Min(colliders.Length, meshes.Length);
            for (int i = 0; i < count; i++)
            {
                if (colliders[i] == null || meshes[i] == null) continue;
                colliders[i].GetWorldPose(out Vector3 pos, out Quaternion rot);
                meshes[i].SetPositionAndRotation(pos, rot);
            }
        }
    }
}
