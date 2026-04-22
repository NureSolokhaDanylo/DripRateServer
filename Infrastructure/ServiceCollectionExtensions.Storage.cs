using Application.Interfaces;
using Infrastructure.Storage;
using Microsoft.Extensions.DependencyInjection;
using SharedSettings;
using SharedSettings.Options;

namespace Infrastructure;

public static partial class ServiceCollectionExtensions
{
    private static IServiceCollection AddStorageConfiguration(this IServiceCollection services)
    {
        var blobOptions = SharedConfiguration.GetIOptions2<BlobStorageOptions>();
        
        services.Configure<BlobStorageOptions>(options =>
        {
            options.ConnectionString = blobOptions.ConnectionString;
            options.ContainerName = blobOptions.ContainerName;
        });

        services.AddScoped<IFileStorageService, AzureBlobStorageService>();

        return services;
    }
}
