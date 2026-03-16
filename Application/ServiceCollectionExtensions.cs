using Microsoft.Extensions.DependencyInjection;

namespace Application.Handlers;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<RegisterCommandHandler>();
        services.AddScoped<LoginQueryHandler>();

        return services;
    }
}
