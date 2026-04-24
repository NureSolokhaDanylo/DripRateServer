using Microsoft.AspNetCore.Identity;

namespace Domain;

public sealed class User : IdentityUser<Guid>
{
    private string? _avatarUrl;
    private string? _bio;

    private readonly List<Publication> _publications = new();
    private readonly List<Cloth> _wardrobe = new();
    private readonly List<Follow> _followers = new();
    private readonly List<Follow> _following = new();
    private readonly List<Collection> _collections = new();
    private readonly List<Tag> _preferredTags = new();

    public new string? Email { get => base.Email; private set => base.Email = value; }
    public new string? UserName { get => base.UserName; private set => base.UserName = value; }

    public string? AvatarUrl => _avatarUrl;
    public string? Bio => _bio;

    public IReadOnlyCollection<Publication> Publications => _publications.AsReadOnly();
    public IReadOnlyCollection<Cloth> Wardrobe => _wardrobe.AsReadOnly();
    public IReadOnlyCollection<Follow> Followers => _followers.AsReadOnly();
    public IReadOnlyCollection<Follow> Following => _following.AsReadOnly();
    public IReadOnlyCollection<Collection> Collections => _collections.AsReadOnly();
    public IReadOnlyCollection<Tag> PreferredTags => _preferredTags.AsReadOnly();

    private User() { }

    public User(string email, string userName)
    {
        Email = email;
        UserName = userName;
    }

    public void UpdateProfile(string? bio)
    {
        _bio = bio;
    }

    public void UpdateAvatar(string? avatarUrl)
    {
        _avatarUrl = avatarUrl;
    }

    public void AddToWardrobe(Cloth item) => _wardrobe.Add(item);

    public void InitializeCollections()
    {
        if (!_collections.Any(c => c.Type == CollectionType.SystemLikes))
        {
            _collections.Add(Collection.CreateLikes(Id));
        }
    }

    public void SetPreferredTags(IEnumerable<Tag> tags)
    {
        _preferredTags.Clear();
        _preferredTags.AddRange(tags);
    }
}
