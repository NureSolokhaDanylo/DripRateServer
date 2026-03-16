// using Microsoft.Extensions.Configuration;
// using Microsoft.Extensions.DependencyInjection;
// using SharedSettings.Options;

// namespace SharedSettings;

// public static class ServiceCollectionExtensions
// {
//     public static IServiceCollection AddConfiguration(this IServiceCollection services, IConfiguration configuration)
//     {
//         services
//             .Configure<Config>(configuration.GetSection(Config.SectionName))
//             .Configure<PasswordPolicyOptions>(configuration.GetSection(PasswordPolicyOptions.SectionName))
//             .Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

//         return services;
//     }
// }

