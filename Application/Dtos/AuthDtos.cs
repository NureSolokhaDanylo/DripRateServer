namespace Application.Dtos;

public record RegisterRequest(string? DisplayName, string Email, string Password);
public record LoginRequest(string Email, string Password);
public record ChangePasswordRequest(string OldPassword, string NewPassword);
public record AuthResponse(string AccessToken, Guid UserId, string DisplayName, string Email);
