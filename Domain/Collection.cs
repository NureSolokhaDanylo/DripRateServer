namespace Domain;

public enum CollectionType
{
    UserDefined = 0,
    SystemLikes = 1,
    SystemSaved = 2
}

public sealed class Collection
{
    private readonly List<Publication> _publications = new();

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsPublic { get; private set; }
    public bool IsSystem { get; private set; }
    public CollectionType Type { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    public IReadOnlyCollection<Publication> Publications => _publications.AsReadOnly();

    private Collection() { }

    private Collection(Guid userId, string name, string? description, bool isPublic, bool isSystem, CollectionType type)
    {
        UserId = userId;
        Name = name;
        Description = description;
        IsPublic = isPublic;
        IsSystem = isSystem;
        Type = type;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public static Collection CreateUserDefined(Guid userId, string name, string? description, bool isPublic)
        => new(userId, name, description, isPublic, isSystem: false, CollectionType.UserDefined);

    public static Collection CreateLikes(Guid userId)
        => new(userId, "Likes", "My liked publications", isPublic: false, isSystem: true, CollectionType.SystemLikes);

    public void AddPublication(Publication publication)
    {
        if (!_publications.Contains(publication))
        {
            _publications.Add(publication);
        }
    }

    public void RemovePublication(Publication publication)
    {
        _publications.Remove(publication);
    }
}
