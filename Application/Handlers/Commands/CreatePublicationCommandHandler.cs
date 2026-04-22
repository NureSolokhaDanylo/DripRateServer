using Application.Commands;
using Application.Interfaces;
using Domain;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Handlers.Commands;

public sealed class CreatePublicationCommandHandler : IRequestHandler<CreatePublicationCommand, ErrorOr<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _storageService;

    public CreatePublicationCommandHandler(IApplicationDbContext context, IFileStorageService storageService)
    {
        _context = context;
        _storageService = storageService;
    }

    public async Task<ErrorOr<Guid>> Handle(CreatePublicationCommand command, CancellationToken cancellationToken)
    {
        // 1. Upload to Azure Blob Storage
        // Generating a unique name to avoid collisions
        var extension = Path.GetExtension(command.FileName);
        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        
        var uploadResult = await _storageService.UploadFileAsync(
            command.ImageStream, 
            command.ContentType, 
            uniqueFileName, 
            cancellationToken);

        if (uploadResult.IsError)
        {
            return uploadResult.Errors;
        }

        var imageUrl = uploadResult.Value;

        // 2. Create Publication Entity
        var publication = new Publication(command.UserId, command.Description, new[] { imageUrl });

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

            foreach (var cloth in clothes)
            {
                publication.AttachCloth(cloth);
            }
        }

        _context.Publications.Add(publication);
        await _context.SaveChangesAsync(cancellationToken);

        return publication.Id;
    }
}
