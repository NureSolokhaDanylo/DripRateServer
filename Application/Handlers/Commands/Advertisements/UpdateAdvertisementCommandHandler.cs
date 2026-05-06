using Application.Commands.Advertisements;
using Application.Dtos;
using Application.Interfaces;
using Application.Interfaces.Internal;
using Domain;
using Domain.Errors;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Handlers.Commands.Advertisements;

internal sealed class UpdateAdvertisementCommandHandler : IRequestHandler<UpdateAdvertisementCommand, ErrorOr<AdvertisementResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileService _fileService;
    private readonly IFileStorageService _storageService;

    public UpdateAdvertisementCommandHandler(IApplicationDbContext context, IFileService fileService, IFileStorageService storageService)
    {
        _context = context;
        _fileService = fileService;
        _storageService = storageService;
    }

    public async Task<ErrorOr<AdvertisementResponse>> Handle(UpdateAdvertisementCommand request, CancellationToken cancellationToken)
    {
        var ad = await _context.Advertisements
            .Include(a => a.Tags)
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (ad is null) return AdvertisementErrors.NotFound;

        var imagesToRemove = ad.Images.Except(request.ExistingImages).ToList();
        foreach (var imageUrl in imagesToRemove)
        {
            await _storageService.DeleteFileAsync(imageUrl, cancellationToken);
        }

        var finalImages = new List<string>(request.ExistingImages);

        if (request.NewImages != null)
        {
            foreach (var image in request.NewImages)
            {
                var uploadResult = await _fileService.UploadAdvertisementImageAsync(
                    image.OpenReadStream(),
                    image.FileName,
                    image.ContentType,
                    cancellationToken);

                if (uploadResult.IsError) return uploadResult.Errors;
                finalImages.Add(uploadResult.Value);
            }
        }

        ad.Update(request.Text, request.Url, request.MaxImpressions, finalImages, request.IsActive);

        ad.ClearTags();
        if (request.TagIds.Any())
        {
            var tags = await _context.Tags
                .Where(t => request.TagIds.Contains(t.Id))
                .ToListAsync(cancellationToken);
            
            foreach (var tag in tags) ad.AddTag(tag);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new AdvertisementResponse(
            ad.Id,
            ad.Images.ToList(),
            ad.Text,
            ad.Url,
            ad.MaxImpressions,
            ad.ShownCount,
            ad.IsActive,
            ad.Tags.Select(t => new TagResponse(t.Id, t.Name, t.Category)).ToList(),
            ad.CreatedAt);
    }
}
