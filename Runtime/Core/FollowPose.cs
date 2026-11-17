namespace VehicleFollow.Core
{
    /// <summary>
    /// Orthonormal world-space pose used to transform targets into a vehicle's local frame.
    /// </summary>
    public readonly struct FollowPose
    {
        public FollowVector3 Position { get; }
        public FollowVector3 Forward { get; }
        public FollowVector3 Up { get; }
        public FollowVector3 Right { get; }

        public FollowPose(FollowVector3 position, FollowVector3 forward, FollowVector3 up)
        {
            Position = position;
            Forward = forward.LengthSquared > 1e-6f ? forward.Normalized : FollowVector3.Forward;
            Up = up.LengthSquared > 1e-6f ? up.Normalized : FollowVector3.Up;
            Right = FollowVector3.Cross(Up, Forward).Normalized;
            if (Right.LengthSquared < 1e-6f)
            {
                Right = FollowVector3.Right;
            }
        }

        /// <summary>
        /// Converts a world-space point into this pose's local coordinates (X=right, Y=up, Z=forward).
        /// </summary>
        public FollowVector3 WorldToLocalPoint(FollowVector3 worldPoint)
        {
            FollowVector3 delta = worldPoint - Position;
            return new FollowVector3(
                FollowVector3.Dot(delta, Right),
                FollowVector3.Dot(delta, Up),
                FollowVector3.Dot(delta, Forward));
        }

        /// <summary>
        /// Converts a world-space direction/velocity into this pose's local coordinates.
        /// </summary>
        public FollowVector3 WorldToLocalDirection(FollowVector3 worldDirection)
        {
            return new FollowVector3(
                FollowVector3.Dot(worldDirection, Right),
                FollowVector3.Dot(worldDirection, Up),
                FollowVector3.Dot(worldDirection, Forward));
        }
    }
}
