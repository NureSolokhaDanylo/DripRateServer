using Application.Commands;
using Application.Interfaces;
using Application.Interfaces.Internal;
using Domain;
using Domain.Errors;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Handlers.Commands;

internal sealed class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, ErrorOr<Deleted>>
{
    private readonly IApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly IDeletionService _deletionService;

    public DeleteUserCommandHandler(IApplicationDbContext context, UserManager<User> userManager, IDeletionService deletionService)
    {
        _context = context;
        _userManager = userManager;
        _deletionService = deletionService;
    }

    public async Task<ErrorOr<Deleted>> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());

        if (user == null)
        {
            return UserErrors.NotFound;
        }

        await _deletionService.DeleteUserContentAsync(user.Id, cancellationToken);
        
        var result = await _userManager.DeleteAsync(user);

        if (!result.Succeeded)
        {
            return UserErrors.DeleteFailed;
        }

        return Result.Deleted;
    }
}
