namespace SharedSettings;

public sealed class IdentityOptions
{
    public const string SectionName = "Identity";

    public int PasswordMinLength { get; set; } = 8;
    public bool PasswordRequireDigit { get; set; } = true;
    public bool PasswordRequireLowercase { get; set; } = true;
    public bool PasswordRequireUppercase { get; set; } = true;
    public bool PasswordRequireNonAlphanumeric { get; set; } = true;
    public int LockoutMaxFailedAccessAttempts { get; set; } = 5;
    public int LockoutDefaultLockoutTimeSpanMinutes { get; set; } = 5;
}
