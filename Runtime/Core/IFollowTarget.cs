namespace VehicleFollow.Core
{
    /// <summary>
    /// Supplies the target anchor pose, velocity, and operational flags of the vehicle to follow.
    /// </summary>
    public interface IFollowTarget
    {
        /// <summary>World-space pose of the lateral follow anchor (e.g., under an unloading auger).</summary>
        FollowPose AnchorPose { get; }

        /// <summary>World-space velocity of the target in meters per second.</summary>
        FollowVector3 Velocity { get; }

        /// <summary>True when the leader is requesting a follower to approach and align.</summary>
        bool IsCallActive { get; }

        /// <summary>True while active unloading/transfer is in progress.</summary>
        bool IsUnloading { get; }

        /// <summary>Optional world-space pose to drive toward when departing.</summary>
        FollowPose? DepartPose { get; }
    }
}
