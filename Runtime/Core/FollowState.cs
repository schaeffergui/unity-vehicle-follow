namespace VehicleFollow.Core
{
    /// <summary>
    /// Lifecycle states for a follower vehicle tracking a moving leader.
    /// </summary>
    public enum FollowState
    {
        Idle = 0,
        Approach = 1,
        Align = 2,
        Follow = 3,
        Depart = 4
    }
}
