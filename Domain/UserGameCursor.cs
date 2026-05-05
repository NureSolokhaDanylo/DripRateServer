namespace Domain;

public sealed class UserGameCursor
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;
    
    public GameType GameType { get; private set; }
    
    public DateTimeOffset LastSeenCreatedAt { get; private set; }

    private UserGameCursor() { }

    public UserGameCursor(Guid userId, GameType gameType, DateTimeOffset lastSeenCreatedAt)
    {
        UserId = userId;
        GameType = gameType;
        LastSeenCreatedAt = lastSeenCreatedAt;
    }

    public void UpdateCursor(DateTimeOffset lastSeenCreatedAt)
    {
        LastSeenCreatedAt = lastSeenCreatedAt;
    }
}
