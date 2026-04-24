using Application.Dtos;
using ErrorOr;
using MediatR;

namespace Application.Queries;

public record LoginQuery(string UserNameOrEmail, string Password) : IRequest<ErrorOr<string>>;
