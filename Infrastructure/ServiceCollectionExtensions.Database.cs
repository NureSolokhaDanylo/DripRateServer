using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharedSettings;
using SharedSettings.Options;

namespace Infrastructure;

public static partial class ServiceCollectionExtensions
{
    private static IServiceCollection AddDatabaseConfiguration(this IServiceCollection services)
    {
        var configuration = SharedConfigurationBuilder.Build();
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        services.AddDbContext<MyDbContext>(options =>
            options.UseSqlServer(
                connectionString,
                sql => sql
                    .MigrationsAssembly(typeof(MyDbContext).Assembly.FullName)
                    .EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null)));

        services.Configure<PasswordPolicyOptions>(configuration.GetSection(PasswordPolicyOptions.SectionName));

        var passwordPolicySection = configuration.GetSection(PasswordPolicyOptions.SectionName);
        var passwordPolicy = passwordPolicySection.Get<PasswordPolicyOptions>() 
            ?? new PasswordPolicyOptions();

        services
            .AddIdentityCore<IdentityUser>(options =>
            {
                options.Password.RequiredLength = passwordPolicy.PasswordMinLength;
                options.Password.RequireDigit = passwordPolicy.PasswordRequireDigit;
                options.Password.RequireLowercase = passwordPolicy.PasswordRequireLowercase;
                options.Password.RequireUppercase = passwordPolicy.PasswordRequireUppercase;
                options.Password.RequireNonAlphanumeric = passwordPolicy.PasswordRequireNonAlphanumeric;
                options.Lockout.MaxFailedAccessAttempts = passwordPolicy.LockoutMaxFailedAccessAttempts;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(passwordPolicy.LockoutDefaultLockoutTimeSpanMinutes);
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<MyDbContext>();

        return services;
    }
}
