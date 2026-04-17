using Microsoft.AspNetCore.Identity;

namespace Domain;

public sealed class User : IdentityUser<Guid>
{
    private readonly List<Publication> _publications = new();
    private readonly List<Cloth> _wardrobe = new();
    private readonly List<Follow> _followers = new();
    private readonly List<Follow> _following = new();

    public string? AvatarUrl { get; private set; }
    public string? Bio { get; private set; }

    public IReadOnlyCollection<Publication> Publications => _publications.AsReadOnly();
    public IReadOnlyCollection<Cloth> Wardrobe => _wardrobe.AsReadOnly();
    public IReadOnlyCollection<Follow> Followers => _followers.AsReadOnly();
    public IReadOnlyCollection<Follow> Following => _following.AsReadOnly();

    private User() { }

    public User(string email, string userName)
    {
        Email = email;
        UserName = userName;
    }

    public void UpdateProfile(string? avatarUrl, string? bio)
    {
        AvatarUrl = avatarUrl;
        Bio = bio;
    }

    public void AddToWardrobe(Cloth item) => _wardrobe.Add(item);
}
