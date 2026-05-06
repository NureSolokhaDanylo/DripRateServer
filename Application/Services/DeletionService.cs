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

    public DeletionService(IApplicationDbContext context, IFileStorageService storageService)
    {
        _context = context;
        _storageService = storageService;
    }

    public async Task DeletePublicationContentAsync(Guid publicationId, CancellationToken cancellationToken)
    {
        var publication = await _context.Publications
            .AsNoTracking()
            .Where(p => p.Id == publicationId)
            .Select(p => new { p.Images })
            .FirstOrDefaultAsync(cancellationToken);

        // Fetch all comments for the publication to determine the deletion order
        var comments = await _context.Comments
            .AsNoTracking()
            .Where(c => c.PublicationId == publicationId)
            .Select(c => new { c.Id, c.ParentCommentId })
            .ToListAsync(cancellationToken);

        var commentIds = comments.Select(c => c.Id).ToHashSet();

        while (commentIds.Count > 0)
        {
            // Find leaf comments (comments that are not parents to any remaining comments)
            var parentIds = comments
                .Where(c => c.ParentCommentId.HasValue && commentIds.Contains(c.Id))
                .Select(c => c.ParentCommentId!.Value)
                .ToHashSet();

            var leaves = commentIds.Except(parentIds).ToList();

            if (leaves.Count == 0)
                break; // Safety break in case of circular references

            await _context.Comments
                .Where(c => leaves.Contains(c.Id))
                .ExecuteDeleteAsync(cancellationToken);

            foreach (var leaf in leaves)
            {
                commentIds.Remove(leaf);
            }
        }

        // Cleanup files
        if (publication != null)
        {
            foreach (var imageUrl in publication.Images)
            {
                await _storageService.DeleteFileAsync(imageUrl, cancellationToken);
            }
        }
    }

    public async Task DeleteCommentAsync(Guid commentId, CancellationToken cancellationToken)
    {
        var comment = await _context.Comments
            .AsNoTracking()
            .Where(c => c.Id == commentId)
            .Select(c => new { c.Id, c.PublicationId, c.ParentCommentId })
            .FirstOrDefaultAsync(cancellationToken);

        if (comment == null) return;

        // Load all comments for the publication to build the tree in memory
        var allCommentsInPublication = await _context.Comments
            .AsNoTracking()
            .Where(c => c.PublicationId == comment.PublicationId)
            .Select(c => new { c.Id, c.ParentCommentId })
            .ToListAsync(cancellationToken);

        // Find all descendants of the target comment
        var descendantIds = new HashSet<Guid>();
        var queue = new Queue<Guid>();
        queue.Enqueue(commentId);

        while (queue.Count > 0)
        {
            var currentId = queue.Dequeue();
            descendantIds.Add(currentId);

            var children = allCommentsInPublication.Where(c => c.ParentCommentId == currentId);
            foreach (var child in children)
            {
                queue.Enqueue(child.Id);
            }
        }

        var deletedCommentsCount = descendantIds.Count;

        if (comment.ParentCommentId.HasValue)
        {
            await _context.Comments
                .Where(c => c.Id == comment.ParentCommentId.Value)
                .ExecuteUpdateAsync(s => s.SetProperty(c => c.RepliesCount, c => c.RepliesCount > 0 ? c.RepliesCount - 1 : 0), cancellationToken);
        }

        // Delete from bottom to top
        var remainingIdsToDelete = descendantIds.ToHashSet();

        while (remainingIdsToDelete.Count > 0)
        {
            var parentIds = allCommentsInPublication
                .Where(c => c.ParentCommentId.HasValue && remainingIdsToDelete.Contains(c.Id))
                .Select(c => c.ParentCommentId!.Value)
                .ToHashSet();

            var leaves = remainingIdsToDelete.Except(parentIds).ToList();

            if (leaves.Count == 0)
                break;

            await _context.Comments
                .Where(c => leaves.Contains(c.Id))
                .ExecuteDeleteAsync(cancellationToken);

            foreach (var leaf in leaves)
            {
                remainingIdsToDelete.Remove(leaf);
            }
        }

        await _context.Publications
            .Where(p => p.Id == comment.PublicationId)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.CommentsCount, p => p.CommentsCount >= deletedCommentsCount ? p.CommentsCount - deletedCommentsCount : 0), cancellationToken);
    }
}
