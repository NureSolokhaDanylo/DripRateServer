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
            .FirstOrDefaultAsync(c => c.Id == request.CommentId, cancellationToken);

        if (comment == null)
        {
            return CommentErrors.NotFound;
        }

        var existingLike = await _context.CommentLikes
            .FirstOrDefaultAsync(l => l.CommentId == request.CommentId && l.UserId == request.UserId, cancellationToken);

        bool isLikedNow;

        if (existingLike != null)
        {
            _context.CommentLikes.Remove(existingLike);
            comment.UpdateLikesCount(-1);
            isLikedNow = false;
        }
        else
        {
            _context.CommentLikes.Add(new CommentLike(request.UserId, request.CommentId));
            comment.UpdateLikesCount(1);
            isLikedNow = true;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return isLikedNow;
    }
}
