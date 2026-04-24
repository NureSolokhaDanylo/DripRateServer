using Application.Dtos;
using ErrorOr;
using MediatR;

namespace Application.Queries;

public record GetUserProfileQuery(string Username, Guid? CurrentUserId = null) : IRequest<ErrorOr<UserProfileResponse>>;
