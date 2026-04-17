namespace Application.Dtos;

public record RegisterRequest(string Username, string Email, string Password);
public record LoginRequest(string Username, string Password);
public record AuthResponse(string AccessToken, Guid UserId, string Username, string Email);
