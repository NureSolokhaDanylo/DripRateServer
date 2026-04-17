using Microsoft.AspNetCore.Identity;

namespace Domain;

public sealed class User : IdentityUser<Guid>
{
    public string? AvatarUrl { get; private set; }
    public string? Bio { get; private set; }

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
}
