using Application.Interfaces;
using Infrastructure.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SharedSettings;
using SharedSettings.Options;
using System.Text;

namespace Infrastructure;

public static partial class ServiceCollectionExtensions
{
    private static IServiceCollection AddAuthenticationConfiguration(this IServiceCollection services)
    {
        var configuration = SharedConfigurationBuilder.Build();

        // Register Jwt options
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

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

        // Register ICurrentUser factory
        services.AddScoped(sp =>
        {
            var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
            var principal = httpContextAccessor.HttpContext?.User;
            return (ICurrentUser)new CurrentUser(principal ?? new System.Security.Claims.ClaimsPrincipal());
        });

        return services;
    }
}
