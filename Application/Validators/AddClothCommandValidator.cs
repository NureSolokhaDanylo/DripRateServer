using Application.Commands;
using FluentValidation;

namespace Application.Validators;

public sealed class AddClothCommandValidator : AbstractValidator<AddClothCommand>
{
    public AddClothCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Brand)
            .MaximumLength(100);

        RuleFor(x => x.StoreLink)
            .MaximumLength(2048)
            .Must(x => string.IsNullOrEmpty(x) || Uri.TryCreate(x, UriKind.Absolute, out _))
            .WithMessage("Invalid URL format.");

        RuleFor(x => x.EstimatedPrice)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.PhotoContentType)
            .Must(x => string.IsNullOrEmpty(x) || x.StartsWith("image/"))
            .WithMessage("Only image files are allowed.");
    }
}
