using Application.Interfaces;
using Infrastructure.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
        // Get JWT options
        var jwtOptions = SharedConfiguration.GetIOptions2<JwtOptions>();
        services.Configure<JwtOptions>(_  => 
        {
            _.Key = jwtOptions.Key;
            _.Issuer = jwtOptions.Issuer;
            _.Audience = jwtOptions.Audience;
            _.ExpirationMinutes = jwtOptions.ExpirationMinutes;
            _.RefreshTokenExpirationDays = jwtOptions.RefreshTokenExpirationDays;
        });

        var key = Encoding.ASCII.GetBytes(jwtOptions.Key 
            ?? throw new InvalidOperationException("JWT Key is not configured"));

        services.AddHttpContextAccessor();

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

        // Register JWT token generation service
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        // Register ICurrentUser implementation
        services.AddScoped<ICurrentUser, Infrastructure.Authentication.HttpContextCurrentUser>();

        return services;
    }
}
