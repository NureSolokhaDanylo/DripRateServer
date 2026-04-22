using Application.Commands;
using Application.Interfaces;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Handlers.Commands;

public sealed class DeletePublicationCommandHandler : IRequestHandler<DeletePublicationCommand, ErrorOr<Deleted>>
{
    private readonly IApplicationDbContext _context;

    public DeletePublicationCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<Deleted>> Handle(DeletePublicationCommand request, CancellationToken cancellationToken)
    {
        var pub = await _context.Publications.FindAsync(new object[] { request.PublicationId }, cancellationToken);
        if (pub == null)
        {
            return Error.NotFound(description: "Publication not found.");
        }

        if (pub.UserId != request.UserId)
        {
            return Error.Forbidden(description: "You do not have permission to delete this publication.");
        }

        // Remove publication from all collections (many-to-many relationship)
        // EF Core doesn't directly expose the junction table in DbSet, so we can't use ExecuteDeleteAsync on it directly easily without raw SQL.
        // However, we can use raw SQL for the junction table or just let the Publication deletion handle it if it were Cascade.
        // Given Rule 8 (Restrict), we MUST manually delete entries from "CollectionPublications"
        await _context.Database.ExecuteSqlRawAsync(
            "DELETE FROM CollectionPublications WHERE PublicationId = {0}", 
            request.PublicationId, 
            cancellationToken);

        await _context.Assessments
            .Where(a => a.PublicationId == request.PublicationId)
            .ExecuteDeleteAsync(cancellationToken);

        await _context.CommentLikes
            .Where(l => l.Comment.PublicationId == request.PublicationId)
            .ExecuteDeleteAsync(cancellationToken);

        while (true)
        {
            var deletedCount = await _context.Comments
                .Where(c => c.PublicationId == request.PublicationId && !_context.Comments.Any(reply => reply.ParentCommentId == c.Id))
                .ExecuteDeleteAsync(cancellationToken);

            if (deletedCount == 0) break;
        }

        _context.Publications.Remove(pub);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Deleted;
    }
}
