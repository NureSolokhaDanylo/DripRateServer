using Application.Dtos;
using Application.Interfaces;
using Application.Queries.Collections;
using Domain.Errors;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Handlers.Queries.Collections;

public sealed class GetCollectionItemsV2QueryHandler : IRequestHandler<GetCollectionItemsV2Query, ErrorOr<List<PublicationResponse>>>
{
    private readonly IApplicationDbContext _context;

    public GetCollectionItemsV2QueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<List<PublicationResponse>>> Handle(GetCollectionItemsV2Query request, CancellationToken cancellationToken)
    {
        var collection = await _context.Collections
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.CollectionId, cancellationToken);

        if (collection == null) return CollectionErrors.NotFound;

        if (!collection.IsPublic && collection.UserId != request.UserId)
        {
            return CollectionErrors.Forbidden;
        }

        var result = await _context.CollectionPublications
            .AsNoTracking()
            .Where(cp => cp.CollectionId == request.CollectionId)
            .OrderByDescending(cp => cp.AddedAt)
            .Skip(request.Skip)
            .Take(request.Take)
            .Select(cp => cp.Publication)
            .Select(PublicationResponse.GetProjection(request.UserId))
            .ToListAsync(cancellationToken);

        return result;
    }
}
