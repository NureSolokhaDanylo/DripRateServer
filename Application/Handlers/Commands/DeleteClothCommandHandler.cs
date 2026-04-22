using Application.Commands;
using Application.Interfaces;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Handlers.Commands;

public sealed class DeleteClothCommandHandler : IRequestHandler<DeleteClothCommand, ErrorOr<Deleted>>
{
    private readonly IApplicationDbContext _context;

    public DeleteClothCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<Deleted>> Handle(DeleteClothCommand request, CancellationToken cancellationToken)
    {
        var cloth = await _context.Clothes
            .FirstOrDefaultAsync(c => c.Id == request.ClothId && c.UserId == request.UserId, cancellationToken);

        if (cloth == null)
        {
            return Error.NotFound(description: "Cloth item not found in your wardrobe.");
        }

        // Rule 8: Manual cleanup of many-to-many junction table "PublicationClothes"
        await _context.Database.ExecuteSqlRawAsync(
            "DELETE FROM PublicationClothes WHERE ClothesId = {0}",
            request.ClothId,
            cancellationToken);

        _context.Clothes.Remove(cloth);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Deleted;
    }
}
