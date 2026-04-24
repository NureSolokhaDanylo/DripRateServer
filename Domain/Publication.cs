namespace Domain;

public sealed class Publication
{
    private Guid _id;
    private string _description = string.Empty;
    private DateTimeOffset _createdAt;
    private Guid _userId;
    private User _user = null!;

    private readonly List<Tag> _tags = new();
    private readonly List<Cloth> _clothes = new();
    private readonly List<Comment> _comments = new();
    private readonly List<Assessment> _assessments = new();
    private readonly List<string> _images = new();
    private readonly List<Collection> _collections = new();

    public Guid Id => _id;
    public string Description => _description;
    public DateTimeOffset CreatedAt => _createdAt;
    public Guid UserId => _userId;
    public User User => _user;

    public IReadOnlyCollection<string> Images => _images.AsReadOnly();
    public IReadOnlyCollection<Tag> Tags => _tags.AsReadOnly();
    public IReadOnlyCollection<Cloth> Clothes => _clothes.AsReadOnly();
    public IReadOnlyCollection<Comment> Comments => _comments.AsReadOnly();
    public IReadOnlyCollection<Assessment> Assessments => _assessments.AsReadOnly();
    public IReadOnlyCollection<Collection> Collections => _collections.AsReadOnly();

    private Publication() { }

    public Publication(Guid userId, string description, IEnumerable<string> images)
    {
        _userId = userId;
        _description = description;
        _createdAt = DateTimeOffset.UtcNow;
        _images.AddRange(images);
    }

    public void AddTag(Tag tag)
    {
        if (!_tags.Contains(tag)) _tags.Add(tag);
    }

    public void AttachCloth(Cloth cloth)
    {
        if (!_clothes.Contains(cloth)) _clothes.Add(cloth);
    }

    public void AddComment(Comment comment) => _comments.Add(comment);
}
