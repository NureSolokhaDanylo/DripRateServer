using Application.Commands;
using FluentValidation;

namespace Application.Validators;

public sealed class UploadAvatarCommandValidator : AbstractValidator<UploadAvatarCommand>
{
    public UploadAvatarCommandValidator()
    {
        RuleFor(x => x.ImageStream).NotNull();
        RuleFor(x => x.ContentType)
            .NotEmpty()
            .Must(x => x.StartsWith("image/"))
            .WithMessage("Only image files are allowed.");
    }
}
