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

        var result = await query
            .OrderByDescending(c => c.Name) // Default sort
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
