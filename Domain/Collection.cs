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

    private readonly List<CollectionPublication> _collectionPublications = new();

    public Guid Id => _id;
    public string Name => _name;
    public string? Description => _description;
    public bool IsPublic => _isPublic;
    public bool IsSystem => _isSystem;
    public CollectionType Type => _type;
    public DateTimeOffset CreatedAt => _createdAt;

    public Guid UserId => _userId;
    public User User => _user;

    public IReadOnlyCollection<CollectionPublication> CollectionPublications => _collectionPublications.AsReadOnly();

    private Collection() { }

    private Collection(Guid userId, string name, string? description, bool isPublic, bool isSystem, CollectionType type)
    {
        if (userId == Guid.Empty) throw new ArgumentException("User ID cannot be empty.", nameof(userId));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be empty.", nameof(name));

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

    public static Collection CreateSaved(Guid userId)
        => new(userId, "Saved", "My saved publications", isPublic: false, isSystem: true, CollectionType.SystemSaved);

    public void AddPublication(Publication publication)
    {
        if (_collectionPublications.All(cp => cp.PublicationId != publication.Id))
        {
            _collectionPublications.Add(new CollectionPublication(Id, publication.Id));
        }
    }

    public void RemovePublication(Publication publication)
    {
        var cp = _collectionPublications.FirstOrDefault(cp => cp.PublicationId == publication.Id);
        if (cp != null)
        {
            _collectionPublications.Remove(cp);
        }
    }

    public void ClearPublications()
    {
        _collectionPublications.Clear();
    }

    public void Update(string name, string? description, bool isPublic)
    {
        if (IsSystem) return;
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be empty.", nameof(name));

        _name = name;
        _description = description;
        _isPublic = isPublic;
    }
}
