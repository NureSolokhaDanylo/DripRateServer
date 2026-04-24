using Application.Commands;
using Application.Extensions;
using Application.Interfaces;
using Application.Interfaces.Internal;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Handlers.Commands;

public sealed class DeleteClothCommandHandler : IRequestHandler<DeleteClothCommand, ErrorOr<Deleted>>
{
    private readonly IApplicationDbContext _context;
    private readonly IDeletionService _deletionService;

    public DeleteClothCommandHandler(IApplicationDbContext context, IDeletionService deletionService)
    {
        _context = context;
        _deletionService = deletionService;
    }

    public async Task<ErrorOr<Deleted>> Handle(DeleteClothCommand request, CancellationToken cancellationToken)
    {
        var clothResult = await _context.Clothes.GetOwnedOrErrorAsync(request.ClothId, request.UserId, cancellationToken);
        if (clothResult.IsError) return clothResult.Errors;

        var cloth = clothResult.Value;

        // Rule 8: Delegate manual cleanup to internal service
        await _deletionService.DeleteClothContentAsync(request.ClothId, cancellationToken);

        _context.Clothes.Remove(cloth);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Deleted;
    }
}
