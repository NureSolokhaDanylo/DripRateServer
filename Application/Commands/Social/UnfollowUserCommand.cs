using ErrorOr;
using MediatR;

namespace Application.Commands.Social;

public record UnfollowUserCommand(Guid FollowerId, Guid FolloweeId) : IRequest<ErrorOr<Success>>;
