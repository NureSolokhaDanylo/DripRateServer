using Application.Commands;
using Application.Dtos;
using Microsoft.AspNetCore.Identity;
using SharedSettings.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;

namespace Application.Handlers;

public sealed class RegisterCommandHandler
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IOptions<JwtOptions> _jwtOptions;

    public RegisterCommandHandler(
        UserManager<IdentityUser> userManager,
        IOptions<JwtOptions> jwtOptions)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _jwtOptions = jwtOptions ?? throw new ArgumentNullException(nameof(jwtOptions));
    }

    public async Task<AuthResponse> Handle(RegisterCommand command)
    {
        var user = new IdentityUser
        {
            UserName = command.Username,
            Email = command.Email
        };

        var result = await _userManager.CreateAsync(user, command.Password!);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return new AuthResponse
            {
                Success = false,
                Message = $"Registration failed: {errors}"
            };
        }

        var token = GenerateJwtToken(user);

        return new AuthResponse
        {
            Success = true,
            Message = "Registration successful",
            AccessToken = token
        };
    }

    private string GenerateJwtToken(IdentityUser user)
    {
        var key = Encoding.ASCII.GetBytes(_jwtOptions.Value.Key 
            ?? throw new InvalidOperationException("JWT Key not configured"));

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? ""),
            new(ClaimTypes.Email, user.Email ?? "")
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtOptions.Value.ExpirationMinutes),
            Issuer = _jwtOptions.Value.Issuer,
            Audience = _jwtOptions.Value.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
