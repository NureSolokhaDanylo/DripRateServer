using Application.Commands.Advertisements;
using Domain.Errors;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Application.Interfaces;

namespace Application.Handlers.Commands.Advertisements;

internal sealed class DeleteAdvertisementCommandHandler : IRequestHandler<DeleteAdvertisementCommand, ErrorOr<Deleted>>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _storageService;

    public DeleteAdvertisementCommandHandler(IApplicationDbContext context, IFileStorageService storageService)
    {
        _context = context;
        _storageService = storageService;
    }

    public async Task<ErrorOr<Deleted>> Handle(DeleteAdvertisementCommand request, CancellationToken cancellationToken)
    {
        var ad = await _context.Advertisements
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (ad is null) return AdvertisementErrors.NotFound;

        foreach (var imageUrl in ad.Images)
        {
            await _storageService.DeleteFileAsync(imageUrl, cancellationToken);
        }

        _context.Advertisements.Remove(ad);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Deleted;
    }
}
