namespace Domain;

public sealed class CommentLike
{
    private Guid _userId;
    private User _user = null!;
    private Guid _commentId;
    private Comment _comment = null!;
    private DateTimeOffset _createdAt;

    public Guid UserId => _userId;
    public User User => _user;

    public Guid CommentId => _commentId;
    public Comment Comment => _comment;

    public DateTimeOffset CreatedAt => _createdAt;

    private CommentLike() { }

    public CommentLike(Guid userId, Guid commentId)
    {
        _userId = userId;
        _commentId = commentId;
        _createdAt = DateTimeOffset.UtcNow;
    }
}
