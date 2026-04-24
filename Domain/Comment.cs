namespace Domain;

public sealed class Comment
{
    private Guid _id;
    private string _text = string.Empty;
    private DateTimeOffset _createdAt;
    private Guid _userId;
    private User _user = null!;
    private Guid _publicationId;
    private Publication _publication = null!;
    private Guid? _parentCommentId;
    private Comment? _parentComment;

    private readonly List<Comment> _replies = new();
    private readonly List<CommentLike> _likes = new();

    public Guid Id => _id;
    public string Text => _text;
    public DateTimeOffset CreatedAt => _createdAt;

    public Guid UserId => _userId;
    public User User => _user;

    public Guid PublicationId => _publicationId;
    public Publication Publication => _publication;

    public Guid? ParentCommentId => _parentCommentId;
    public Comment? ParentComment => _parentComment;

    public IReadOnlyCollection<Comment> Replies => _replies.AsReadOnly();
    public IReadOnlyCollection<CommentLike> Likes => _likes.AsReadOnly();

    private Comment() { }

    public Comment(Guid userId, Guid publicationId, string text, Guid? parentCommentId = null)
    {
        _userId = userId;
        _publicationId = publicationId;
        _text = text;
        _parentCommentId = parentCommentId;
        _createdAt = DateTimeOffset.UtcNow;
    }

    public void AddReply(Comment reply)
    {
        _replies.Add(reply);
    }

    public void ToggleLike(Guid userId)
    {
        var existing = _likes.FirstOrDefault(l => l.UserId == userId);
        if (existing is not null)
        {
            _likes.Remove(existing);
        }
        else
        {
            _likes.Add(new CommentLike(userId, _id));
        }
    }
}
