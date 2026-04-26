using Application.Commands;
using Application.Extensions;
using Application.Interfaces;
using Application.Interfaces.Internal;
using Domain.Errors;
using ErrorOr;
using MediatR;

namespace Application.Handlers.Commands;

internal sealed class UpdateClothCommandHandler : IRequestHandler<UpdateClothCommand, ErrorOr<Success>>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileService _fileService;
    private readonly IFileStorageService _storageService;

    public UpdateClothCommandHandler(IApplicationDbContext context, IFileService fileService, IFileStorageService storageService)
    {
        _context = context;
        _fileService = fileService;
        _storageService = storageService;
    }

    public async Task<ErrorOr<Success>> Handle(UpdateClothCommand request, CancellationToken cancellationToken)
    {
        var clothResult = await _context.Clothes.GetOwnedOrErrorAsync(
            request.ClothId,
            request.UserId,
            ClothErrors.Forbidden,
            cancellationToken);

        if (clothResult.IsError) return clothResult.Errors;

        var cloth = clothResult.Value;
        string? photoUrl = cloth.PhotoUrl;

        if (request.PhotoStream != null && request.PhotoFileName != null && request.PhotoContentType != null)
        {
            var uploadResult = await _fileService.UploadClothPhotoAsync(
                request.PhotoStream,
                request.PhotoFileName,
                request.PhotoContentType,
                cancellationToken);

            if (uploadResult.IsError)
            {
                return uploadResult.Errors;
            }

            if (cloth.PhotoUrl != null)
            {
                await _storageService.DeleteFileAsync(cloth.PhotoUrl, cancellationToken);
            }

            photoUrl = uploadResult.Value;
        }

        cloth.UpdateInfo(request.Name, request.Brand, photoUrl, request.StoreLink, request.EstimatedPrice);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}
