using Application.Dtos;
using Application.Interfaces;
using Application.Queries.Games;
using Domain;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Handlers.Queries.Games;

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

        var result = await _context.Publications
            .AsNoTracking()
            .Where(p => p.GameSettings.IsGuessPriceEnabled && !playedPublicationIds.Contains(p.Id))
            .OrderBy(p => Guid.NewGuid())
            .Take(request.BatchSize)
            .Select(p => new GuessPriceGameItemDto(
                p.Id,
                new UserSimpleDto(p.User.Id, p.User.DisplayName, p.User.AvatarUrl),
                p.Images,
                p.GameSnapshotPrice
            ))
            .ToListAsync(cancellationToken);

        return result;
    }
}
