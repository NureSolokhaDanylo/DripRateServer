using Application.Dtos;
using ErrorOr;
using MediatR;

namespace Application.Commands;

public record RegisterCommand(string UserName, string Email, string Password) : IRequest<ErrorOr<Guid>>;
