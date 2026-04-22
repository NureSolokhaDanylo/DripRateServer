using Application.Commands;
using FluentValidation;

namespace Application.Validators;

public sealed class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.Bio)
            .MaximumLength(500);
    }
}
