using Application.Interfaces;
using Domain.Errors;
using ErrorOr;
using Microsoft.Extensions.Configuration;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Infrastructure.Storage;

public sealed class AzureBlobStorageService : IFileStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName;

    public AzureBlobStorageService(IConfiguration configuration)
    {
        var connectionString = configuration["BlobStorage:ConnectionString"] 
            ?? throw new ArgumentNullException("BlobStorage:ConnectionString is missing");
        _containerName = configuration["BlobStorage:ContainerName"] 
            ?? throw new ArgumentNullException("BlobStorage:ContainerName is missing");
            
        _blobServiceClient = new BlobServiceClient(connectionString);
    }

    public async Task<ErrorOr<string>> UploadFileAsync(Stream stream, string contentType, string fileName, CancellationToken cancellationToken = default)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: cancellationToken);

            var blobClient = containerClient.GetBlobClient($"{Guid.NewGuid()}_{fileName}");
            
            var options = new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
            };

            await blobClient.UploadAsync(stream, options, cancellationToken);

            return blobClient.Uri.ToString();
        }
        catch (Exception)
        {
            return FileErrors.ProcessingFailed;
        }
    }

    public async Task DeleteFileAsync(string fileUrl, CancellationToken cancellationToken)
    {
        try
        {
            var uri = new Uri(fileUrl);
            var blobClient = new BlobClient(uri);
            await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: cancellationToken);
        }
        catch (Exception)
        {
            // Logging would be appropriate here
        }
    }
}
