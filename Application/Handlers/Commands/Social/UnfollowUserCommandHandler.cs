using Application.Commands.Social;
using Application.Interfaces;
using Domain;
using Domain.Errors;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Application.Commands.Social;

namespace Application.Handlers.Commands.Social;

public sealed class UnfollowUserCommandHandler : IRequestHandler<UnfollowUserCommand, ErrorOr<Success>>
{
    private readonly IApplicationDbContext _context;

    public UnfollowUserCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<Success>> Handle(UnfollowUserCommand request, CancellationToken cancellationToken)
    {
        var followeeExists = await _context.Users.AnyAsync(u => u.Id == request.FolloweeId, cancellationToken);
        if (!followeeExists)
        {
            return UserErrors.NotFound;
        }

        var follow = await _context.Follows
            .FirstOrDefaultAsync(f => f.FollowerId == request.FollowerId && f.FolloweeId == request.FolloweeId, cancellationToken);

        if (follow != null)
        {
            _context.Follows.Remove(follow);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return Result.Success;
    }
}
