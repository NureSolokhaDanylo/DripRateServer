using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Infrastructure.Authentication;

public sealed class HttpContextCurrentUser : ICurrentUser
{
    private readonly ClaimsPrincipal _principal;
    private readonly IApplicationDbContext _context;
    private bool? _isBannedCache;

    public Guid? UserId
    {
        get
        {
            var id = _principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(id, out var guid) ? guid : null;
        }
    }

    public string? Username => _principal.FindFirst(ClaimTypes.Name)?.Value;
    public bool IsAuthenticated => _principal.Identity?.IsAuthenticated ?? false;

    public IReadOnlyList<string> Roles =>
        _principal.FindAll(ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList()
            .AsReadOnly();

    public HttpContextCurrentUser(IHttpContextAccessor httpContextAccessor, IApplicationDbContext context)
    {
        _principal = httpContextAccessor.HttpContext?.User ?? new ClaimsPrincipal();
        _context = context;
    }

    public bool IsInRole(string role)
    {
        return _principal.IsInRole(role);
    }

    public async Task<bool> IsBannedAsync(CancellationToken cancellationToken = default)
    {
        if (!IsAuthenticated || UserId == null)
        {
            return false;
        }

        if (_isBannedCache.HasValue)
        {
            return _isBannedCache.Value;
        }

        _isBannedCache = await _context.Users
            .Where(u => u.Id == UserId.Value)
            .Select(u => u.IsBanned)
            .FirstOrDefaultAsync(cancellationToken);

        return _isBannedCache.Value;
    }
}
