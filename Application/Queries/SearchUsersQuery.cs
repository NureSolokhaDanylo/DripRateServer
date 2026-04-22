using Application.Dtos;
using ErrorOr;
using MediatR;

namespace Application.Queries;

public record SearchUsersQuery(string SearchQuery, int Take = 20) : IRequest<ErrorOr<List<UserProfileResponse>>>;
