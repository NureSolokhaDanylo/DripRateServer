using Application.Commands;
using Application.Dtos;
using Application.Interfaces;
using Application.Interfaces.Internal;
using Domain;
using Domain.Errors;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SharedSettings.Options;

namespace Application.Handlers.Commands;

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

        var ad = new Advertisement(request.Text, request.MaxImpressions, imageUrls);

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
            ad.MaxImpressions,
            ad.ShownCount,
            ad.Tags.Select(t => new TagResponse(t.Id, t.Name, t.Category)).ToList(),
            ad.CreatedAt);
    }
}

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

        // Physical deletion of removed images
        var imagesToRemove = ad.Images.Except(request.ExistingImages).ToList();
        foreach (var imageUrl in imagesToRemove)
        {
            await _storageService.DeleteFileAsync(imageUrl, cancellationToken);
        }

        var finalImages = new List<string>(request.ExistingImages);

        // Upload new images
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

        ad.Update(request.Text, request.MaxImpressions, finalImages);

        // Update tags
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
            ad.MaxImpressions,
            ad.ShownCount,
            ad.Tags.Select(t => new TagResponse(t.Id, t.Name, t.Category)).ToList(),
            ad.CreatedAt);
    }
}

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

internal sealed class ViewAdvertisementCommandHandler : IRequestHandler<ViewAdvertisementCommand, ErrorOr<Success>>
{
    private readonly IApplicationDbContext _context;
    private readonly FeedOptions _feedOptions;

    public ViewAdvertisementCommandHandler(IApplicationDbContext context, IOptions<FeedOptions> feedOptions)
    {
        _context = context;
        _feedOptions = feedOptions.Value;
    }

    public async Task<ErrorOr<Success>> Handle(ViewAdvertisementCommand request, CancellationToken cancellationToken)
    {
        var ad = await _context.Advertisements
            .FirstOrDefaultAsync(a => a.Id == request.AdId, cancellationToken);

        if (ad is null) return AdvertisementErrors.NotFound;

        var view = await _context.AdvertisementViews
            .FirstOrDefaultAsync(v => v.AdvertisementId == request.AdId && v.UserId == request.UserId, cancellationToken);

        if (view != null)
        {
            if ((DateTimeOffset.UtcNow - view.ViewedAt).TotalHours < _feedOptions.ViewCooldownHours)
            {
                return Result.Success;
            }
            view.UpdateViewedAt();
        }
        else
        {
            view = new AdvertisementView(request.AdId, request.UserId);
            _context.AdvertisementViews.Add(view);
        }

        ad.IncrementShownCount();

        try 
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return Result.Success;
        }

        return Result.Success;
    }
}
