using Application.Dtos;
using Application.Interfaces;
using Application.Queries;
using Domain;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Handlers.Queries;

internal sealed class GetGuessPriceBatchQueryHandler : IRequestHandler<GetGuessPriceBatchQuery, ErrorOr<List<GuessPriceGameItemDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetGuessPriceBatchQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<List<GuessPriceGameItemDto>>> Handle(GetGuessPriceBatchQuery request, CancellationToken cancellationToken)
    {
        var playedPublicationIds = await _context.UserGameHistories
            .Where(h => h.UserId == request.UserId && h.GameType == GameType.GuessPrice)
            .Select(h => h.PublicationId)
            .ToListAsync(cancellationToken);

        var nextPublicationsQuery = _context.Publications
            .Include(p => p.User)
            .Include(p => p.Clothes)
            .Where(p => p.GameSettings.IsGuessPriceEnabled && !playedPublicationIds.Contains(p.Id));

        var nextPublications = await nextPublicationsQuery
            .OrderBy(p => Guid.NewGuid())
            .Take(request.BatchSize * 2) // Take more to filter out 0 price items in memory if needed, though DB can also do it.
            .ToListAsync(cancellationToken);

        var result = nextPublications
            .Select(p => new
            {
                Publication = p,
                RealPrice = p.Clothes.Sum(c => c.EstimatedPrice ?? 0)
            })
            .Where(x => x.RealPrice > 0)
            .Take(request.BatchSize)
            .Select(x => new GuessPriceGameItemDto(
                x.Publication.Id,
                new UserSimpleDto(x.Publication.User.Id, x.Publication.User.DisplayName, x.Publication.User.AvatarUrl),
                x.Publication.Images,
                x.RealPrice
            ))
            .ToList();

        return result;
    }
}
