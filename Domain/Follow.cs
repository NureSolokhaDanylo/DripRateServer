namespace Domain;

public sealed class Follow
{
    public Guid FollowerId { get; private set; }
    public User Follower { get; private set; } = null!;

    public Guid FolloweeId { get; private set; }
    public User Followee { get; private set; } = null!;

    private Follow() { }

    public Follow(Guid followerId, Guid followeeId)
    {
        FollowerId = followerId;
        FolloweeId = followeeId;
    }
}
