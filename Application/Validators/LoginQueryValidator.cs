using Application.Queries;
using FluentValidation;

namespace Application.Validators;

public sealed class LoginQueryValidator : AbstractValidator<LoginQuery>
{
    public LoginQueryValidator()
    {
        RuleFor(x => x.UserNameOrEmail)
            .NotEmpty();

        RuleFor(x => x.Password)
            .NotEmpty();
    }
}
