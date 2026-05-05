using Application.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence.Scripts;

public sealed class SeedRolesStartupScript : IStartupScript
{
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly ILogger<SeedRolesStartupScript> _logger;

    public int Order => 5; // Run before tags

    public SeedRolesStartupScript(RoleManager<IdentityRole<Guid>> roleManager, ILogger<SeedRolesStartupScript> logger)
    {
        _roleManager = roleManager;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var roles = new[] { "Moderator", "User" };

        foreach (var roleName in roles)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                _logger.LogInformation("Seeding role: {RoleName}", roleName);
                await _roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
            }
        }
    }
}
