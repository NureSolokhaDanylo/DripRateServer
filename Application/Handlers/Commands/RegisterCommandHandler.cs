using Application.Commands;
using Application.Interfaces;
using Domain;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Handlers.Commands;

public sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand, ErrorOr<Guid>>
{
    private readonly UserManager<User> _userManager;

    public RegisterCommandHandler(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<ErrorOr<Guid>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var user = new User(request.Email, request.UserName);

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var errors = result.Errors
                .Select(e => Error.Validation(e.Code, e.Description))
                .ToList();

            return errors;
        }

        user.InitializeCollections();
        await _userManager.UpdateAsync(user);

        return user.Id;
    }
}
