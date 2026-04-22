using Application.Dtos;
using ErrorOr;
using MediatR;

namespace Application.Queries;

public record GetMyProfileQuery(Guid UserId) : IRequest<ErrorOr<UserProfileResponse>>;
