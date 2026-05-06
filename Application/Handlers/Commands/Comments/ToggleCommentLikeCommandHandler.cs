using Application.Commands.Comments;
using Application.Interfaces;
using Application.Interfaces.Internal;
using Domain;
using Domain.Errors;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Application.Commands.Comments;

namespace Application.Handlers.Commands.Comments;

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

        if (comment == null)
        {
            return CommentErrors.NotFound;
        }

        comment.ToggleLike(request.UserId);
        await _context.SaveChangesAsync(cancellationToken);

        return comment.Likes.Any(l => l.UserId == request.UserId);
    }
}
