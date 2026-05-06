using Application.Dtos;
using Application.Interfaces;
using Application.Queries.Feed;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SharedSettings.Options;

namespace Application.Handlers.Queries.Feed;

public sealed class GetGlobalFeedQueryHandler : IRequestHandler<GetGlobalFeedQuery, ErrorOr<GlobalFeedResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly FeedOptions _feedOptions;

    public GetGlobalFeedQueryHandler(IApplicationDbContext context, IOptions<FeedOptions> feedOptions)
    {
        _context = context;
        _feedOptions = feedOptions.Value;
    }

    public async Task<ErrorOr<GlobalFeedResponse>> Handle(GetGlobalFeedQuery request, CancellationToken cancellationToken)
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

        var publications = await query
            .OrderByDescending(p => p.CreatedAt)
            .ThenByDescending(p => p.Id)
            .Take(request.Take)
            .Select(PublicationResponse.GetProjection(request.UserId))
            .ToListAsync(cancellationToken);

        // Fetch Advertisements
        var advertisements = new List<AdvertisementResponse>();
        int adsToFetch = publications.Count / _feedOptions.AdFrequency;

        if (adsToFetch > 0)
        {
            var cutoffTime = DateTimeOffset.UtcNow.AddHours(-_feedOptions.ViewCooldownHours);

            // First, try to fetch ads with matching tags
            var matchingAds = await _context.Advertisements
                .AsNoTracking()
                .Where(a => a.IsActive)
                .Where(a => a.Tags.Any(t => preferredTagIds.Contains(t.Id)))
                .Select(a => new 
                { 
                    Ad = a, 
                    View = a.Views.FirstOrDefault(v => v.UserId == request.UserId) 
                })
                .OrderBy(x => x.View == null || x.View.ViewedAt < cutoffTime ? 0 : 1)
                .ThenBy(x => Guid.NewGuid()) // Random selection within priority
                .Take(adsToFetch)
                .Select(x => new AdvertisementResponse(
                    x.Ad.Id,
                    x.Ad.Images.ToList(),
                    x.Ad.Text,
                    x.Ad.Url,
                    x.Ad.MaxImpressions,
                    x.Ad.ShownCount,
                    x.Ad.IsActive,
                    x.Ad.Tags.Select(t => new TagResponse(t.Id, t.Name, t.Category)).ToList(),
                    x.Ad.CreatedAt))
                .ToListAsync(cancellationToken);

            advertisements.AddRange(matchingAds);

            // If not enough matching ads, fill with random active ads
            if (advertisements.Count < adsToFetch)
            {
                var existingAdIds = advertisements.Select(a => a.Id).ToList();
                int remaining = adsToFetch - advertisements.Count;

                var randomAds = await _context.Advertisements
                    .AsNoTracking()
                    .Where(a => a.IsActive)
                    .Where(a => !existingAdIds.Contains(a.Id))
                    .Select(a => new 
                    { 
                        Ad = a, 
                        View = a.Views.FirstOrDefault(v => v.UserId == request.UserId) 
                    })
                    .OrderBy(x => x.View == null || x.View.ViewedAt < cutoffTime ? 0 : 1)
                    .ThenBy(x => Guid.NewGuid())
                    .Take(remaining)
                    .Select(x => new AdvertisementResponse(
                        x.Ad.Id,
                        x.Ad.Images.ToList(),
                        x.Ad.Text,
                        x.Ad.Url,
                        x.Ad.MaxImpressions,
                        x.Ad.ShownCount,
                        x.Ad.IsActive,
                        x.Ad.Tags.Select(t => new TagResponse(t.Id, t.Name, t.Category)).ToList(),
                        x.Ad.CreatedAt))                    .ToListAsync(cancellationToken);

                advertisements.AddRange(randomAds);
            }
        }

        return new GlobalFeedResponse(publications, advertisements);
    }
}
