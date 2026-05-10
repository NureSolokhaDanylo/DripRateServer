using Application.Dtos;
using Application.Interfaces;
using Application.Queries.Search;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Handlers.Queries.Search;

public sealed class SearchCollectionsQueryHandler : IRequestHandler<SearchCollectionsQuery, ErrorOr<List<CollectionResponse>>>
{
    private readonly IApplicationDbContext _context;

    public SearchCollectionsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<List<CollectionResponse>>> Handle(SearchCollectionsQuery request, CancellationToken cancellationToken)
    {
        var search = request.SearchQuery?.Trim().ToLowerInvariant();

        var query = _context.Collections
            .AsNoTracking()
            .Where(c => c.IsPublic);

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(c => c.Name.ToLower().Contains(search) || (c.Description != null && c.Description.ToLower().Contains(search)));
        }

        var result = await query
            .OrderByDescending(c => c.CreatedAt)
            .ThenBy(c => c.Id)
            .Skip(request.Skip)
            .Take(request.Take)
            .Select(CollectionResponse.Projection)
            .ToListAsync(cancellationToken);

        return result;
    }
}
