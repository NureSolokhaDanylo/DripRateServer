using Application.Dtos;
using Application.Interfaces;
using Application.Queries;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Handlers.Queries;

public sealed class GetGlobalFeedQueryHandler : IRequestHandler<GetGlobalFeedQuery, ErrorOr<List<PublicationResponse>>>
{
    private readonly IApplicationDbContext _context;

    public GetGlobalFeedQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<List<PublicationResponse>>> Handle(GetGlobalFeedQuery request, CancellationToken cancellationToken)
    {
        var preferredTagIds = await _context.Users
            .Where(u => u.Id == request.UserId)
            .SelectMany(u => u.PreferredTags)
            .Select(t => t.Id)
            .ToListAsync(cancellationToken);

        var query = _context.Publications.AsNoTracking();

        // Filter by user preferences if any
        if (preferredTagIds.Any())
        {
            query = query.Where(p => p.Tags.Any(t => preferredTagIds.Contains(t.Id)));
        }

        // Cursor pagination
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

public sealed class GetUserFeedQueryHandler : IRequestHandler<GetUserFeedQuery, ErrorOr<List<PublicationResponse>>>
{
    private readonly IApplicationDbContext _context;

    public GetUserFeedQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<List<PublicationResponse>>> Handle(GetUserFeedQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Publications
            .AsNoTracking()
            .Where(p => p.UserId == request.UserId);

        if (request.Cursor.HasValue)
        {
            query = query.Where(p => p.CreatedAt < request.Cursor.Value);
        }

        var result = await query
            .OrderByDescending(p => p.CreatedAt)
            .ThenByDescending(p => p.Id)
            .Take(request.Take)
            .Select(PublicationResponse.GetProjection(request.CurrentUserId))
            .ToListAsync(cancellationToken);

        return result;
    }
}

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
