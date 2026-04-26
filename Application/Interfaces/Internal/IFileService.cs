using ErrorOr;

namespace Application.Interfaces.Internal;

internal interface IFileService
{
    Task<ErrorOr<string>> UploadAvatarAsync(
        Guid userId,
        Stream stream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken);

    Task<ErrorOr<string>> UploadPublicationImageAsync(
        Stream stream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken);

    Task<ErrorOr<string>> UploadClothPhotoAsync(
        Stream stream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken);
}
