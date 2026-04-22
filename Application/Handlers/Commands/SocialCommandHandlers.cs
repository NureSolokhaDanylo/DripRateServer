using Application.Commands;
using Application.Interfaces;
using Domain;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Handlers.Commands;

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
            return Error.Validation(description: "You cannot follow yourself.");
        }

        var followExists = await _context.Follows
            .AnyAsync(f => f.FollowerId == request.FollowerId && f.FolloweeId == request.FolloweeId, cancellationToken);

        if (followExists)
        {
            return Result.Success;
        }

        var follow = new Follow(request.FollowerId, request.FolloweeId);
        _context.Follows.Add(follow);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}

public sealed class UnfollowUserCommandHandler : IRequestHandler<UnfollowUserCommand, ErrorOr<Success>>
{
    private readonly IApplicationDbContext _context;

    public UnfollowUserCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<Success>> Handle(UnfollowUserCommand request, CancellationToken cancellationToken)
    {
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
