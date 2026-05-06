using Application.Commands.Advertisements;
using Domain.Errors;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Application.Interfaces;

namespace Application.Handlers.Commands.Advertisements;

internal sealed class ToggleAdvertisementActiveCommandHandler : IRequestHandler<ToggleAdvertisementActiveCommand, ErrorOr<Success>>
{
    private readonly IApplicationDbContext _context;

    public ToggleAdvertisementActiveCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<Success>> Handle(ToggleAdvertisementActiveCommand request, CancellationToken cancellationToken)
    {
        var ad = await _context.Advertisements
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (ad is null) return AdvertisementErrors.NotFound;

        if (!ad.SetStatus(request.IsActive))
        {
            return AdvertisementErrors.LimitReached;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success;
    }
}
