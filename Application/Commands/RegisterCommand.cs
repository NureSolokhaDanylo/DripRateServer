using Application.Dtos;
using ErrorOr;
using MediatR;

namespace Application.Commands;

public record RegisterCommand(string DisplayName, string Email, string Password) : IRequest<ErrorOr<Guid>>;
