using Application.Commands;
using FluentValidation;

namespace Application.Validators;

public sealed class CreatePublicationCommandValidator : AbstractValidator<CreatePublicationCommand>
{
    public CreatePublicationCommandValidator()
    {
        RuleFor(x => x.Description)
            .MaximumLength(1000);

        RuleFor(x => x.ImageStream)
            .NotNull();

        RuleFor(x => x.ContentType)
            .NotEmpty()
            .Must(x => x.StartsWith("image/"))
            .WithMessage("Only image files are allowed.");
    }
}
