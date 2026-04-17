namespace Domain;

public sealed class Comment
{
    private readonly List<Comment> _replies = new();

    public Guid Id { get; private set; }
    public string Text { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }

    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    public Guid PublicationId { get; private set; }
    public Publication Publication { get; private set; } = null!;

    public Guid? ParentCommentId { get; private set; }
    public Comment? ParentComment { get; private set; }

    public IReadOnlyCollection<Comment> Replies => _replies.AsReadOnly();

    private Comment() { }

    public Comment(Guid userId, Guid publicationId, string text, Guid? parentCommentId = null)
    {
        UserId = userId;
        PublicationId = publicationId;
        Text = text;
        ParentCommentId = parentCommentId;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void AddReply(Comment reply)
    {
        _replies.Add(reply);
    }
}
