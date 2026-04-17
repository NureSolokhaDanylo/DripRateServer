using Domain;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SharedSettings;
using SharedSettings.Options;

namespace Infrastructure;

public static partial class ServiceCollectionExtensions
{
    private static IServiceCollection AddDatabaseConfiguration(this IServiceCollection services)
    {
        var configuration = SharedConfiguration.GetConfiguration();
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        services.AddDbContext<MyDbContext>((serviceProvider, options) =>
        {
            var logger = serviceProvider.GetRequiredService<ILogger<LoggingSqlServerRetryingExecutionStrategy>>();
            
            options.UseSqlServer(
                connectionString,
                sql => sql
                    .MigrationsAssembly(typeof(MyDbContext).Assembly.FullName)
                    .ExecutionStrategy(dependencies => new LoggingSqlServerRetryingExecutionStrategy(
                        dependencies,
                        maxRetryCount: int.MaxValue, // Бесконечно (как ты и просил)
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null,
                        logger: logger)));
        });

        services.AddScoped<DatabaseInitializer>();

        var passwordPolicy = SharedConfiguration.GetIOptions2<PasswordPolicyOptions>();
        services.Configure<PasswordPolicyOptions>(_ =>
        {
            _.PasswordMinLength = passwordPolicy.PasswordMinLength;
            _.PasswordRequireDigit = passwordPolicy.PasswordRequireDigit;
            _.PasswordRequireLowercase = passwordPolicy.PasswordRequireLowercase;
            _.PasswordRequireUppercase = passwordPolicy.PasswordRequireUppercase;
            _.PasswordRequireNonAlphanumeric = passwordPolicy.PasswordRequireNonAlphanumeric;
            _.LockoutMaxFailedAccessAttempts = passwordPolicy.LockoutMaxFailedAccessAttempts;
            _.LockoutDefaultLockoutTimeSpanMinutes = passwordPolicy.LockoutDefaultLockoutTimeSpanMinutes;
        });

        services
            .AddIdentityCore<User>(options =>
            {
                options.Password.RequiredLength = passwordPolicy.PasswordMinLength;
                options.Password.RequireDigit = passwordPolicy.PasswordRequireDigit;
                options.Password.RequireLowercase = passwordPolicy.PasswordRequireLowercase;
                options.Password.RequireUppercase = passwordPolicy.PasswordRequireUppercase;
                options.Password.RequireNonAlphanumeric = passwordPolicy.PasswordRequireNonAlphanumeric;
                options.Lockout.MaxFailedAccessAttempts = passwordPolicy.LockoutMaxFailedAccessAttempts;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(passwordPolicy.LockoutDefaultLockoutTimeSpanMinutes);
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<MyDbContext>();

        return services;
    }
}
