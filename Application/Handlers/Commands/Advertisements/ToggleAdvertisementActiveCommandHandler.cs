using Application.Commands.Advertisements;
using Application.Dtos;
using Domain.Errors;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Application.Interfaces;

namespace Application.Handlers.Commands.Advertisements;

internal sealed class ToggleAdvertisementActiveCommandHandler : IRequestHandler<ToggleAdvertisementActiveCommand, ErrorOr<AdvertisementResponse>>
{
    private readonly IApplicationDbContext _context;

    public ToggleAdvertisementActiveCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<AdvertisementResponse>> Handle(ToggleAdvertisementActiveCommand request, CancellationToken cancellationToken)
    {
        var ad = await _context.Advertisements
            .Include(a => a.Tags)
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (ad is null) return AdvertisementErrors.NotFound;

        if (!ad.SetStatus(request.IsActive))
        {
            return AdvertisementErrors.LimitReached;
        }

        await _context.SaveChangesAsync(cancellationToken);
        
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
