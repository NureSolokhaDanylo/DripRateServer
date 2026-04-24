using Application.Commands;
using Application.Extensions;
using Application.Interfaces;
using Application.Interfaces.Internal;
using Domain.Errors;
using ErrorOr;
using MediatR;

namespace Application.Handlers.Commands;

internal sealed class DeleteClothCommandHandler : IRequestHandler<DeleteClothCommand, ErrorOr<Deleted>>
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
        var clothResult = await _context.Clothes.GetOwnedOrErrorAsync(
            request.ClothId, 
            request.UserId, 
            ClothErrors.Forbidden, 
            cancellationToken);
            
        if (clothResult.IsError) return clothResult.Errors;

        await _deletionService.DeleteClothContentAsync(request.ClothId, cancellationToken);
        
        _context.Clothes.Remove(clothResult.Value);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Deleted;
    }
}
