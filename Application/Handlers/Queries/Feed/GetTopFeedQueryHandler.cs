using Application.Dtos;
using Application.Interfaces;
using Application.Queries.Feed;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SharedSettings.Options;

namespace Application.Handlers.Queries.Feed;

public sealed class GetTopFeedQueryHandler : IRequestHandler<GetTopFeedQuery, ErrorOr<List<PublicationResponse>>>
{
    private readonly IApplicationDbContext _context;

    public GetTopFeedQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<List<PublicationResponse>>> Handle(GetTopFeedQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Publications.AsNoTracking();

        // Time filter
        var now = DateTimeOffset.UtcNow;
        query = request.Period switch
        {
            TopFeedPeriod.Weekly => query.Where(p => p.CreatedAt >= now.AddDays(-7)),
            TopFeedPeriod.Monthly => query.Where(p => p.CreatedAt >= now.AddDays(-30)),
            _ => query
        };

        // Scope filter (Friends/Following)
        if (request.OnlyFollowing)
        {
            var followingIds = await _context.Follows
                .Where(f => f.FollowerId == request.UserId)
                .Select(f => f.FolloweeId)
                .ToListAsync(cancellationToken);

            query = query.Where(p => followingIds.Contains(p.UserId));
        }

        // Tags filter
        if (request.TagIds != null && request.TagIds.Any())
        {
            query = query.Where(p => p.Tags.Any(t => request.TagIds.Contains(t.Id)));
        }

        var result = await query
            .OrderByDescending(p => p.AverageRating)
            .ThenByDescending(p => p.AssessmentsCount)
            .ThenByDescending(p => p.Id)
            .Skip(request.Skip)
            .Take(request.Take)
            .Select(PublicationResponse.GetProjection(request.UserId))
            .ToListAsync(cancellationToken);

        return result;
    }
}
