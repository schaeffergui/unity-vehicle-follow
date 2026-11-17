using UnityEngine;
using VehicleFollow.Unity;

namespace VehicleFollow.Samples
{
    /// <summary>
    /// Prefab-free demo bootstrapper that creates a ground plane, a moving leader with a lateral
    /// follow anchor and unload zone trigger, and a 4-wheel WheelCollider follower vehicle.
    /// </summary>
    public class WheelColliderFollowDemoSetup : MonoBehaviour
    {
        [SerializeField] private float leaderForwardSpeed = 3.0f;
        [SerializeField] private float lateralOffset = 4.0f;
        [SerializeField] private Vector3 followerStartOffset = new Vector3(-12f, 0.5f, -15f);

        private Transform _leaderTransform;
        private FollowTargetAnchor _targetAnchor;
        private VehicleFollower _follower;

        private void Start()
        {
            BuildGround();
            BuildLeaderAndAnchor();
            BuildFollower();
        }

        private void FixedUpdate()
        {
            if (_leaderTransform != null)
            {
                _leaderTransform.Translate(Vector3.forward * (leaderForwardSpeed * Time.fixedDeltaTime), Space.World);
            }
        }

        private static void BuildGround()
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "DemoGround";
            ground.transform.localScale = new Vector3(20f, 1f, 20f);
        }

        private void BuildLeaderAndAnchor()
        {
            GameObject leader = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leader.name = "LeaderVehicle";
            leader.transform.position = new Vector3(0f, 1f, 0f);
            leader.transform.localScale = new Vector3(3f, 2f, 6f);
            _leaderTransform = leader.transform;

            GameObject anchorObj = new GameObject("LateralFollowAnchor");
            anchorObj.transform.SetParent(leader.transform, false);
            anchorObj.transform.localPosition = new Vector3(-lateralOffset, -0.5f, 0f);

            GameObject departObj = new GameObject("DepartAnchor");
            departObj.transform.position = new Vector3(-25f, 0.5f, 40f);

            GameObject zoneObj = new GameObject("UnloadZone");
            zoneObj.transform.SetParent(anchorObj.transform, false);
            BoxCollider zoneCollider = zoneObj.AddComponent<BoxCollider>();
            zoneCollider.isTrigger = true;
            zoneCollider.size = new Vector3(2.5f, 2f, 4f);
            UnloadZoneTrigger zoneTrigger = zoneObj.AddComponent<UnloadZoneTrigger>();
            zoneTrigger.ZoneEntered += _ => _targetAnchor.IsUnloading = true;
            zoneTrigger.ZoneExited += _ => _targetAnchor.IsUnloading = false;

            _targetAnchor = leader.AddComponent<FollowTargetAnchor>();
            _targetAnchor.SetAnchor(anchorObj.transform);
            _targetAnchor.SetDepartAnchor(departObj.transform);
            _targetAnchor.IsCallActive = true;
        }

        private void BuildFollower()
        {
            GameObject followerObj = new GameObject("FollowerVehicle");
            followerObj.transform.position = followerStartOffset;

            Rigidbody rb = followerObj.AddComponent<Rigidbody>();
            rb.mass = 1800f;

            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Chassis";
            body.transform.SetParent(followerObj.transform, false);
            body.transform.localScale = new Vector3(2.2f, 1.2f, 4.5f);
            body.transform.localPosition = new Vector3(0f, 0.6f, 0f);

            WheelCollider fl = CreateWheel(followerObj.transform, "WheelFL", new Vector3(-1f, 0.4f, 1.5f), out Transform flMesh);
            WheelCollider fr = CreateWheel(followerObj.transform, "WheelFR", new Vector3(1f, 0.4f, 1.5f), out Transform frMesh);
            WheelCollider rl = CreateWheel(followerObj.transform, "WheelRL", new Vector3(-1f, 0.4f, -1.5f), out Transform rlMesh);
            WheelCollider rr = CreateWheel(followerObj.transform, "WheelRR", new Vector3(1f, 0.4f, -1.5f), out Transform rrMesh);

            WheelColliderVehicleDrive drive = followerObj.AddComponent<WheelColliderVehicleDrive>();
            drive.ConfigureWheels(
                new[] { fl, fr },
                new[] { rl, rr },
                null,
                new[] { flMesh, frMesh },
                new[] { rlMesh, rrMesh });

            _follower = followerObj.AddComponent<VehicleFollower>();
            _follower.SetDrive(drive);
            _follower.SetTarget(_targetAnchor);
        }

        private static WheelCollider CreateWheel(Transform parent, string name, Vector3 localPos, out Transform visualMesh)
        {
            GameObject wheelObj = new GameObject(name);
            wheelObj.transform.SetParent(parent, false);
            wheelObj.transform.localPosition = localPos;

            WheelCollider wc = wheelObj.AddComponent<WheelCollider>();
            wc.radius = 0.4f;
            wc.suspensionDistance = 0.2f;

            GameObject mesh = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            mesh.name = name + "_Visual";
            Destroy(mesh.GetComponent<Collider>());
            mesh.transform.SetParent(wheelObj.transform, false);
            mesh.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            mesh.transform.localScale = new Vector3(0.8f, 0.15f, 0.8f);
            visualMesh = mesh.transform;

            return wc;
        }
    }
}
