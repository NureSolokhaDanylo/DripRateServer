using Application.Dtos;
using Application.Interfaces;
using Application.Queries;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Handlers.Queries;

public sealed class GetCommentsQueryHandler : IRequestHandler<GetCommentsQuery, ErrorOr<List<CommentResponse>>>
{
    private readonly IApplicationDbContext _context;

    public GetCommentsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<List<CommentResponse>>> Handle(GetCommentsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Comments
            .AsNoTracking()
            .Where(c => c.PublicationId == request.PublicationId);

        if (request.Cursor.HasValue)
        {
            query = query.Where(c => c.CreatedAt < request.Cursor.Value);
        }

        var result = await query
            .OrderByDescending(c => c.CreatedAt)
            .Take(request.Take)
            .Select(c => new CommentResponse(
                c.Id,
                c.Text,
                c.CreatedAt,
                c.UserId,
                c.User.UserName ?? string.Empty,
                c.User.AvatarUrl,
                c.Likes.Count,
                request.UserId.HasValue && c.Likes.Any(l => l.UserId == request.UserId.Value),
                c.ParentCommentId,
                c.Replies.Count
            ))
            .ToListAsync(cancellationToken);

        return result;
    }
}
