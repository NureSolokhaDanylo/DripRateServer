namespace Domain;

public enum ReportCategory
{
    Spam,
    Harassment,
    InappropriateContent,
    Copyright,
    Other
}

public enum ReportTargetType
{
    Comment,
    Publication,
    User
}

public enum ReportStatus
{
    Pending,
    InReview,
    Resolved,
    Dismissed
}

public enum ModerationAction
{
    DeleteEntity,
    BanUser,
    Dismiss
}

public sealed class Report
{
    private Guid _id;
    private ReportCategory _category;
    private string? _text;
    private Guid _authorId;
    private User _author = null!;
    private ReportTargetType _targetType;
    private Guid _targetId;
    private ReportStatus _status;
    private Guid? _assignedToUserId;
    private User? _assignedToUser;
    private DateTimeOffset? _assignedAt;
    private DateTimeOffset _createdAt;
    private DateTimeOffset? _resolvedAt;

    public Guid Id => _id;
    public ReportCategory Category => _category;
    public string? Text => _text;
    public Guid AuthorId => _authorId;
    public User Author => _author;
    public ReportTargetType TargetType => _targetType;
    public Guid TargetId => _targetId;
    public ReportStatus Status => _status;
    public Guid? AssignedToUserId => _assignedToUserId;
    public User? AssignedToUser => _assignedToUser;
    public DateTimeOffset? AssignedAt => _assignedAt;
    public DateTimeOffset CreatedAt => _createdAt;
    public DateTimeOffset? ResolvedAt => _resolvedAt;

    private Report() { }

    public Report(Guid authorId, ReportTargetType targetType, Guid targetId, ReportCategory category, string? text)
    {
        _id = Guid.NewGuid();
        _authorId = authorId;
        _targetType = targetType;
        _targetId = targetId;
        _category = category;
        _text = text;
        _status = ReportStatus.Pending;
        _createdAt = DateTimeOffset.UtcNow;
    }

    public void AssignTo(Guid moderatorId)
    {
        _assignedToUserId = moderatorId;
        _assignedAt = DateTimeOffset.UtcNow;
        _status = ReportStatus.InReview;
    }

    public void Resolve(ReportStatus status)
    {
        _status = status;
        _resolvedAt = DateTimeOffset.UtcNow;
    }
}
