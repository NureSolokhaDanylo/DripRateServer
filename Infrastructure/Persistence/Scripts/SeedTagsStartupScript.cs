using Application.Interfaces;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace Infrastructure.Persistence.Scripts;

public sealed class SeedTagsStartupScript : IStartupScript
{
    private readonly MyDbContext _context;
    private readonly ILogger<SeedTagsStartupScript> _logger;

    public int Order => 10; // Run after migrations but early on

    public SeedTagsStartupScript(MyDbContext context, ILogger<SeedTagsStartupScript> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (await _context.Tags.AnyAsync(cancellationToken))
        {
            _logger.LogInformation("Tags already seeded. Skipping.");
            return;
        }

        _logger.LogInformation("Seeding initial tags...");

        var tagsToSeed = new List<Tag>
        {
            // Aesthetics
            new("Streetwear", "Aesthetics"),
            new("Y2K", "Aesthetics"),
            new("Grunge", "Aesthetics"),
            new("Minimalist", "Aesthetics"),
            new("Avant-Garde", "Aesthetics"),
            new("Goth", "Aesthetics"),
            new("Cyberpunk", "Aesthetics"),
            new("Old Money", "Aesthetics"),
            new("Techwear", "Aesthetics"),
            new("Vintage", "Aesthetics"),
            new("Soft Girl/Boy", "Aesthetics"),
            new("Opium", "Aesthetics"),
            new("Skater", "Aesthetics"),

            // Occasion
            new("Casual", "Occasion"),
            new("Office/Corporate", "Occasion"),
            new("Night Out", "Occasion"),
            new("Active/Gym", "Occasion"),
            new("Date Night", "Occasion"),
            new("Loungewear", "Occasion"),
            new("Festival", "Occasion"),

            // Vibes
            new("Cozy", "Vibes"),
            new("Edgy", "Vibes"),
            new("Elegant", "Vibes"),
            new("Hypebeast", "Vibes"),
            new("Chill", "Vibes"),
            new("Loud", "Vibes"),

            // Seasons
            new("Summer", "Seasons"),
            new("Winter", "Seasons"),
            new("Spring", "Seasons"),
            new("Autumn", "Seasons")
        };

        _context.Tags.AddRange(tagsToSeed);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Seeded {Count} tags successfully.", tagsToSeed.Count);
    }
}
