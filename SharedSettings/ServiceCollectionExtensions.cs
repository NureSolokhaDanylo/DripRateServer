using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharedSettings.Options;

namespace SharedSettings;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddConfiguration(this IServiceCollection services, IConfiguration config)
    {
        services
            .Configure<Config>(config.GetSection(Config.SectionName));

        return services;
    }
}

