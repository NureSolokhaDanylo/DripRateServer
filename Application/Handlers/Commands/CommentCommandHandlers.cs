using Application.Commands;
using Application.Interfaces;
using Domain;
using Domain.Errors;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Handlers.Commands;

public sealed class CreateCommentCommandHandler : IRequestHandler<CreateCommentCommand, ErrorOr<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateCommentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<Guid>> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
    {
        var publicationExists = await _context.Publications.AnyAsync(p => p.Id == request.PublicationId, cancellationToken);
        if (!publicationExists) return PublicationErrors.NotFound;

        if (request.ParentCommentId.HasValue)
        {
            var parentExists = await _context.Comments.AnyAsync(c => c.Id == request.ParentCommentId.Value, cancellationToken);
            if (!parentExists) return CommentErrors.ParentNotFound;
        }

        var comment = new Comment(request.UserId, request.PublicationId, request.Text, request.ParentCommentId);
        _context.Comments.Add(comment);
        await _context.SaveChangesAsync(cancellationToken);

        return comment.Id;
    }
}

public sealed class DeleteCommentCommandHandler : IRequestHandler<DeleteCommentCommand, ErrorOr<Deleted>>
{
    private readonly IApplicationDbContext _context;

    public DeleteCommentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<Deleted>> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == request.CommentId, cancellationToken);

        if (comment == null) return CommentErrors.NotFound;

        if (comment.UserId != request.UserId)
        {
            return CommentErrors.Forbidden;
        }

        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Deleted;
    }
}
