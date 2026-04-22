using Application.Dtos;
using ErrorOr;
using MediatR;

namespace Application.Commands;

public record RegisterCommand(string? Username, string Email, string Password) : IRequest<ErrorOr<AuthResponse>>;
