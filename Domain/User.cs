using Microsoft.AspNetCore.Identity;

namespace Domain;

public sealed class User : IdentityUser<Guid>
{
    private string _avatarUrl = null!;
    private string? _bio;
    private DateTimeOffset _createdAt;
    private bool _isBanned;

    private readonly List<Publication> _publications = new();
    private readonly List<Cloth> _wardrobe = new();
    private readonly List<Follow> _followers = new();
    private readonly List<Follow> _following = new();
    private readonly List<Collection> _collections = new();
    private readonly List<Tag> _preferredTags = new();

    public string DisplayName { get; private set; } = null!;
    public string AvatarUrl => _avatarUrl;
    public string? Bio => _bio;
    public DateTimeOffset CreatedAt => _createdAt;
    public bool IsBanned => _isBanned;

    public IReadOnlyCollection<Publication> Publications => _publications.AsReadOnly();
    public IReadOnlyCollection<Cloth> Wardrobe => _wardrobe.AsReadOnly();
    public IReadOnlyCollection<Follow> Followers => _followers.AsReadOnly();
    public IReadOnlyCollection<Follow> Following => _following.AsReadOnly();
    public IReadOnlyCollection<Collection> Collections => _collections.AsReadOnly();
    public IReadOnlyCollection<Tag> PreferredTags => _preferredTags.AsReadOnly();

    private User() { }

    public User(string email, string displayName)
    {
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email cannot be empty.", nameof(email));
        if (string.IsNullOrWhiteSpace(displayName)) throw new ArgumentException("Display name cannot be empty.", nameof(displayName));

        Email = email;
        UserName = email;
        DisplayName = displayName;
        _createdAt = DateTimeOffset.UtcNow;
    }

    public void UpdateProfile(string? displayName, string? bio)
    {
        if (!string.IsNullOrWhiteSpace(displayName))
        {
            DisplayName = displayName;
        }
        _bio = bio;
    }

    public void UpdateAvatar(string avatarUrl)
    {
        if (string.IsNullOrWhiteSpace(avatarUrl)) throw new ArgumentException("Avatar URL cannot be empty.", nameof(avatarUrl));
        if (!Uri.IsWellFormedUriString(avatarUrl, UriKind.Absolute)) throw new ArgumentException("Invalid avatar URL.", nameof(avatarUrl));

        _avatarUrl = avatarUrl;
    }

    public void AddToWardrobe(Cloth item) => _wardrobe.Add(item);

    public void InitializeCollections()
    {
        if (!_collections.Any(c => c.Type == CollectionType.SystemLikes))
        {
            _collections.Add(Collection.CreateLikes(Id));
        }

        if (!_collections.Any(c => c.Type == CollectionType.SystemSaved))
        {
            _collections.Add(Collection.CreateSaved(Id));
        }
    }

    public void SetPreferredTags(IEnumerable<Tag> tags)
    {
        _preferredTags.Clear();
        _preferredTags.AddRange(tags);
    }

    public void Ban() => _isBanned = true;
    public void Unban() => _isBanned = false;
}