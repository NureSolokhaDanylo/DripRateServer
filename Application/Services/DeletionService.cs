using Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

internal sealed class DeletionService : IDeletionService
{
    private readonly IApplicationDbContext _context;

    public DeletionService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task DeleteUserContentAsync(Guid userId, CancellationToken cancellationToken)
    {
        // 1. Group simple related entities (Likes, Assessments, Follows)
        await _context.CommentLikes
            .Where(l => l.UserId == userId || 
                        l.Comment.UserId == userId || 
                        l.Comment.Publication.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);

        await _context.Assessments
            .Where(a => a.UserId == userId || a.Publication.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);

        await _context.Follows
            .Where(f => f.FollowerId == userId || f.FolloweeId == userId)
            .ExecuteDeleteAsync(cancellationToken);

        // 2. Junction tables for user's Publications, Collections and User itself
        await _context.Database.ExecuteSqlRawAsync(@"
            DELETE FROM CollectionPublications WHERE PublicationId IN (SELECT Id FROM Publications WHERE UserId = {0}); 
            DELETE FROM CollectionPublications WHERE CollectionId IN (SELECT Id FROM Collections WHERE UserId = {0}); 
            DELETE FROM PublicationClothes WHERE PublicationId IN (SELECT Id FROM Publications WHERE UserId = {0}); 
            DELETE FROM PublicationClothes WHERE ClothesId IN (SELECT Id FROM Clothes WHERE UserId = {0});
            DELETE FROM PublicationTags WHERE PublicationId IN (SELECT Id FROM Publications WHERE UserId = {0});
            DELETE FROM UserPreferredTags WHERE UserId = {0};",
            userId, cancellationToken);

        // 3. Comments (on user's publications and comments made by the user)
        await _context.Comments
            .Where(c => c.UserId == userId || c.Publication.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);

        // 4. Remaining top-level entities
        await _context.Collections.Where(c => c.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        await _context.Clothes.Where(c => c.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        await _context.Publications.Where(p => p.UserId == userId).ExecuteDeleteAsync(cancellationToken);
    }

    public async Task DeletePublicationContentAsync(Guid publicationId, CancellationToken cancellationToken)
    {
        // Rule 8 (Restrict): Manual deletion from junction tables and related entities
        await _context.Database.ExecuteSqlRawAsync(
            "DELETE FROM CollectionPublications WHERE PublicationId = {0}; " +
            "DELETE FROM PublicationClothes WHERE PublicationId = {0}; " +
            "DELETE FROM PublicationTags WHERE PublicationId = {0};",
            publicationId,
            cancellationToken);

        await _context.Assessments
            .Where(a => a.PublicationId == publicationId)
            .ExecuteDeleteAsync(cancellationToken);

        await _context.CommentLikes
            .Where(l => l.Comment.PublicationId == publicationId)
            .ExecuteDeleteAsync(cancellationToken);

        await _context.Comments
            .Where(c => c.PublicationId == publicationId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task DeleteClothContentAsync(Guid clothId, CancellationToken cancellationToken)
    {
        // Rule 8: Manual cleanup of junction table "PublicationClothes"
        await _context.Database.ExecuteSqlRawAsync(
            "DELETE FROM PublicationClothes WHERE ClothesId = {0}",
            clothId,
            cancellationToken);
    }
}
