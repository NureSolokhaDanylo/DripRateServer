using Application.Dtos;
using Application.Interfaces;
using Application.Queries.Wardrobe;
using Domain.Errors;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Handlers.Queries.Wardrobe;

public sealed class GetClothByIdQueryHandler : IRequestHandler<GetClothByIdQuery, ErrorOr<ClothResponseDto>>
{
    private readonly IApplicationDbContext _context;

    public GetClothByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<ClothResponseDto>> Handle(GetClothByIdQuery request, CancellationToken cancellationToken)
    {
        var cloth = await _context.Clothes
            .AsNoTracking()
            .Where(c => c.Id == request.Id)
            .Select(c => new ClothResponseDto(
                c.Id,
                c.Name,
                c.Brand,
                c.PhotoUrl,
                c.StoreLink,
                c.EstimatedPrice))
            .FirstOrDefaultAsync(cancellationToken);

        if (cloth == null)
        {
            return ClothErrors.NotFound;
        }

        return cloth;
    }
}
