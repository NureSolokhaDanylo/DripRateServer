using Application.Dtos;
using Application.Interfaces;
using Application.Queries.Search;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Handlers.Queries.Search;

public sealed class SearchPublicationsQueryHandler : IRequestHandler<SearchPublicationsQuery, ErrorOr<List<PublicationResponse>>>
{
    private readonly IApplicationDbContext _context;

    public SearchPublicationsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<List<PublicationResponse>>> Handle(SearchPublicationsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Publications.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.SearchQuery))
        {
            var search = request.SearchQuery.ToLower();
            query = query.Where(p => p.Description.ToLower().Contains(search));
        }

        if (request.TagIds != null && request.TagIds.Any())
        {
            query = query.Where(p => p.Tags.Any(t => request.TagIds.Contains(t.Id)));
        }

        if (request.Cursor.HasValue)
        {
            query = query.Where(p => p.CreatedAt < request.Cursor.Value);
        }

        var result = await query
            .OrderByDescending(p => p.CreatedAt)
            .ThenByDescending(p => p.Id)
            .Take(request.Take)
            .Select(PublicationResponse.GetProjection(request.UserId))
            .ToListAsync(cancellationToken);

        return result;
    }
}
