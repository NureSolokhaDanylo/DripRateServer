using Application.Commands.Advertisements;
using Domain;
using Domain.Errors;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SharedSettings.Options;
using Application.Interfaces;

namespace Application.Handlers.Commands.Advertisements;

internal sealed class ViewAdvertisementCommandHandler : IRequestHandler<ViewAdvertisementCommand, ErrorOr<Success>>
{
    private readonly IApplicationDbContext _context;
    private readonly FeedOptions _feedOptions;

    public ViewAdvertisementCommandHandler(IApplicationDbContext context, IOptions<FeedOptions> feedOptions)
    {
        _context = context;
        _feedOptions = feedOptions.Value;
    }

    public async Task<ErrorOr<Success>> Handle(ViewAdvertisementCommand request, CancellationToken cancellationToken)
    {
        var ad = await _context.Advertisements
            .FirstOrDefaultAsync(a => a.Id == request.AdId, cancellationToken);

        if (ad is null) return AdvertisementErrors.NotFound;

        var view = await _context.AdvertisementViews
            .FirstOrDefaultAsync(v => v.AdvertisementId == request.AdId && v.UserId == request.UserId, cancellationToken);

        if (view != null)
        {
            if ((DateTimeOffset.UtcNow - view.ViewedAt).TotalHours < _feedOptions.ViewCooldownHours)
            {
                return Result.Success;
            }
            view.UpdateViewedAt();
        }
        else
        {
            view = new AdvertisementView(request.AdId, request.UserId);
            _context.AdvertisementViews.Add(view);
        }

        await _context.Advertisements
            .Where(a => a.Id == request.AdId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(a => a.ShownCount, a => a.ShownCount + 1)
                .SetProperty(a => a.IsActive, a => (a.ShownCount + 1) < a.MaxImpressions),
            cancellationToken);

        try 
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return Result.Success;
        }

        return Result.Success;
    }
}
