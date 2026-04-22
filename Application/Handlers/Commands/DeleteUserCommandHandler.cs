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

        await _context.Likes
            .Where(l => l.UserId == request.UserId)
            .ExecuteDeleteAsync(cancellationToken);

        await _context.Assessments
            .Where(a => a.UserId == request.UserId)
            .ExecuteDeleteAsync(cancellationToken);

        await _context.Follows
            .Where(f => f.FollowerId == request.UserId || f.FolloweeId == request.UserId)
            .ExecuteDeleteAsync(cancellationToken);

        var userPublicationIds = await _context.Publications
            .Where(p => p.UserId == request.UserId)
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);

        foreach (var pubId in userPublicationIds)
        {
            await _context.Likes.Where(l => l.PublicationId == pubId).ExecuteDeleteAsync(cancellationToken);
            await _context.Assessments.Where(a => a.PublicationId == pubId).ExecuteDeleteAsync(cancellationToken);
        }

        await _context.Likes
            .Where(l => l.Comment != null && l.Comment.UserId == request.UserId)
            .ExecuteDeleteAsync(cancellationToken);

        await DeleteUserCommentsAsync(request.UserId, cancellationToken);

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
