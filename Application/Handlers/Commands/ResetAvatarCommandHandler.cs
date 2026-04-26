using Application.Commands;
using Application.Extensions;
using Application.Interfaces;
using Domain.Errors;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Options;
using SharedSettings.Options;

namespace Application.Handlers.Commands;

internal sealed class ResetAvatarCommandHandler : IRequestHandler<ResetAvatarCommand, ErrorOr<Updated>>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _storageService;
    private readonly BlobStorageOptions _blobOptions;

    public ResetAvatarCommandHandler(
        IApplicationDbContext context, 
        IFileStorageService storageService,
        IOptions<BlobStorageOptions> blobOptions)
    {
        _context = context;
        _storageService = storageService;
        _blobOptions = blobOptions.Value;
    }

    public async Task<ErrorOr<Updated>> Handle(ResetAvatarCommand request, CancellationToken cancellationToken)
    {
        var userResult = await _context.Users.GetByIdOrErrorAsync(request.UserId, UserErrors.NotFound, cancellationToken);
        if (userResult.IsError) return userResult.Errors;

        var user = userResult.Value;
        var oldAvatarUrl = user.AvatarUrl;
        
        user.UpdateAvatar(_blobOptions.DefaultAvatarUrl);
        
        await _context.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrEmpty(oldAvatarUrl) && oldAvatarUrl != _blobOptions.DefaultAvatarUrl)
        {
            await _storageService.DeleteFileAsync(oldAvatarUrl, cancellationToken);
        }

        return Result.Updated;
    }
}
