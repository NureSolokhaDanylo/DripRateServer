using Application.Dtos;
using Application.Interfaces;
using Application.Queries.Games;
using Domain;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Handlers.Queries.Games;

internal sealed class GetFirstImpressionBatchQueryHandler : IRequestHandler<GetFirstImpressionBatchQuery, ErrorOr<List<FirstImpressionGameItemDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetFirstImpressionBatchQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<List<FirstImpressionGameItemDto>>> Handle(GetFirstImpressionBatchQuery request, CancellationToken cancellationToken)
    {
        var playedPublicationIds = await _context.UserGameHistories
            .Where(h => h.UserId == request.UserId && h.GameType == GameType.FirstImpression)
            .Select(h => h.PublicationId)
            .ToListAsync(cancellationToken);

        var result = await _context.Publications
            .AsNoTracking()
            .Where(p => p.GameSettings.IsFirstImpressionEnabled && !playedPublicationIds.Contains(p.Id))
            .OrderBy(p => Guid.NewGuid())
            .Take(request.BatchSize)
            .Select(p => new FirstImpressionGameItemDto(
                p.Id,
                new UserSimpleDto(p.User.Id, p.User.DisplayName, p.User.AvatarUrl),
                p.Images
            ))
            .ToListAsync(cancellationToken);

        return result;
    }
}
