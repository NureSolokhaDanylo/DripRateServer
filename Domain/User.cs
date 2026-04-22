using Microsoft.AspNetCore.Identity;

namespace Domain;

public sealed class User : IdentityUser<Guid>
{
    private readonly List<Publication> _publications = new();
    private readonly List<Cloth> _wardrobe = new();
    private readonly List<Follow> _followers = new();
    private readonly List<Follow> _following = new();
    private readonly List<Collection> _collections = new();
    private readonly List<Tag> _preferredTags = new();

    public string? AvatarUrl { get; private set; }
    public string? Bio { get; private set; }

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
        Bio = bio;
    }

    public void UpdateAvatar(string? avatarUrl)
    {
        AvatarUrl = avatarUrl;
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
