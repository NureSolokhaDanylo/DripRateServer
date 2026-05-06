using Application.Dtos;
using ErrorOr;
using MediatR;

namespace Application.Queries.Social;

public record GetFollowingQuery(Guid UserId, int Skip = 0, int Take = 50) : IRequest<ErrorOr<List<UserProfileResponse>>>;
