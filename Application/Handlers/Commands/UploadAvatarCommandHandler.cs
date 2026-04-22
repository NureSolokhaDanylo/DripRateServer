using Application.Commands;
using Application.Interfaces;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Handlers.Commands;

public sealed class UploadAvatarCommandHandler : IRequestHandler<UploadAvatarCommand, ErrorOr<Updated>>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _storageService;

    public UploadAvatarCommandHandler(IApplicationDbContext context, IFileStorageService storageService)
    {
        _context = context;
        _storageService = storageService;
    }

    public async Task<ErrorOr<Updated>> Handle(UploadAvatarCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            return Error.NotFound(description: "User not found.");
        }

        // Upload to storage
        var extension = Path.GetExtension(request.FileName);
        var uniqueFileName = $"avatars/{user.Id}/{Guid.NewGuid()}{extension}";
        
        var uploadResult = await _storageService.UploadFileAsync(
            request.ImageStream, 
            request.ContentType, 
            uniqueFileName, 
            cancellationToken);

        if (uploadResult.IsError)
        {
            return uploadResult.Errors;
        }

        user.UpdateAvatar(uploadResult.Value);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Updated;
    }
}
