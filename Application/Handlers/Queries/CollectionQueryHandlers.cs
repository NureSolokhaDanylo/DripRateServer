using Application.Dtos;
using Application.Interfaces;
using Application.Queries;
using Domain;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Handlers.Queries;

public sealed class GetMyCollectionsQueryHandler : IRequestHandler<GetMyCollectionsQuery, ErrorOr<List<CollectionResponse>>>
{
    private readonly IApplicationDbContext _context;

    public GetMyCollectionsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<List<CollectionResponse>>> Handle(GetMyCollectionsQuery request, CancellationToken cancellationToken)
    {
        var result = await _context.Collections
            .AsNoTracking()
            .Where(c => c.UserId == request.UserId)
            .OrderByDescending(c => c.IsSystem)
            .ThenByDescending(c => c.CreatedAt)
            .Select(CollectionResponse.Projection)
            .ToListAsync(cancellationToken);

        return result;
    }
}

public sealed class GetCollectionItemsQueryHandler : IRequestHandler<GetCollectionItemsQuery, ErrorOr<List<PublicationResponse>>>
{
    private readonly IApplicationDbContext _context;

    public GetCollectionItemsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<List<PublicationResponse>>> Handle(GetCollectionItemsQuery request, CancellationToken cancellationToken)
    {
        var collection = await _context.Collections
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.CollectionId, cancellationToken);

        if (collection == null) return Error.NotFound(description: "Collection not found.");

        if (!collection.IsPublic && collection.UserId != request.UserId)
        {
            return Error.Forbidden(description: "This collection is private.");
        }

        // We need to fetch publications that belong to this collection with cursor pagination.
        // EF Core can join directly through the many-to-many relationship.
        var query = _context.Publications
            .AsNoTracking()
            .Where(p => p.Collections.Any(c => c.Id == request.CollectionId));

        if (request.Cursor.HasValue)
        {
            query = query.Where(p => p.CreatedAt < request.Cursor.Value);
        }

        var result = await query
            .OrderByDescending(p => p.CreatedAt)
            .Take(request.Take)
            .Select(PublicationResponse.Projection)
            .ToListAsync(cancellationToken);

        return result;
    }
}
