namespace Application.Dtos;

public record UserProfileResponse(
    Guid Id,
    string Username,
    string? Bio,
    string? AvatarUrl,
    int FollowersCount,
    int FollowingCount,
    int PublicationsCount);

public record UpdateProfileRequest(
    string? Bio);
