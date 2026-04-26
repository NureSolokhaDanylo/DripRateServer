using Application.Commands;
using FluentValidation;

namespace Application.Validators;

public sealed class UploadAvatarCommandValidator : AbstractValidator<UploadAvatarCommand>
{
    public UploadAvatarCommandValidator()
    {
        RuleFor(x => x.ImageStream)
            .NotNull()
            .Must(s => s.Length <= 5 * 1024 * 1024).WithMessage("File size must not exceed 5 MB.");
        RuleFor(x => x.ContentType)
            .NotEmpty()
            .Must(x => x.StartsWith("image/"))
            .WithMessage("Only image files are allowed.");
    }
}
