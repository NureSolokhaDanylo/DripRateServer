using Application.Dtos;
using Application.Interfaces;
using Application.Queries.Advertisements;
using Domain.Errors;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Handlers.Queries.Advertisements;

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
            ad.Url,
            ad.MaxImpressions,
            ad.ShownCount,
            ad.IsActive,
            ad.Tags.Select(t => new TagResponse(t.Id, t.Name, t.Category)).ToList(),
            ad.CreatedAt);
    }
}
