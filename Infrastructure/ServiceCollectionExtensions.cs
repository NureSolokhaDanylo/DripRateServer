using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SharedSettings;
using System.Text;

namespace Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        var configuration = SharedConfigurationBuilder.Build();
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        services.AddDbContext<MyDbContext>(options =>
            options.UseSqlServer(
                connectionString,
                sql => sql.MigrationsAssembly(typeof(MyDbContext).Assembly.FullName)));

        services.Configure<SharedSettings.IdentityOptions>(configuration.GetSection(SharedSettings.IdentityOptions.SectionName));
        
        // Register Jwt options
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        var identityOptionsSection = configuration.GetSection(SharedSettings.IdentityOptions.SectionName);
        var identityOpts = identityOptionsSection.Get<SharedSettings.IdentityOptions>() 
            ?? new SharedSettings.IdentityOptions();

        services
            .AddIdentityCore<IdentityUser>(options =>
            {
                options.Password.RequiredLength = identityOpts.PasswordMinLength;
                options.Password.RequireDigit = identityOpts.PasswordRequireDigit;
                options.Password.RequireLowercase = identityOpts.PasswordRequireLowercase;
                options.Password.RequireUppercase = identityOpts.PasswordRequireUppercase;
                options.Password.RequireNonAlphanumeric = identityOpts.PasswordRequireNonAlphanumeric;
                options.Lockout.MaxFailedAccessAttempts = identityOpts.LockoutMaxFailedAccessAttempts;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(identityOpts.LockoutDefaultLockoutTimeSpanMinutes);
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<MyDbContext>();

        // Configure JWT authentication
        var jwtSection = configuration.GetSection(JwtOptions.SectionName);
        var jwtOptions = jwtSection.Get<JwtOptions>() ?? new JwtOptions();

        var key = Encoding.ASCII.GetBytes(jwtOptions.Key 
            ?? throw new InvalidOperationException("JWT Key is not configured"));

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = jwtOptions.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtOptions.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

        services.AddAuthorization();

        return services;
    }
}
