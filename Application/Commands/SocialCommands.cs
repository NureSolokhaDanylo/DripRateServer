using ErrorOr;
using MediatR;

namespace Application.Commands;

public record FollowUserCommand(Guid FollowerId, Guid FolloweeId) : IRequest<ErrorOr<Success>>;

public record UnfollowUserCommand(Guid FollowerId, Guid FolloweeId) : IRequest<ErrorOr<Success>>;
