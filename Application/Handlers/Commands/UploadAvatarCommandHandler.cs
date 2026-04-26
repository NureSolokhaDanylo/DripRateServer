using Application.Commands;
using Application.Extensions;
using Application.Interfaces;
using Application.Interfaces.Internal;
using Domain.Errors;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SharedSettings.Options;

namespace Application.Handlers.Commands;

internal sealed class UploadAvatarCommandHandler : IRequestHandler<UploadAvatarCommand, ErrorOr<Updated>>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileService _fileService;
    private readonly IFileStorageService _storageService;
    private readonly BlobStorageOptions _blobOptions;

    public UploadAvatarCommandHandler(
        IApplicationDbContext context, 
        IFileService fileService,
        IFileStorageService storageService,
        IOptions<BlobStorageOptions> blobOptions)
    {
        _context = context;
        _fileService = fileService;
        _storageService = storageService;
        _blobOptions = blobOptions.Value;
    }

    public async Task<ErrorOr<Updated>> Handle(UploadAvatarCommand request, CancellationToken cancellationToken)
    {
        var userResult = await _context.Users.GetByIdOrErrorAsync(request.UserId, UserErrors.NotFound, cancellationToken);
        if (userResult.IsError) return userResult.Errors;

        var user = userResult.Value;
        var oldAvatarUrl = user.AvatarUrl;

        // Upload to storage
        var uploadResult = await _fileService.UploadAvatarAsync(
            user.Id,
            request.ImageStream, 
            request.FileName, 
            request.ContentType, 
            cancellationToken);

        if (uploadResult.IsError)
        {
            return uploadResult.Errors;
        }

        user.UpdateAvatar(uploadResult.Value);
        await _context.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrEmpty(oldAvatarUrl) && oldAvatarUrl != _blobOptions.DefaultAvatarUrl)
        {
            await _storageService.DeleteFileAsync(oldAvatarUrl, cancellationToken);
        }

        return Result.Updated;
    }
}
