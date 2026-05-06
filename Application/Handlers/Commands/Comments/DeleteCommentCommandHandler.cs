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
