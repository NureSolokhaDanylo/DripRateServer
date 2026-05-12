using Application.Interfaces;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence.Scripts;

public sealed class CleanupCollectionsStartupScript : IStartupScript
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<CleanupCollectionsStartupScript> _logger;

    public int Order => 10; // Run after roles/tags if any dependencies

    public CleanupCollectionsStartupScript(IApplicationDbContext context, ILogger<CleanupCollectionsStartupScript> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting collection cleanup script...");

        // 1. Remove duplicates
        var allEntries = await _context.CollectionPublications
            .ToListAsync(cancellationToken);

        var duplicates = allEntries
            .GroupBy(cp => new { cp.CollectionId, cp.PublicationId })
            .Where(g => g.Count() > 1)
            .ToList();

        if (duplicates.Any())
        {
            _logger.LogInformation("Found {Count} duplicate collection-publication pairs. Cleaning up...", duplicates.Count);
            foreach (var group in duplicates)
            {
                var toRemove = group.Skip(1).ToList();
                _context.CollectionPublications.RemoveRange(toRemove);
            }
            await _context.SaveChangesAsync(cancellationToken);
        }

        // 2. Fix default AddedAt
        var entriesToFix = await _context.CollectionPublications
            .Include(cp => cp.Publication)
            .Where(cp => cp.AddedAt == default)
            .ToListAsync(cancellationToken);

        if (entriesToFix.Any())
        {
            _logger.LogInformation("Found {Count} entries with default AddedAt. Fixing...", entriesToFix.Count);
            foreach (var entry in entriesToFix)
            {
                // We use Reflection to set the private field _addedAt because it's a private field in Domain entity
                // and we don't have a public setter or a specific method for this maintenance task.
                var field = typeof(CollectionPublication).GetField("_addedAt", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    field.SetValue(entry, entry.Publication.CreatedAt.AddMinutes(5));
                }
            }
            await _context.SaveChangesAsync(cancellationToken);
        }

        _logger.LogInformation("Collection cleanup script finished.");
    }
}
