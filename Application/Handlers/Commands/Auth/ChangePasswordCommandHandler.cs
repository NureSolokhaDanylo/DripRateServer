using Application.Commands.Auth;
using Domain;
using Domain.Errors;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Handlers.Commands.Auth;

public sealed class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, ErrorOr<Success>>
{
    private readonly UserManager<User> _userManager;

    public ChangePasswordCommandHandler(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<ErrorOr<Success>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        // 1. Retrieve the user by ID
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());

        if (user == null)
        {
            return UserErrors.NotFound;
        }

        // 2. Attempt to change the password
        var result = await _userManager.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);

        // 3. Handle failure results
        if (!result.Succeeded)
        {
            var errors = result.Errors
                .Select(e => Error.Validation(e.Code, e.Description))
                .ToList();

            return errors;
        }

        // 4. Return success
        return Result.Success;
    }
}
