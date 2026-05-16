using Application.Commands.Games;
using Application.Dtos;
using Application.Interfaces;
using Domain;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Handlers.Commands.Games;

internal sealed class SubmitTagMatchBatchCommandHandler : IRequestHandler<SubmitTagMatchBatchCommand, ErrorOr<List<TagMatchResultResponse>>>
{
    private readonly IApplicationDbContext _context;

    public SubmitTagMatchBatchCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<List<TagMatchResultResponse>>> Handle(SubmitTagMatchBatchCommand request, CancellationToken cancellationToken)
    {
        if (request.Results == null || !request.Results.Any())
        {
            return new List<TagMatchResultResponse>();
        }

        var playedPublicationIds = await _context.UserGameHistories
            .Where(h => h.UserId == request.UserId && h.GameType == GameType.TagMatch)
            .Select(h => h.PublicationId)
            .ToListAsync(cancellationToken);

        var newHistories = new List<UserGameHistory>();
        var publicationIdsToUpdate = new List<Guid>();

        foreach (var result in request.Results)
        {
            if (playedPublicationIds.Contains(result.PublicationId) || newHistories.Any(h => h.PublicationId == result.PublicationId))
            {
                continue;
            }

            var history = new UserGameHistory(request.UserId, result.PublicationId, GameType.TagMatch);
            newHistories.Add(history);
            publicationIdsToUpdate.Add(result.PublicationId);
        }

        if (newHistories.Any())
        {
            _context.UserGameHistories.AddRange(newHistories);
        }

        var responseResults = new List<TagMatchResultResponse>();

        if (publicationIdsToUpdate.Any())
        {
            var publicationsWithTags = await _context.Publications
                .Include(p => p.Tags)
                .Where(p => publicationIdsToUpdate.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p.Tags.Select(t => t.Id).ToHashSet(), cancellationToken);

            var stats = await _context.GameStats
                .Where(s => publicationIdsToUpdate.Contains(s.PublicationId))
                .ToDictionaryAsync(s => s.PublicationId, cancellationToken);

            foreach (var result in request.Results)
            {
                if (publicationIdsToUpdate.Contains(result.PublicationId) && publicationsWithTags.TryGetValue(result.PublicationId, out var correctTagIds))
                {
                    if (!stats.TryGetValue(result.PublicationId, out var stat))
                    {
                        stat = new PublicationGameStats(result.PublicationId);
                        _context.GameStats.Add(stat);
                        stats[result.PublicationId] = stat;
                    }

                    int correctCount = result.TagIds.Count(id => correctTagIds.Contains(id));
                    stat.AddTagMatchResult(correctCount);

                    responseResults.Add(new TagMatchResultResponse(
                        result.PublicationId,
                        correctTagIds.ToList(),
                        result.TagIds.ToList(),
                        correctCount));
                }
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return responseResults;
    }
}
