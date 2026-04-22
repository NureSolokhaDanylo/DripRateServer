namespace Domain;

public sealed class CommentLike
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    public Guid CommentId { get; private set; }
    public Comment Comment { get; private set; } = null!;

    public DateTimeOffset CreatedAt { get; private set; }

    private CommentLike() { }

    public CommentLike(Guid userId, Guid commentId)
    {
        UserId = userId;
        CommentId = commentId;
        CreatedAt = DateTimeOffset.UtcNow;
    }
}
