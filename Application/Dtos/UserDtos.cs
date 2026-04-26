using System.Linq.Expressions;
using Domain;

namespace Application.Dtos;

public record UserProfileResponse(
    Guid Id,
    string DisplayName,
    string? Bio,
    string AvatarUrl,
    int FollowersCount,
    int FollowingCount,
    int PublicationsCount,
    bool IsFollowing)
{
    public static Expression<Func<User, UserProfileResponse>> Projection => u => new UserProfileResponse(
        u.Id,
        u.DisplayName,
        u.Bio,
        u.AvatarUrl,
        u.Followers.Count,
        u.Following.Count,
        u.Publications.Count,
        false // Default for projection, will be filled in handler if needed
    );
}

public record UpdateProfileRequest(
    string? DisplayName,
    string? Bio);

public record UploadAvatarRequest(
    Microsoft.AspNetCore.Http.IFormFile File);
