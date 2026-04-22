using Application.Dtos;
using Application.Interfaces;
using Application.Queries;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Handlers.Queries;

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
            .Take(request.Take)
            .Select(p => new PublicationResponse(
                p.Id,
                p.Description,
                p.Images.FirstOrDefault() ?? string.Empty,
                p.CreatedAt,
                p.UserId,
                p.User.UserName ?? string.Empty,
                p.Tags.Select(t => new TagResponse(t.Id, t.Name, t.Category)).ToList(),
                p.Clothes.Select(c => new ClothResponse(c.Id, c.Name, c.Brand, c.PhotoUrl)).ToList()
            ))
            .ToListAsync(cancellationToken);

        return result;
    }
}

public sealed class SearchCollectionsQueryHandler : IRequestHandler<SearchCollectionsQuery, ErrorOr<List<CollectionResponse>>>
{
    private readonly IApplicationDbContext _context;

    public SearchCollectionsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<List<CollectionResponse>>> Handle(SearchCollectionsQuery request, CancellationToken cancellationToken)
    {
        var search = request.SearchQuery.ToLower();

        var result = await _context.Collections
            .AsNoTracking()
            .Where(c => c.IsPublic && (c.Name.ToLower().Contains(search) || (c.Description != null && c.Description.ToLower().Contains(search))))
            .OrderByDescending(c => c.CreatedAt)
            .Take(request.Take)
            .Select(c => new CollectionResponse(
                c.Id,
                c.Name,
                c.Description,
                c.IsPublic,
                c.IsSystem,
                c.Publications.Count,
                c.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return result;
    }
}
