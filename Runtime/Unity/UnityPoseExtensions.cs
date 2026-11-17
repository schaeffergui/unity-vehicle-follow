using UnityEngine;
using VehicleFollow.Core;

namespace VehicleFollow.Unity
{
    /// <summary>
    /// Conversion helpers between UnityEngine math types and <see cref="VehicleFollow.Core"/> types.
    /// </summary>
    public static class UnityPoseExtensions
    {
        public static FollowVector3 ToFollowVector3(this Vector3 v) =>
            new FollowVector3(v.x, v.y, v.z);

        public static Vector3 ToUnityVector3(this FollowVector3 v) =>
            new Vector3(v.X, v.Y, v.Z);

        public static FollowPose ToFollowPose(this Transform t) =>
            new FollowPose(
                t.position.ToFollowVector3(),
                t.forward.ToFollowVector3(),
                t.up.ToFollowVector3());
    }
}
