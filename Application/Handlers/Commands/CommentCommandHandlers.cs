using Application.Commands;
using Application.Interfaces;
using Domain;
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
        if (!publicationExists) return Error.NotFound(description: "Publication not found.");

        if (request.ParentCommentId.HasValue)
        {
            var parentExists = await _context.Comments.AnyAsync(c => c.Id == request.ParentCommentId.Value, cancellationToken);
            if (!parentExists) return Error.NotFound(description: "Parent comment not found.");
        }

        var comment = new Comment(request.UserId, request.PublicationId, request.Text, request.ParentCommentId);
        _context.Comments.Add(comment);
        await _context.SaveChangesAsync(cancellationToken);

        return comment.Id;
    }
}

public sealed class ToggleCommentLikeCommandHandler : IRequestHandler<ToggleCommentLikeCommand, ErrorOr<bool>>
{
    private readonly IApplicationDbContext _context;

    public ToggleCommentLikeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<bool>> Handle(ToggleCommentLikeCommand request, CancellationToken cancellationToken)
    {
        var comment = await _context.Comments
            .Include(c => c.Likes)
            .FirstOrDefaultAsync(c => c.Id == request.CommentId, cancellationToken);

        if (comment == null) return Error.NotFound(description: "Comment not found.");

        comment.ToggleLike(request.UserId);
        await _context.SaveChangesAsync(cancellationToken);

        // Check if it's currently liked
        return comment.Likes.Any(l => l.UserId == request.UserId);
    }
}
