using Application.Commands;
using FluentValidation;

namespace Application.Validators;

public sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.OldPassword)
            .NotEmpty();

        RuleFor(x => x.NewPassword)
            .NotEmpty();
    }
}
