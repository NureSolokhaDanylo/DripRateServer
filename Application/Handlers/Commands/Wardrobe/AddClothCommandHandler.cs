using Application.Commands.Wardrobe;
using Application.Interfaces;
using Application.Interfaces.Internal;
using Domain;
using ErrorOr;
using MediatR;

namespace Application.Handlers.Commands.Wardrobe;

internal sealed class AddClothCommandHandler : IRequestHandler<AddClothCommand, ErrorOr<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileService _fileService;

    public AddClothCommandHandler(IApplicationDbContext context, IFileService fileService)
    {
        _context = context;
        _fileService = fileService;
    }

    public async Task<ErrorOr<Guid>> Handle(AddClothCommand request, CancellationToken cancellationToken)
    {
        string? photoUrl = null;

        // 1. Upload photo if provided
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

            photoUrl = uploadResult.Value;
        }

        // 2. Create and configure Cloth entity
        var cloth = new Cloth(request.UserId, request.Name, request.Brand);
        cloth.UpdateInfo(request.Name, request.Brand, photoUrl, request.StoreLink, request.EstimatedPrice);

        // 3. Save to database
        _context.Clothes.Add(cloth);
        await _context.SaveChangesAsync(cancellationToken);

        return cloth.Id;
    }
}
