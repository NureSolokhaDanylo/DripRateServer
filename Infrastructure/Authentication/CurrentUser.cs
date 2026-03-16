using Application.Interfaces;
using System.Security.Claims;

namespace Infrastructure.Authentication;

public sealed class CurrentUser : ICurrentUser
{
    private readonly ClaimsPrincipal _principal;

    public string? UserId => _principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    public string? Username => _principal.FindFirst(ClaimTypes.Name)?.Value;
    public bool IsAuthenticated => _principal.Identity?.IsAuthenticated ?? false;

    public IReadOnlyList<string> Roles =>
        _principal.FindAll(ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList()
            .AsReadOnly();

    public CurrentUser(ClaimsPrincipal principal)
    {
        _principal = principal ?? throw new ArgumentNullException(nameof(principal));
    }

    public bool IsInRole(string role)
    {
        return _principal.IsInRole(role);
    }
}
