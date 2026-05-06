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
