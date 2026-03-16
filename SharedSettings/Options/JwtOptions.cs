namespace SharedSettings.Options;

public sealed class JwtOptions : IOptions2
{
    public string GetSectionName() => "Jwt";

    public string? Key { get; set; }
    public string? Issuer { get; set; }
    public string? Audience { get; set; }
    public int ExpirationMinutes { get; set; } = 60;
    public int RefreshTokenExpirationDays { get; set; } = 7;


}
