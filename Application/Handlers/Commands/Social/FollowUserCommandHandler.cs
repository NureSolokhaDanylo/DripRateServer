using Application.Commands.Social;
using Application.Interfaces;
using Domain;
using Domain.Errors;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Application.Commands.Social;

namespace Application.Handlers.Commands.Social;

public sealed class FollowUserCommandHandler : IRequestHandler<FollowUserCommand, ErrorOr<Success>>
{
    private readonly IApplicationDbContext _context;

    public FollowUserCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<Success>> Handle(FollowUserCommand request, CancellationToken cancellationToken)
    {
        if (request.FollowerId == request.FolloweeId)
        {
            return SocialErrors.CannotFollowSelf;
        }

        var followeeExists = await _context.Users.AnyAsync(u => u.Id == request.FolloweeId, cancellationToken);
        if (!followeeExists)
        {
            return UserErrors.NotFound;
        }

        var followExists = await _context.Follows
            .AnyAsync(f => f.FollowerId == request.FollowerId && f.FolloweeId == request.FolloweeId, cancellationToken);

        if (!followExists)
        {
            var follow = new Follow(request.FollowerId, request.FolloweeId);
            _context.Follows.Add(follow);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return Result.Success;
    }
}
