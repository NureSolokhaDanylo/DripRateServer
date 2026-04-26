using Application.Commands;
using FluentValidation;

namespace Application.Validators;

public sealed class UpdateClothCommandValidator : AbstractValidator<UpdateClothCommand>
{
    public UpdateClothCommandValidator()
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

        RuleFor(x => x.PhotoStream)
            .Must(s => s == null || s.Length <= 5 * 1024 * 1024).WithMessage("File size must not exceed 5 MB.");

        RuleFor(x => x.PhotoContentType)
            .Must(x => string.IsNullOrEmpty(x) || x.StartsWith("image/"))
            .WithMessage("Only image files are allowed.");
    }
}
