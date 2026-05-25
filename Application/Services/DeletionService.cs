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
            .IgnoreQueryFilters()
            .Where(p => p.Id == publicationId)
            .Select(p => new { p.Images })
            .FirstOrDefaultAsync(cancellationToken);

        // Load all comments belonging to the publication with tracking
        var comments = await _context.Comments
            .IgnoreQueryFilters()
            .Where(c => c.PublicationId == publicationId)
            .ToListAsync(cancellationToken);

        foreach (var comment in comments)
        {
            comment.DisconnectAndMarkAsDeleted();
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
            .IgnoreQueryFilters()
            .Where(c => c.Id == commentId)
            .FirstOrDefaultAsync(cancellationToken);

        if (comment == null) return;

        Publication? publication = null;
        List<Comment> allCommentsInPublication = new();

        if (comment.PublicationId.HasValue)
        {
            // Load the publication with tracking
            publication = await _context.Publications
                .IgnoreQueryFilters()
                .Where(p => p.Id == comment.PublicationId.Value)
                .FirstOrDefaultAsync(cancellationToken);

            // Load all comments for the publication with tracking to build the tree and mark them
            allCommentsInPublication = await _context.Comments
                .IgnoreQueryFilters()
                .Where(c => c.PublicationId == comment.PublicationId.Value)
                .ToListAsync(cancellationToken);
        }
        else
        {
            allCommentsInPublication.Add(comment);
        }

        // Find the parent comment if it exists in the loaded list
        Comment? parentComment = null;
        if (comment.ParentCommentId.HasValue)
        {
            parentComment = allCommentsInPublication.FirstOrDefault(c => c.Id == comment.ParentCommentId.Value);
        }

        // Find all descendants of the target comment
        var descendants = new List<Comment>();
        var queue = new Queue<Guid>();
        queue.Enqueue(commentId);

        var descendantIds = new HashSet<Guid>();

        while (queue.Count > 0)
        {
            var currentId = queue.Dequeue();
            descendantIds.Add(currentId);

            var children = allCommentsInPublication.Where(c => c.ParentCommentId == currentId);
            foreach (var child in children)
            {
                if (!descendantIds.Contains(child.Id))
                {
                    queue.Enqueue(child.Id);
                    descendants.Add(child);
                }
            }
        }

        var deletedCommentsCount = descendants.Count + 1; // plus the target comment itself

        // Soft delete the comment and all its descendants
        comment.MarkAsDeleted();
        foreach (var desc in descendants)
        {
            desc.MarkAsDeleted();
        }

        // Decrement parent replies count
        if (parentComment != null)
        {
            parentComment.DecrementRepliesCount();
        }

        // Decrement publication comments count
        if (publication != null)
        {
            publication.DecrementCommentsCount(deletedCommentsCount);
        }
    }
}
