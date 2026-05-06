using Application.Dtos;
using ErrorOr;
using MediatR;

namespace Application.Queries.Users;

public record GetUserProfileQuery(Guid UserId, Guid? CurrentUserId = null) : IRequest<ErrorOr<UserProfileResponse>>;
