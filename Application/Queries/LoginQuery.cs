using Application.Dtos;
using ErrorOr;
using MediatR;

namespace Application.Queries;

public record LoginQuery(string Username, string Password) : IRequest<ErrorOr<AuthResponse>>;
