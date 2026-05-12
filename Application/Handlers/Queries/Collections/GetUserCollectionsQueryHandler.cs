using Application.Dtos;
using Application.Interfaces;
using Application.Queries.Collections;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Handlers.Queries.Collections;

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
            .Select(c => new CollectionResponse(c.Id, c.Name, c.Description, c.IsPublic, c.IsSystem, c.CollectionPublications.Count, c.CreatedAt))
            .ToListAsync(cancellationToken);

        return collections;
    }
}
