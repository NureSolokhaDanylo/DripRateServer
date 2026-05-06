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
            .Where(p => p.GameSettings.IsGuessPriceEnabled && !playedPublicationIds.Contains(p.Id));

        var nextPublications = await nextPublicationsQuery
            .OrderBy(p => Guid.NewGuid())
            .Take(request.BatchSize)
            .ToListAsync(cancellationToken);

        var result = nextPublications
            .Select(p => new GuessPriceGameItemDto(
                p.Id,
                new UserSimpleDto(p.User.Id, p.User.DisplayName, p.User.AvatarUrl),
                p.Images,
                p.GameSnapshotPrice
            ))
            .ToList();

        return result;
    }
}
