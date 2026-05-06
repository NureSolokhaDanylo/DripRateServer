using Application.Commands;
using Application.Interfaces;
using Application.Interfaces.Internal;
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
        var publication = await _context.Publications.FirstOrDefaultAsync(p => p.Id == request.PublicationId, cancellationToken);
        if (publication == null) return PublicationErrors.NotFound;

        Comment? parentComment = null;
        if (request.ParentCommentId.HasValue)
        {
            parentComment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == request.ParentCommentId.Value, cancellationToken);
            if (parentComment == null) return CommentErrors.ParentNotFound;
        }

        var comment = new Comment(request.UserId, request.PublicationId, request.Text, request.ParentCommentId);
        
        if (parentComment != null)
        {
            parentComment.AddReply(comment);
        }
        
        publication.AddComment(comment);
        
        await _context.SaveChangesAsync(cancellationToken);

        return comment.Id;
    }
}

internal sealed class DeleteCommentCommandHandler : IRequestHandler<DeleteCommentCommand, ErrorOr<Deleted>>
{
    private readonly IApplicationDbContext _context;
    private readonly IDeletionService _deletionService;

    public DeleteCommentCommandHandler(IApplicationDbContext context, IDeletionService deletionService)
    {
        _context = context;
        _deletionService = deletionService;
    }

    public async Task<ErrorOr<Deleted>> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = await _context.Comments
            .AsNoTracking()
            .Where(c => c.Id == request.CommentId)
            .Select(c => new
            {
                c.Id,
                c.UserId
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (comment == null) return CommentErrors.NotFound;

        if (comment.UserId != request.UserId)
        {
            return CommentErrors.Forbidden;
        }

        await _deletionService.DeleteCommentAsync(request.CommentId, cancellationToken);

        return Result.Deleted;
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

        if (comment == null)
        {
            return CommentErrors.NotFound;
        }

        comment.ToggleLike(request.UserId);
        await _context.SaveChangesAsync(cancellationToken);

        return comment.Likes.Any(l => l.UserId == request.UserId);
    }
}
