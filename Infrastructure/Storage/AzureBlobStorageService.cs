using Application.Interfaces;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using ErrorOr;
using Microsoft.Extensions.Options;
using SharedSettings.Options;

namespace Infrastructure.Storage;

public sealed class AzureBlobStorageService : IFileStorageService
{
    private readonly BlobStorageOptions _options;
    private readonly BlobServiceClient _blobServiceClient;

    public AzureBlobStorageService(IOptions<BlobStorageOptions> options)
    {
        _options = options.Value;
        _blobServiceClient = new BlobServiceClient(_options.ConnectionString);
    }

    public async Task<ErrorOr<string>> UploadFileAsync(
        Stream content,
        string contentType,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_options.ContainerName);
            
            // Note: In production, you might want to ensure the container exists once during startup 
            // rather than checking on every upload, but for development, this is safer.
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: cancellationToken);

            var blobClient = containerClient.GetBlobClient(fileName);

            var blobUploadOptions = new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
            };

            await blobClient.UploadAsync(content, blobUploadOptions, cancellationToken);

            return blobClient.Uri.ToString();
        }
        catch (Exception)
        {
            // Log exception here if a logger was available
            return Error.Failure("FileStorage.UploadFailed", "Failed to upload file to Azure Blob Storage.");
        }
    }
}
