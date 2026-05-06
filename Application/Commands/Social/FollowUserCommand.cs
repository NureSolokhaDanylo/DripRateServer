using ErrorOr;
using MediatR;

namespace Application.Commands.Social;

public record FollowUserCommand(Guid FollowerId, Guid FolloweeId) : IRequest<ErrorOr<Success>>;
