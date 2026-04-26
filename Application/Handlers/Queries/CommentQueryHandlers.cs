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
        var followingIds = request.UserId.HasValue 
            ? await _context.Follows
                .Where(f => f.FollowerId == request.UserId.Value)
                .Select(f => f.FolloweeId)
                .ToListAsync(cancellationToken)
            : new List<Guid>();

        var query = _context.Comments
            .AsNoTracking()
            .Where(c => c.PublicationId == request.PublicationId && c.ParentCommentId == request.ParentCommentId);

        if (request.Cursor.HasValue)
        {
            query = query.Where(c => c.CreatedAt < request.Cursor.Value);
        }

        var result = await query
            .OrderByDescending(c => followingIds.Contains(c.UserId))
            .ThenByDescending(c => c.LikesCount)
            .ThenByDescending(c => c.CreatedAt)
            .ThenByDescending(c => c.Id)
            .Take(request.Take)
            .Select(CommentResponse.GetProjection(request.UserId))
            .ToListAsync(cancellationToken);

        return result;
    }
}
