using Application.Interfaces;
using Application.Interfaces.Internal;
using ErrorOr;

namespace Application.Services;

internal sealed class FileService : IFileService
{
    private readonly IFileStorageService _storageService;

    public FileService(IFileStorageService storageService)
    {
        _storageService = storageService;
    }

    public async Task<ErrorOr<string>> UploadAvatarAsync(
        Guid userId,
        Stream stream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(fileName);
        var uniqueFileName = $"avatars/{userId}/{Guid.NewGuid()}{extension}";
        
        return await _storageService.UploadFileAsync(
            stream, 
            contentType, 
            uniqueFileName, 
            cancellationToken);
    }

    public async Task<ErrorOr<string>> UploadPublicationImageAsync(
        Stream stream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(fileName);
        var uniqueFileName = $"publications/{Guid.NewGuid()}{extension}";
        
        return await _storageService.UploadFileAsync(
            stream, 
            contentType, 
            uniqueFileName, 
            cancellationToken);
    }
}
