using Application.Commands;
using Application.Interfaces;
using Domain;
using Domain.Errors;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using SharedSettings.Options;

namespace Application.Handlers.Commands;

public sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand, ErrorOr<Guid>>
{
    private readonly UserManager<User> _userManager;
    private readonly BlobStorageOptions _blobOptions;

    public RegisterCommandHandler(UserManager<User> userManager, IOptions<BlobStorageOptions> blobOptions)
    {
        _userManager = userManager;
        _blobOptions = blobOptions.Value;
    }

    public async Task<ErrorOr<Guid>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var user = new User(request.Email, request.DisplayName);
        user.UpdateAvatar(_blobOptions.DefaultAvatarUrl);

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Code switch
            {
                "DuplicateEmail" => AuthErrors.EmailAlreadyTaken,
                _ => Error.Validation(e.Code, e.Description)
            }).ToList();

            return errors;
        }

        user.InitializeCollections();
        await _userManager.UpdateAsync(user);

        return user.Id;
    }
}
