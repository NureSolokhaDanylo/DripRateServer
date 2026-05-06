using Application.Commands.Comments;
using FluentValidation;

namespace Application.Validators;

public sealed class CreateCommentCommandValidator : AbstractValidator<CreateCommentCommand>
{
    public CreateCommentCommandValidator()
    {
        RuleFor(x => x.Text)
            .NotEmpty()
            .MaximumLength(1000);
    }
}
