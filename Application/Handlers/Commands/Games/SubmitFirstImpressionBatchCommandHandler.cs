using Application.Commands.Games;
using Application.Interfaces;
using Domain;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Handlers.Commands.Games;

internal sealed class SubmitFirstImpressionBatchCommandHandler : IRequestHandler<SubmitFirstImpressionBatchCommand, ErrorOr<Success>>
{
    private readonly IApplicationDbContext _context;
    public SubmitFirstImpressionBatchCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<Success>> Handle(SubmitFirstImpressionBatchCommand request, CancellationToken cancellationToken)
    {
        if (request.Results == null || !request.Results.Any())
        {
            return Result.Success;
        }

        var playedPublicationIds = await _context.UserGameHistories
            .Where(h => h.UserId == request.UserId && h.GameType == GameType.FirstImpression)
            .Select(h => h.PublicationId)
            .ToListAsync(cancellationToken);

        var newHistories = new List<UserGameHistory>();
        
        // Gather valid unique results to process for stats
        var publicationIdsToUpdate = new List<Guid>();

        foreach (var result in request.Results)
        {
            // Protect against duplicates in the request itself or previously submitted
            if (playedPublicationIds.Contains(result.PublicationId) || newHistories.Any(h => h.PublicationId == result.PublicationId))
            {
                continue;
            }

            var history = new UserGameHistory(request.UserId, result.PublicationId, GameType.FirstImpression);
            newHistories.Add(history);
            
            if (PublicationGameStats.IsValidReactionTime(result.ReactionTimeMs))
            {
                publicationIdsToUpdate.Add(result.PublicationId);
            }
        }

        if (newHistories.Any())
        {
            _context.UserGameHistories.AddRange(newHistories);
        }

        if (publicationIdsToUpdate.Any())
        {
            var stats = await _context.GameStats
                .Where(s => publicationIdsToUpdate.Contains(s.PublicationId))
                .ToDictionaryAsync(s => s.PublicationId, cancellationToken);

            foreach (var result in request.Results)
            {
                if (publicationIdsToUpdate.Contains(result.PublicationId))
                {
                    if (!stats.TryGetValue(result.PublicationId, out var stat))
                    {
                        stat = new PublicationGameStats(result.PublicationId);
                        _context.GameStats.Add(stat);
                        stats[result.PublicationId] = stat;
                    }
                    
                    stat.AddFirstImpressionResult(result.IsPositive, result.ReactionTimeMs);
                }
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}
