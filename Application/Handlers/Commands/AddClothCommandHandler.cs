using Application.Commands;
using Application.Interfaces;
using Domain;
using ErrorOr;
using MediatR;

namespace Application.Handlers.Commands;

public sealed class AddClothCommandHandler : IRequestHandler<AddClothCommand, ErrorOr<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _storageService;

    public AddClothCommandHandler(IApplicationDbContext context, IFileStorageService storageService)
    {
        _context = context;
        _storageService = storageService;
    }

    public async Task<ErrorOr<Guid>> Handle(AddClothCommand request, CancellationToken cancellationToken)
    {
        string? photoUrl = null;

        if (request.PhotoStream != null && request.PhotoFileName != null && request.PhotoContentType != null)
        {
            var extension = Path.GetExtension(request.PhotoFileName);
            var uniqueFileName = $"cloth_{Guid.NewGuid()}{extension}";

            var uploadResult = await _storageService.UploadFileAsync(
                request.PhotoStream,
                request.PhotoContentType,
                uniqueFileName,
                cancellationToken);

            if (uploadResult.IsError)
            {
                return uploadResult.Errors;
            }

            photoUrl = uploadResult.Value;
        }

        var cloth = new Cloth(request.UserId, request.Name, request.Brand);
        cloth.UpdateInfo(request.Name, request.Brand, photoUrl, request.StoreLink, request.EstimatedPrice);

        _context.Clothes.Add(cloth);
        await _context.SaveChangesAsync(cancellationToken);

        return cloth.Id;
    }
}
