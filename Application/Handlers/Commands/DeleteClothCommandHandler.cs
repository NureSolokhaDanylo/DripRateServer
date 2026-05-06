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
    private readonly IFileStorageService _storageService;

    public DeleteClothCommandHandler(IApplicationDbContext context, IFileStorageService storageService)
    {
        _context = context;
        _storageService = storageService;
    }

    public async Task<ErrorOr<Deleted>> Handle(DeleteClothCommand request, CancellationToken cancellationToken)
    {
        var clothResult = await _context.Clothes.GetOwnedOrErrorAsync(
            request.ClothId, 
            request.UserId, 
            ClothErrors.Forbidden, 
            cancellationToken);
            
        if (clothResult.IsError) return clothResult.Errors;

        var cloth = clothResult.Value;

        if (cloth.PhotoUrl != null)
        {
            await _storageService.DeleteFileAsync(cloth.PhotoUrl, cancellationToken);
        }
        
        _context.Clothes.Remove(cloth);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Deleted;
    }
}
