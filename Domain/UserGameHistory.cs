namespace Domain;

public sealed class UserGameHistory
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    public Guid PublicationId { get; private set; }
    public Publication Publication { get; private set; } = null!;

    public GameType GameType { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    private UserGameHistory() { }

    public UserGameHistory(Guid userId, Guid publicationId, GameType gameType)
    {
        if (userId == Guid.Empty) throw new ArgumentException("User ID cannot be empty.", nameof(userId));
        if (publicationId == Guid.Empty) throw new ArgumentException("Publication ID cannot be empty.", nameof(publicationId));

        UserId = userId;
        PublicationId = publicationId;
        GameType = gameType;
        CreatedAt = DateTimeOffset.UtcNow;
    }
}
