using Application.Commands;
using FluentValidation;

namespace Application.Validators;

public sealed class CreatePublicationCommandValidator : AbstractValidator<CreatePublicationCommand>
{
    public CreatePublicationCommandValidator()
    {
        RuleFor(x => x.Description)
            .MaximumLength(1000);

        RuleFor(x => x.Images)
            .NotEmpty().WithMessage("At least one image is required.")
            .Must(x => x.Count <= 5).WithMessage("You can upload up to 5 images.");

        RuleForEach(x => x.Images).ChildRules(image =>
        {
            image.RuleFor(x => x.Stream)
                .NotNull()
                .Must(s => s.Length <= 5 * 1024 * 1024).WithMessage("Each file size must not exceed 5 MB.");

            image.RuleFor(x => x.ContentType)
                .NotEmpty()
                .Must(x => x != null && x.StartsWith("image/"))
                .WithMessage("Only image files are allowed.");
        });
    }
}
