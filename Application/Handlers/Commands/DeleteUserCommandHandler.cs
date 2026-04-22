using Application.Commands;
using Application.Interfaces;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Handlers.Commands;

public sealed class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, ErrorOr<Deleted>>
{
    private readonly IApplicationDbContext _context;

    public DeleteUserCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<Deleted>> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var userExists = await _context.Users.AnyAsync(u => u.Id == request.UserId, cancellationToken);
        if (!userExists)
        {
            return Error.NotFound(description: "User not found.");
        }

        // 1. Remove all User's Likes on comments
        await _context.CommentLikes
            .Where(l => l.UserId == request.UserId)
            .ExecuteDeleteAsync(cancellationToken);

        // 2. Remove all User's Assessments
        await _context.Assessments
            .Where(a => a.UserId == request.UserId)
            .ExecuteDeleteAsync(cancellationToken);

        // 3. Remove all User's Follows
        await _context.Follows
            .Where(f => f.FollowerId == request.UserId || f.FolloweeId == request.UserId)
            .ExecuteDeleteAsync(cancellationToken);

        // 4. Remove all relations between User's Publications and any Collections (including other users' collections)
        await _context.Database.ExecuteSqlRawAsync(
            "DELETE FROM CollectionPublications WHERE PublicationId IN (SELECT Id FROM Publications WHERE UserId = {0})", 
            request.UserId, 
            cancellationToken);

        // 5. Remove all relations inside User's Collections (many-to-many junction)
        await _context.Database.ExecuteSqlRawAsync(
            "DELETE FROM CollectionPublications WHERE CollectionId IN (SELECT Id FROM Collections WHERE UserId = {0})", 
            request.UserId, 
            cancellationToken);

        // 6. Remove User's Collections
        await _context.Collections
            .Where(c => c.UserId == request.UserId)
            .ExecuteDeleteAsync(cancellationToken);

        // 7. Cleanup after User's Publications (likes on their comments, comments themselves)
        await _context.CommentLikes
            .Where(l => l.Comment.Publication.UserId == request.UserId)
            .ExecuteDeleteAsync(cancellationToken);

        await DeleteUserCommentsByPublicationAsync(request.UserId, cancellationToken);
        await DeleteUserCommentsAsync(request.UserId, cancellationToken);

        // 8. Remove Clothes, Publications and the User itself
        await _context.Clothes
            .Where(c => c.UserId == request.UserId)
            .ExecuteDeleteAsync(cancellationToken);

        await _context.Publications
            .Where(p => p.UserId == request.UserId)
            .ExecuteDeleteAsync(cancellationToken);

        await _context.Users
            .Where(u => u.Id == request.UserId)
            .ExecuteDeleteAsync(cancellationToken);

        return Result.Deleted;
    }

    private async Task DeleteUserCommentsByPublicationAsync(Guid userId, CancellationToken cancellationToken)
    {
        while (true)
        {
            var deletedCount = await _context.Comments
                .Where(c => c.Publication.UserId == userId && !_context.Comments.Any(reply => reply.ParentCommentId == c.Id))
                .ExecuteDeleteAsync(cancellationToken);

            if (deletedCount == 0) break;
        }
    }

    private async Task DeleteUserCommentsAsync(Guid userId, CancellationToken cancellationToken)
    {
        while (true)
        {
            var deletedCount = await _context.Comments
                .Where(c => c.UserId == userId && !_context.Comments.Any(reply => reply.ParentCommentId == c.Id))
                .ExecuteDeleteAsync(cancellationToken);

            if (deletedCount == 0) break;
        }
    }
}
