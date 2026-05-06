using Application.Commands.Games;
using Application.Dtos;
using FluentValidation;

namespace Application.Validators;

public sealed class SubmitTagMatchBatchCommandValidator : AbstractValidator<SubmitTagMatchBatchCommand>
{
    public SubmitTagMatchBatchCommandValidator()
    {
        RuleFor(x => x.Results)
            .NotNull()
            .Must(r => r.Count <= 50)
            .WithMessage("Batch size cannot exceed 50 results.");

        RuleForEach(x => x.Results).SetValidator(new TagMatchResultDtoValidator());
    }
}

public sealed class TagMatchResultDtoValidator : AbstractValidator<TagMatchResultDto>
{
    public TagMatchResultDtoValidator()
    {
        RuleFor(x => x.PublicationId)
            .NotEmpty();

        RuleFor(x => x.TagIds)
            .NotNull();
    }
}
