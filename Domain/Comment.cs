namespace Domain;

public sealed class Comment
{
    private Guid _id;
    private string _text = string.Empty;
    private DateTimeOffset _createdAt;
    private Guid _userId;
    private User _user = null!;
    private Guid? _publicationId;
    private Publication _publication = null!;
    private Guid? _parentCommentId;
    private Comment? _parentComment;
    private int _likesCount;
    private int _repliesCount;
    private bool _isDeleted;

    private readonly List<Comment> _replies = new();
    private readonly List<CommentLike> _likes = new();

    public Guid Id => _id;
    public string Text => _text;
    public DateTimeOffset CreatedAt => _createdAt;
    public int LikesCount => _likesCount;
    public int RepliesCount => _repliesCount;
    public bool IsDeleted => _isDeleted;

    public Guid UserId => _userId;
    public User User => _user;

    public Guid? PublicationId => _publicationId;
    public Publication Publication => _publication;

    public Guid? ParentCommentId => _parentCommentId;
    public Comment? ParentComment => _parentComment;

    public IReadOnlyCollection<Comment> Replies => _replies.AsReadOnly();
    public IReadOnlyCollection<CommentLike> Likes => _likes.AsReadOnly();

    private Comment() { }

    public Comment(Guid userId, Guid publicationId, string text, Guid? parentCommentId = null)
    {
        if (userId == Guid.Empty) throw new ArgumentException("User ID cannot be empty.", nameof(userId));
        if (publicationId == Guid.Empty) throw new ArgumentException("Publication ID cannot be empty.", nameof(publicationId));
        if (string.IsNullOrWhiteSpace(text)) throw new ArgumentException("Text cannot be empty.", nameof(text));

        _userId = userId;
        _publicationId = publicationId;
        _text = text;
        _parentCommentId = parentCommentId;
        _createdAt = DateTimeOffset.UtcNow;
    }

    public void MarkAsDeleted()
    {
        _isDeleted = true;
    }

    public void DisconnectAndMarkAsDeleted()
    {
        _publicationId = null;
        _isDeleted = true;
    }

    public void DecrementRepliesCount()
    {
        _repliesCount = Math.Max(0, _repliesCount - 1);
    }

    internal void AddReply(Comment reply)
    {
        _replies.Add(reply);
        _repliesCount++;
    }

    internal void RemoveReply(Comment reply)
    {
        if (_replies.Remove(reply))
        {
            _repliesCount = Math.Max(0, _repliesCount - 1);
        }
    }

    public void UpdateLikesCount(int change)
    {
        _likesCount += change;
        if (_likesCount < 0) _likesCount = 0;
    }
}
