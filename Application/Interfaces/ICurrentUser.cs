namespace Application.Interfaces;

public interface ICurrentUser
{
    Guid? UserId { get; }
    string? Username { get; }
    IReadOnlyList<string> Roles { get; }
    bool IsAuthenticated { get; }

    bool IsInRole(string role);
}
