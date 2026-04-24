using Application.Commands;
using Application.Extensions;
using Application.Interfaces;
using Application.Interfaces.Internal;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Handlers.Commands;

public sealed class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, ErrorOr<Deleted>>
{
    private readonly IApplicationDbContext _context;
    private readonly IDeletionService _deletionService;

    public DeleteUserCommandHandler(IApplicationDbContext context, IDeletionService deletionService)
    {
        _context = context;
        _deletionService = deletionService;
    }

    public async Task<ErrorOr<Deleted>> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var userExists = await _context.Users.AnyAsync(u => u.Id == request.UserId, cancellationToken);
        if (!userExists)
        {
            return Error.NotFound("User.NotFound", "User not found.");
        }

        // Delegate complex cleanup to internal service (Rule 8)
        await _deletionService.DeleteUserContentAsync(request.UserId, cancellationToken);

        // Finally delete the user itself
        await _context.Users
            .Where(u => u.Id == request.UserId)
            .ExecuteDeleteAsync(cancellationToken);

        return Result.Deleted;
    }
}
