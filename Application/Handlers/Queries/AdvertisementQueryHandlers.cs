using Application.Dtos;
using Application.Interfaces;
using Application.Queries;
using Domain.Errors;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Handlers.Queries;

public sealed class GetAdvertisementQueryHandler : IRequestHandler<GetAdvertisementQuery, ErrorOr<AdvertisementResponse>>
{
    private readonly IApplicationDbContext _context;

    public GetAdvertisementQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<AdvertisementResponse>> Handle(GetAdvertisementQuery request, CancellationToken cancellationToken)
    {
        var ad = await _context.Advertisements
            .AsNoTracking()
            .Include(a => a.Tags)
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (ad is null) return AdvertisementErrors.NotFound;

        return new AdvertisementResponse(
            ad.Id,
            ad.Images.ToList(),
            ad.Text,
            ad.MaxImpressions,
            ad.ShownCount,
            ad.IsActive,
            ad.Tags.Select(t => new TagResponse(t.Id, t.Name, t.Category)).ToList(),
            ad.CreatedAt);
    }
}

public sealed class GetAdvertisementsQueryHandler : IRequestHandler<GetAdvertisementsQuery, ErrorOr<List<AdvertisementResponse>>>
{
    private readonly IApplicationDbContext _context;

    public GetAdvertisementsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<List<AdvertisementResponse>>> Handle(GetAdvertisementsQuery request, CancellationToken cancellationToken)
    {
        var ads = await _context.Advertisements
            .AsNoTracking()
            .Include(a => a.Tags)
            .OrderByDescending(a => a.CreatedAt)
            .Skip(request.Skip)
            .Take(request.Take)
            .Select(a => new AdvertisementResponse(
                a.Id,
                a.Images.ToList(),
                a.Text,
                a.MaxImpressions,
                a.ShownCount,
                a.IsActive,
                a.Tags.Select(t => new TagResponse(t.Id, t.Name, t.Category)).ToList(),
                a.CreatedAt))
            .ToListAsync(cancellationToken);

        return ads;
    }
}
