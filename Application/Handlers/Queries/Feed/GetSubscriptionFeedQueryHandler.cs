using Application.Dtos;
using Application.Interfaces;
using Application.Queries.Feed;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SharedSettings.Options;

namespace Application.Handlers.Queries.Feed;

public sealed class GetSubscriptionFeedQueryHandler : IRequestHandler<GetSubscriptionFeedQuery, ErrorOr<List<PublicationResponse>>>
{
    private readonly IApplicationDbContext _context;

    public GetSubscriptionFeedQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<List<PublicationResponse>>> Handle(GetSubscriptionFeedQuery request, CancellationToken cancellationToken)
    {
        var followingIds = await _context.Follows
            .Where(f => f.FollowerId == request.UserId)
            .Select(f => f.FolloweeId)
            .ToListAsync(cancellationToken);

        var query = _context.Publications
            .AsNoTracking()
            .Where(p => followingIds.Contains(p.UserId));

        if (request.Cursor.HasValue)
        {
            query = query.Where(p => p.CreatedAt < request.Cursor.Value);
        }

        var result = await query
            .OrderByDescending(p => p.CreatedAt)
            .ThenByDescending(p => p.Id)
            .Take(request.Take)
            .Select(PublicationResponse.GetProjection(request.UserId))
            .ToListAsync(cancellationToken);

        return result;
    }
}
