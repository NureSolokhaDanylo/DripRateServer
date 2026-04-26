using Application.Dtos;
using Application.Interfaces;
using Application.Queries;
using Domain.Errors;
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
        var collections = await _context.Collections
            .AsNoTracking()
            .Where(c => c.UserId == request.UserId)
            .OrderByDescending(c => c.CreatedAt)
            .ThenBy(c => c.Id)
            .Skip(request.Skip)
            .Take(request.Take)
            .Select(c => new CollectionResponse(c.Id, c.Name, c.Description, c.IsPublic, c.IsSystem, c.Publications.Count, c.CreatedAt))
            .ToListAsync(cancellationToken);

        return collections;
    }
}

public sealed class GetUserCollectionsQueryHandler : IRequestHandler<GetUserCollectionsQuery, ErrorOr<List<CollectionResponse>>>
{
    private readonly IApplicationDbContext _context;

    public GetUserCollectionsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<List<CollectionResponse>>> Handle(GetUserCollectionsQuery request, CancellationToken cancellationToken)
    {
        var collections = await _context.Collections
            .AsNoTracking()
            .Where(c => c.UserId == request.UserId && c.IsPublic && !c.IsSystem)
            .OrderByDescending(c => c.CreatedAt)
            .ThenBy(c => c.Id)
            .Skip(request.Skip)
            .Take(request.Take)
            .Select(c => new CollectionResponse(c.Id, c.Name, c.Description, c.IsPublic, c.IsSystem, c.Publications.Count, c.CreatedAt))
            .ToListAsync(cancellationToken);

        return collections;
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

        if (collection == null) return CollectionErrors.NotFound;

        if (!collection.IsPublic && collection.UserId != request.UserId)
        {
            return CollectionErrors.Forbidden;
        }

        var query = _context.Collections
            .AsNoTracking()
            .Where(c => c.Id == request.CollectionId)
            .SelectMany(c => c.Publications);

        if (request.Cursor.HasValue)
        {
            query = query.Where(p => p.CreatedAt < request.Cursor.Value);
        }

        var result = await query
            .OrderByDescending(p => p.CreatedAt)
            .Take(request.Take)
            .Select(PublicationResponse.GetProjection(request.UserId))
            .ToListAsync(cancellationToken);

        return result;
    }
}
