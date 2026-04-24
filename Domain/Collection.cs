namespace Domain;

public enum CollectionType
{
    UserDefined = 0,
    SystemLikes = 1,
    SystemSaved = 2
}

public sealed class Collection
{
    private Guid _id;
    private string _name = string.Empty;
    private string? _description;
    private bool _isPublic;
    private bool _isSystem;
    private CollectionType _type;
    private DateTimeOffset _createdAt;
    private Guid _userId;
    private User _user = null!;

    private readonly List<Publication> _publications = new();

    public Guid Id => _id;
    public string Name => _name;
    public string? Description => _description;
    public bool IsPublic => _isPublic;
    public bool IsSystem => _isSystem;
    public CollectionType Type => _type;
    public DateTimeOffset CreatedAt => _createdAt;

    public Guid UserId => _userId;
    public User User => _user;

    public IReadOnlyCollection<Publication> Publications => _publications.AsReadOnly();

    private Collection() { }

    private Collection(Guid userId, string name, string? description, bool isPublic, bool isSystem, CollectionType type)
    {
        _userId = userId;
        _name = name;
        _description = description;
        _isPublic = isPublic;
        _isSystem = isSystem;
        _type = type;
        _createdAt = DateTimeOffset.UtcNow;
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
