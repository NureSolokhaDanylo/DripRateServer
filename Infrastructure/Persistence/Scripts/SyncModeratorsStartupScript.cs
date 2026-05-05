using Application.Interfaces;
using Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence.Scripts;

public sealed class SyncModeratorsStartupScript : IStartupScript
{
    private readonly UserManager<User> _userManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SyncModeratorsStartupScript> _logger;

    public int Order => 10; // Run after SeedRolesStartupScript (Order 5)

    public SyncModeratorsStartupScript(
        UserManager<User> userManager,
        IConfiguration configuration,
        ILogger<SyncModeratorsStartupScript> logger)
    {
        _userManager = userManager;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var moderatorEmailsString = _configuration["MODERATOR_EMAILS"];
        var targetEmails = string.IsNullOrWhiteSpace(moderatorEmailsString)
            ? new HashSet<string>()
            : moderatorEmailsString
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(e => e.ToLowerInvariant())
                .ToHashSet();

        if (targetEmails.Count == 0)
        {
            _logger.LogInformation("No moderator emails configured. All existing moderators will be demoted if any.");
        }
        else
        {
            _logger.LogInformation("Syncing moderators. Target emails: {Emails}", string.Join(", ", targetEmails));
        }

        // 1. Get all current moderators
        var currentModerators = await _userManager.GetUsersInRoleAsync("Moderator");
        
        // 2. Remove role from those not in the list
        foreach (var user in currentModerators)
        {
            if (user.Email != null && !targetEmails.Contains(user.Email.ToLowerInvariant()))
            {
                _logger.LogInformation("Removing Moderator role from user {Email}", user.Email);
                await _userManager.RemoveFromRoleAsync(user, "Moderator");
            }
        }

        // 3. Add role to those in the list who don't have it
        foreach (var email in targetEmails)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                var isInRole = await _userManager.IsInRoleAsync(user, "Moderator");
                if (!isInRole)
                {
                    _logger.LogInformation("Adding Moderator role to user {Email}", email);
                    await _userManager.AddToRoleAsync(user, "Moderator");
                }
            }
            else
            {
                _logger.LogWarning("User with email {Email} not found. Cannot assign Moderator role.", email);
            }
        }
    }
}
