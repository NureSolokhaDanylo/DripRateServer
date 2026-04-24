namespace Domain;

public sealed class Follow
{
    private Guid _followerId;
    private User _follower = null!;
    private Guid _followeeId;
    private User _followee = null!;

    public Guid FollowerId => _followerId;
    public User Follower => _follower;

    public Guid FolloweeId => _followeeId;
    public User Followee => _followee;

    private Follow() { }

    public Follow(Guid followerId, Guid followeeId)
    {
        _followerId = followerId;
        _followeeId = followeeId;
    }
}
