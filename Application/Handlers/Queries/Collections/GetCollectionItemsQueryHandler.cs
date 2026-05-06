using Application.Dtos;
using Application.Interfaces;
using Application.Queries.Collections;
using Domain.Errors;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Handlers.Queries.Collections;

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
