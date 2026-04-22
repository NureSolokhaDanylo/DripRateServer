namespace Domain;

public sealed class Publication
{
    private readonly List<Tag> _tags = new();
    private readonly List<Cloth> _clothes = new();
    private readonly List<Comment> _comments = new();
    private readonly List<Assessment> _assessments = new();
    private readonly List<string> _images = new();
    private readonly List<Collection> _collections = new();

    public Guid Id { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
    
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    public IReadOnlyCollection<string> Images => _images.AsReadOnly();
    public IReadOnlyCollection<Tag> Tags => _tags.AsReadOnly();
    public IReadOnlyCollection<Cloth> Clothes => _clothes.AsReadOnly();
    public IReadOnlyCollection<Comment> Comments => _comments.AsReadOnly();
    public IReadOnlyCollection<Assessment> Assessments => _assessments.AsReadOnly();
    public IReadOnlyCollection<Collection> Collections => _collections.AsReadOnly();

    private Publication() { }

    public Publication(Guid userId, string description, IEnumerable<string> images)
    {
        UserId = userId;
        Description = description;
        CreatedAt = DateTimeOffset.UtcNow;
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
