using Application.Dtos;
using ErrorOr;
using MediatR;

namespace Application.Queries.Auth;

public record LoginQuery(string Email, string Password) : IRequest<ErrorOr<string>>;
