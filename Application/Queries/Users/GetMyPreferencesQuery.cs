using Application.Dtos;
using ErrorOr;
using MediatR;

namespace Application.Queries.Users;

public record GetMyPreferencesQuery(Guid UserId) : IRequest<ErrorOr<List<TagResponse>>>;
