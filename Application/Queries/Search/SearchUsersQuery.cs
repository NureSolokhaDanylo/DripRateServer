using Application.Dtos;
using ErrorOr;
using MediatR;

namespace Application.Queries.Search;

public record SearchUsersQuery(
    string SearchQuery,
    int Skip = 0,
    int Take = 20) : IRequest<ErrorOr<List<UserProfileResponse>>>;
