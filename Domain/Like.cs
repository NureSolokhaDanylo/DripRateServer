namespace Domain;

public sealed class Like
{
    public Guid Id { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    public Guid? PublicationId { get; private set; }
    public Publication? Publication { get; private set; }

    public Guid? CommentId { get; private set; }
    public Comment? Comment { get; private set; }

    private Like() { }

    public static Like ForPublication(Guid userId, Guid publicationId) => new()
    {
        UserId = userId,
        PublicationId = publicationId,
        CreatedAt = DateTimeOffset.UtcNow
    };

    public static Like ForComment(Guid userId, Guid commentId) => new()
    {
        UserId = userId,
        CommentId = commentId,
        CreatedAt = DateTimeOffset.UtcNow
    };
}
