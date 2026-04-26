using Application.Interfaces;
using Application.Interfaces.Internal;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SharedSettings.Options;

namespace Application.Services;

internal sealed class DeletionService : IDeletionService
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _storageService;
    private readonly BlobStorageOptions _blobOptions;
    private const int SystemLikesCollectionType = (int)CollectionType.SystemLikes;

    public DeletionService(IApplicationDbContext context, IFileStorageService storageService, IOptions<BlobStorageOptions> blobOptions)
    {
        _context = context;
        _storageService = storageService;
        _blobOptions = blobOptions.Value;
    }

    public async Task DeleteUserContentAsync(Guid userId, CancellationToken cancellationToken)
    {
        // Fetch all file URLs before deleting entities
        var user = await _context.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new { u.AvatarUrl })
            .FirstOrDefaultAsync(cancellationToken);

        var publicationImages = await _context.Publications
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .Select(p => p.Images)
            .ToListAsync(cancellationToken);

        var clothPhotos = await _context.Clothes
            .AsNoTracking()
            .Where(c => c.UserId == userId)
            .Select(c => c.PhotoUrl)
            .ToListAsync(cancellationToken);

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
        await _context.Database.ExecuteSqlInterpolatedAsync($@"
            DELETE FROM CollectionPublications WHERE PublicationId IN (SELECT Id FROM Publications WHERE UserId = {userId}); 
            DELETE FROM CollectionPublications WHERE CollectionId IN (SELECT Id FROM Collections WHERE UserId = {userId}); 
            DELETE FROM PublicationClothes WHERE PublicationId IN (SELECT Id FROM Publications WHERE UserId = {userId}); 
            DELETE FROM PublicationClothes WHERE ClothesId IN (SELECT Id FROM Clothes WHERE UserId = {userId});
            DELETE FROM PublicationTags WHERE PublicationId IN (SELECT Id FROM Publications WHERE UserId = {userId});
            DELETE FROM UserPreferredTags WHERE UserId = {userId};",
            cancellationToken);

        // 3. Comments (on user's publications and comments made by the user), including nested replies
        await _context.Database.ExecuteSqlInterpolatedAsync($@"
            ;WITH RootComments AS (
                SELECT DISTINCT c.Id
                FROM Comments c
                WHERE c.UserId = {userId}
                   OR c.PublicationId IN (SELECT p.Id FROM Publications p WHERE p.UserId = {userId})
            ),
            CommentTree AS (
                SELECT rc.Id
                FROM RootComments rc

                UNION

                SELECT c.Id
                FROM Comments c
                INNER JOIN CommentTree ct ON c.ParentCommentId = ct.Id
            )
            DELETE FROM CommentLikes
            WHERE CommentId IN (SELECT Id FROM CommentTree)
            OPTION (MAXRECURSION 0);

            ;WITH RootComments AS (
                SELECT DISTINCT c.Id
                FROM Comments c
                WHERE c.UserId = {userId}
                   OR c.PublicationId IN (SELECT p.Id FROM Publications p WHERE p.UserId = {userId})
            ),
            CommentTree AS (
                SELECT rc.Id
                FROM RootComments rc

                UNION

                SELECT c.Id
                FROM Comments c
                INNER JOIN CommentTree ct ON c.ParentCommentId = ct.Id
            )
            DELETE FROM Comments
            WHERE Id IN (SELECT Id FROM CommentTree)
            OPTION (MAXRECURSION 0);",
            cancellationToken);

        // 4. Recalculate counters for surviving publications only (not owned by deleted user)
        await _context.Database.ExecuteSqlInterpolatedAsync($@"
            UPDATE p
            SET
                LikesCount = ISNULL(l.LikesCount, 0),
                CommentsCount = ISNULL(cm.CommentsCount, 0),
                AssessmentsCount = ISNULL(a.AssessmentsCount, 0),
                RatingColorSum = ISNULL(a.RatingColorSum, 0),
                RatingFitSum = ISNULL(a.RatingFitSum, 0),
                RatingOriginalitySum = ISNULL(a.RatingOriginalitySum, 0),
                RatingStyleSum = ISNULL(a.RatingStyleSum, 0),
                AverageRating = CASE
                    WHEN ISNULL(a.AssessmentsCount, 0) = 0 THEN 0
                    ELSE CAST(
                        ISNULL(a.RatingColorSum, 0) +
                        ISNULL(a.RatingFitSum, 0) +
                        ISNULL(a.RatingOriginalitySum, 0) +
                        ISNULL(a.RatingStyleSum, 0) AS float
                    ) / (4.0 * ISNULL(a.AssessmentsCount, 0))
                END
            FROM Publications p
            LEFT JOIN (
                SELECT cp.PublicationId, COUNT(*) AS LikesCount
                FROM CollectionPublications cp
                INNER JOIN Collections c ON c.Id = cp.CollectionId
                WHERE c.Type = {SystemLikesCollectionType}
                GROUP BY cp.PublicationId
            ) l ON l.PublicationId = p.Id
            LEFT JOIN (
                SELECT c.PublicationId, COUNT(*) AS CommentsCount
                FROM Comments c
                GROUP BY c.PublicationId
            ) cm ON cm.PublicationId = p.Id
            LEFT JOIN (
                SELECT
                    a.PublicationId,
                    COUNT(*) AS AssessmentsCount,
                    SUM(a.ColorCoordination) AS RatingColorSum,
                    SUM(a.FitAndProportions) AS RatingFitSum,
                    SUM(a.Originality) AS RatingOriginalitySum,
                    SUM(a.OverallStyle) AS RatingStyleSum
                FROM Assessments a
                GROUP BY a.PublicationId
            ) a ON a.PublicationId = p.Id
            WHERE p.UserId <> {userId};",
            cancellationToken);

        // 5. Remaining top-level entities belonging to user
        await _context.Collections.Where(c => c.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        await _context.Clothes.Where(c => c.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        await _context.Publications.Where(p => p.UserId == userId).ExecuteDeleteAsync(cancellationToken);

        // 6. Cleanup files from storage
        if (user?.AvatarUrl != null && user.AvatarUrl != _blobOptions.DefaultAvatarUrl)
        {
            await _storageService.DeleteFileAsync(user.AvatarUrl, cancellationToken);
        }

        foreach (var images in publicationImages)
        {
            foreach (var imageUrl in images)
            {
                await _storageService.DeleteFileAsync(imageUrl, cancellationToken);
            }
        }

        foreach (var photoUrl in clothPhotos)
        {
            if (photoUrl != null)
            {
                await _storageService.DeleteFileAsync(photoUrl, cancellationToken);
            }
        }
    }

    public async Task DeletePublicationContentAsync(Guid publicationId, CancellationToken cancellationToken)
    {
        var publication = await _context.Publications
            .AsNoTracking()
            .Where(p => p.Id == publicationId)
            .Select(p => new { p.Images })
            .FirstOrDefaultAsync(cancellationToken);

        // Rule 8 (Restrict): Manual deletion from junction tables and related entities
        await _context.Database.ExecuteSqlInterpolatedAsync($@"
            DELETE FROM CollectionPublications WHERE PublicationId = {publicationId}; 
            DELETE FROM PublicationClothes WHERE PublicationId = {publicationId}; 
            DELETE FROM PublicationTags WHERE PublicationId = {publicationId};",
            cancellationToken);

        await _context.Assessments
            .Where(a => a.PublicationId == publicationId)
            .ExecuteDeleteAsync(cancellationToken);

        await _context.Database.ExecuteSqlInterpolatedAsync($@"
            ;WITH CommentTree AS (
                SELECT c.Id
                FROM Comments c
                WHERE c.PublicationId = {publicationId}
                  AND (c.ParentCommentId IS NULL OR NOT EXISTS (SELECT 1 FROM Comments parent WHERE parent.Id = c.ParentCommentId))

                UNION ALL

                SELECT c.Id
                FROM Comments c
                INNER JOIN CommentTree ct ON c.ParentCommentId = ct.Id
            )
            DELETE FROM CommentLikes
            WHERE CommentId IN (SELECT Id FROM CommentTree)
            OPTION (MAXRECURSION 0);

            ;WITH CommentTree AS (
                SELECT c.Id
                FROM Comments c
                WHERE c.PublicationId = {publicationId}
                  AND (c.ParentCommentId IS NULL OR NOT EXISTS (SELECT 1 FROM Comments parent WHERE parent.Id = c.ParentCommentId))

                UNION ALL

                SELECT c.Id
                FROM Comments c
                INNER JOIN CommentTree ct ON c.ParentCommentId = ct.Id
            )
            DELETE FROM Comments
            WHERE Id IN (SELECT Id FROM CommentTree)
            OPTION (MAXRECURSION 0);",
            cancellationToken);

        // Cleanup files
        if (publication != null)
        {
            foreach (var imageUrl in publication.Images)
            {
                await _storageService.DeleteFileAsync(imageUrl, cancellationToken);
            }
        }
    }

    public async Task DeleteClothContentAsync(Guid clothId, CancellationToken cancellationToken)
    {
        var cloth = await _context.Clothes
            .AsNoTracking()
            .Where(c => c.Id == clothId)
            .Select(c => new { c.PhotoUrl })
            .FirstOrDefaultAsync(cancellationToken);

        // Rule 8: Manual cleanup of junction table "PublicationClothes"
        await _context.Database.ExecuteSqlRawAsync(
            "DELETE FROM PublicationClothes WHERE ClothesId = {0}",
            clothId,
            cancellationToken);

        // Cleanup file
        if (cloth?.PhotoUrl != null)
        {
            await _storageService.DeleteFileAsync(cloth.PhotoUrl, cancellationToken);
        }
    }
}
