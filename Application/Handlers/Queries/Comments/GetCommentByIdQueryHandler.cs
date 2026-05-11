using Application.Dtos;
using Application.Interfaces;
using Application.Queries.Comments;
using Domain.Errors;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Handlers.Queries.Comments;

public sealed class GetCommentByIdQueryHandler : IRequestHandler<GetCommentByIdQuery, ErrorOr<CommentResponse>>
{
    private readonly IApplicationDbContext _context;

    public GetCommentByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<CommentResponse>> Handle(GetCommentByIdQuery request, CancellationToken cancellationToken)
    {
        var comment = await _context.Comments
            .AsNoTracking()
            .Where(c => c.Id == request.CommentId)
            .Select(CommentResponse.GetProjection(request.UserId))
            .FirstOrDefaultAsync(cancellationToken);

        if (comment is null)
        {
            return CommentErrors.NotFound;
        }

        return comment;
    }
}
