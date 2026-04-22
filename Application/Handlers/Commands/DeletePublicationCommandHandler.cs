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
        var publicationExists = await _context.Publications.AnyAsync(p => p.Id == request.PublicationId, cancellationToken);
        if (!publicationExists)
        {
            return Error.NotFound(description: "Publication not found.");
        }

        await _context.Likes
            .Where(l => l.PublicationId == request.PublicationId)
            .ExecuteDeleteAsync(cancellationToken);

        await _context.Assessments
            .Where(a => a.PublicationId == request.PublicationId)
            .ExecuteDeleteAsync(cancellationToken);

        await _context.Likes
            .Where(l => l.Comment != null && l.Comment.PublicationId == request.PublicationId)
            .ExecuteDeleteAsync(cancellationToken);

        while (true)
        {
            var deletedCount = await _context.Comments
                .Where(c => c.PublicationId == request.PublicationId && !_context.Comments.Any(reply => reply.ParentCommentId == c.Id))
                .ExecuteDeleteAsync(cancellationToken);

            if (deletedCount == 0) break;
        }

        var pub = await _context.Publications.FindAsync(new object[] { request.PublicationId }, cancellationToken);
        if (pub != null)
        {
            _context.Publications.Remove(pub);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return Result.Deleted;
    }
}
