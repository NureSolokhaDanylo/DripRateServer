using Application.Commands;
using Application.Extensions;
using Application.Interfaces;
using Application.Interfaces.Internal;
using Domain;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Handlers.Commands;

internal sealed class CreatePublicationCommandHandler : IRequestHandler<CreatePublicationCommand, ErrorOr<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileService _fileService;
    private readonly IFileStorageService _fileStorageService;

    public CreatePublicationCommandHandler(
        IApplicationDbContext context,
        IFileService fileService,
        IFileStorageService fileStorageService)
    {
        _context = context;
        _fileService = fileService;
        _fileStorageService = fileStorageService;
    }

    public async Task<ErrorOr<Guid>> Handle(CreatePublicationCommand command, CancellationToken cancellationToken)
    {
        var imageUrls = new List<string>();
        var cleanupCompleted = false;

        try
        {
            // 1. Upload all images to Storage
            foreach (var image in command.Images)
            {
                var uploadResult = await _fileService.UploadPublicationImageAsync(
                    image.Stream,
                    image.FileName,
                    image.ContentType,
                    cancellationToken);

                if (uploadResult.IsError)
                {
                    await CleanupUploadedImagesAsync(imageUrls, cancellationToken);
                    cleanupCompleted = true;
                    return uploadResult.Errors;
                }

                imageUrls.Add(uploadResult.Value);
            }

            // 2. Create Publication Entity with all images
            var publication = new Publication(command.UserId, command.Description, imageUrls);

            // 3. Attach Tags
            if (command.TagIds != null && command.TagIds.Any())
            {
                var tags = await _context.Tags
                    .Where(t => command.TagIds.Contains(t.Id))
                    .ToListAsync(cancellationToken);

                foreach (var tag in tags)
                {
                    publication.AddTag(tag);
                }
            }

            // 4. Attach Clothes
            if (command.ClothIds != null && command.ClothIds.Any())
            {
                var clothes = await _context.Clothes
                    .Where(c => command.ClothIds.Contains(c.Id) && c.UserId == command.UserId)
                    .ToListAsync(cancellationToken);

                if (clothes.Count != command.ClothIds.Count)
                {
                    await CleanupUploadedImagesAsync(imageUrls, cancellationToken);
                    cleanupCompleted = true;
                    return Domain.Errors.ClothErrors.NotFound;
                }

                foreach (var cloth in clothes)
                {
                    publication.AttachCloth(cloth);
                }
            }

            _context.Publications.Add(publication);
            await _context.SaveChangesAsync(cancellationToken);

            return publication.Id;
        }
        catch
        {
            if (!cleanupCompleted)
            {
                await CleanupUploadedImagesAsync(imageUrls, cancellationToken);
            }
            throw;
        }
    }

    private async Task CleanupUploadedImagesAsync(IEnumerable<string> imageUrls, CancellationToken cancellationToken)
    {
        foreach (var imageUrl in imageUrls)
        {
            await _fileStorageService.DeleteFileAsync(imageUrl, cancellationToken);
        }
    }
}
