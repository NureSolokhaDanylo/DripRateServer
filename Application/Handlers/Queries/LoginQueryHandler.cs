using Application.Dtos;
using Application.Queries;
using Microsoft.AspNetCore.Identity;
using SharedSettings.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;

namespace Application.Handlers;

public sealed class LoginQueryHandler
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IOptions<JwtOptions> _jwtOptions;

    public LoginQueryHandler(
        UserManager<IdentityUser> userManager,
        IOptions<JwtOptions> jwtOptions)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _jwtOptions = jwtOptions ?? throw new ArgumentNullException(nameof(jwtOptions));
    }

    public async Task<AuthResponse> Handle(LoginQuery query)
    {
        var user = await _userManager.FindByNameAsync(query.Username!);
        if (user == null)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Invalid username or password"
            };
        }

        var passwordValid = await _userManager.CheckPasswordAsync(user, query.Password!);
        if (!passwordValid)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Invalid username or password"
            };
        }

        var token = GenerateJwtToken(user);

        return new AuthResponse
        {
            Success = true,
            Message = "Login successful",
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
