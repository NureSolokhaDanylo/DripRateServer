using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Infrastructure.Authentication;

public sealed class HttpContextCurrentUser : ICurrentUser
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

    public HttpContextCurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        var principal = httpContextAccessor.HttpContext?.User ?? new ClaimsPrincipal();
        _principal = principal ?? throw new ArgumentNullException(nameof(principal));
    }

    public bool IsInRole(string role)
    {
        return _principal.IsInRole(role);
    }
}
