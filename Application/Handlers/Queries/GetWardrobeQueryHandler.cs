using Application.Dtos;
using Application.Interfaces;
using Application.Queries;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Handlers.Queries;

public sealed class GetWardrobeQueryHandler : IRequestHandler<GetWardrobeQuery, ErrorOr<List<ClothResponseDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetWardrobeQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<List<ClothResponseDto>>> Handle(GetWardrobeQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Clothes
            .AsNoTracking()
            .Where(c => c.UserId == request.UserId);

        if (!string.IsNullOrWhiteSpace(request.SearchQuery))
        {
            var search = request.SearchQuery.ToLower();
            query = query.Where(c => 
                c.Name.ToLower().Contains(search) || 
                (c.Brand != null && c.Brand.ToLower().Contains(search)));
        }

        query = request.SortBy?.ToLower() switch
        {
            "price_asc" => query.OrderBy(c => c.EstimatedPrice).ThenBy(c => c.Name),
            "price_desc" => query.OrderByDescending(c => c.EstimatedPrice).ThenBy(c => c.Name),
            "newest" => query.OrderByDescending(c => c.CreatedAt).ThenBy(c => c.Id),
            "oldest" => query.OrderBy(c => c.CreatedAt).ThenBy(c => c.Id),
            _ => query.OrderBy(c => c.Name).ThenBy(c => c.Id)
        };

        var result = await query
            .Skip(request.Skip)
            .Take(request.Take)
            .Select(c => new ClothResponseDto(
                c.Id,
                c.Name,
                c.Brand,
                c.PhotoUrl,
                c.StoreLink,
                c.EstimatedPrice))
            .ToListAsync(cancellationToken);

        return result;
    }
}
