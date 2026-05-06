using Application.Dtos;
using Application.Interfaces;
using Application.Queries.Advertisements;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Handlers.Queries.Advertisements;

public sealed class GetAdvertisementsQueryHandler : IRequestHandler<GetAdvertisementsQuery, ErrorOr<List<AdvertisementResponse>>>
{
    private readonly IApplicationDbContext _context;

    public GetAdvertisementsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<List<AdvertisementResponse>>> Handle(GetAdvertisementsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Advertisements
            .AsNoTracking()
            .Include(a => a.Tags)
            .AsQueryable();

        if (request.IsActive.HasValue)
        {
            query = query.Where(a => a.IsActive == request.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(a => a.Text.ToLower().Contains(search) || a.Url.ToLower().Contains(search));
        }

        var ads = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip(request.Skip)
            .Take(request.Take)
            .Select(a => new AdvertisementResponse(
                a.Id,
                a.Images.ToList(),
                a.Text,
                a.Url,
                a.MaxImpressions,
                a.ShownCount,
                a.IsActive,
                a.Tags.Select(t => new TagResponse(t.Id, t.Name, t.Category)).ToList(),
                a.CreatedAt))
            .ToListAsync(cancellationToken);

        return ads;
    }
}
