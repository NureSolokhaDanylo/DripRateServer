using ErrorOr;

namespace Application.Interfaces;

public interface IFileStorageService
{
    Task<ErrorOr<string>> UploadFileAsync(Stream content, string contentType, string fileName, CancellationToken cancellationToken = default);
    Task DeleteFileAsync(string fileUrl, CancellationToken cancellationToken = default);
}
