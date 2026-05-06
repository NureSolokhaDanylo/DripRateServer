using Application.Commands.Advertisements;
using Application.Dtos;
using Application.Interfaces;
using Application.Interfaces.Internal;
using Domain;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Handlers.Commands.Advertisements;

internal sealed class CreateAdvertisementCommandHandler : IRequestHandler<CreateAdvertisementCommand, ErrorOr<AdvertisementResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileService _fileService;

    public CreateAdvertisementCommandHandler(IApplicationDbContext context, IFileService fileService)
    {
        _context = context;
        _fileService = fileService;
    }

    public async Task<ErrorOr<AdvertisementResponse>> Handle(CreateAdvertisementCommand request, CancellationToken cancellationToken)
    {
        var imageUrls = new List<string>();
        foreach (var image in request.Images)
        {
            var uploadResult = await _fileService.UploadAdvertisementImageAsync(
                image.OpenReadStream(),
                image.FileName,
                image.ContentType,
                cancellationToken);

            if (uploadResult.IsError) return uploadResult.Errors;
            imageUrls.Add(uploadResult.Value);
        }

        var ad = new Advertisement(request.Text, request.Url, request.MaxImpressions, imageUrls);

        if (request.TagIds.Any())
        {
            var tags = await _context.Tags
                .Where(t => request.TagIds.Contains(t.Id))
                .ToListAsync(cancellationToken);
            
            foreach (var tag in tags) ad.AddTag(tag);
        }

        _context.Advertisements.Add(ad);
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
