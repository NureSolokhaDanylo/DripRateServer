using Application.Dtos;
using Application.Interfaces;
using Application.Queries.Games;
using Domain;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Handlers.Queries.Games;

internal sealed class GetTagMatchBatchQueryHandler : IRequestHandler<GetTagMatchBatchQuery, ErrorOr<List<TagMatchGameItemDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetTagMatchBatchQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<List<TagMatchGameItemDto>>> Handle(GetTagMatchBatchQuery request, CancellationToken cancellationToken)
    {
        var playedPublicationIds = await _context.UserGameHistories
            .Where(h => h.UserId == request.UserId && h.GameType == GameType.TagMatch)
            .Select(h => h.PublicationId)
            .ToListAsync(cancellationToken);

        var nextPublications = await _context.Publications
            .AsNoTracking()
            .Where(p => p.GameSettings.IsTagMatchEnabled && !playedPublicationIds.Contains(p.Id))
            .OrderBy(p => Guid.NewGuid())
            .Take(request.BatchSize)
            .Select(p => new
            {
                p.Id,
                User = new UserSimpleDto(p.User.Id, p.User.DisplayName, p.User.AvatarUrl),
                p.Images,
                Tags = p.Tags.Select(t => new TagResponse(t.Id, t.Name, t.Category)).ToList()
            })
            .ToListAsync(cancellationToken);

        if (!nextPublications.Any())
        {
            return new List<TagMatchGameItemDto>();
        }

        var allPossibleDistractors = await _context.Tags
            .AsNoTracking()
            .OrderBy(t => Guid.NewGuid())
            .Take(100)
            .Select(t => new TagResponse(t.Id, t.Name, t.Category))
            .ToListAsync(cancellationToken);

        var result = new List<TagMatchGameItemDto>();

        foreach (var p in nextPublications)
        {
            var correctTags = p.Tags;
            var correctTagIds = p.Tags.Select(t => t.Id).ToHashSet();
            
            var distractors = allPossibleDistractors
                .Where(t => !correctTagIds.Contains(t.Id))
                .OrderBy(t => Guid.NewGuid())
                .Take(3)
                .ToList();

            var combinedTags = correctTags.Concat(distractors).OrderBy(t => Guid.NewGuid()).ToList();

            result.Add(new TagMatchGameItemDto(
                p.Id,
                p.User,
                p.Images,
                combinedTags
            ));
        }

        return result;
    }
}
