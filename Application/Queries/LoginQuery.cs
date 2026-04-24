using Application.Dtos;
using ErrorOr;
using MediatR;

namespace Application.Queries;

public record LoginQuery(string Email, string Password) : IRequest<ErrorOr<string>>;
