using Application.Dtos;
using ErrorOr;
using MediatR;

namespace Application.Queries;

public record GetMyPreferencesQuery(Guid UserId) : IRequest<ErrorOr<List<TagResponse>>>;
