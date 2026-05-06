using Application.Commands.Games;
using Application.Dtos;
using FluentValidation;

namespace Application.Validators;

public sealed class SubmitFirstImpressionBatchCommandValidator : AbstractValidator<SubmitFirstImpressionBatchCommand>
{
    public SubmitFirstImpressionBatchCommandValidator()
    {
        RuleFor(x => x.Results)
            .NotNull()
            .Must(r => r.Count <= 50)
            .WithMessage("Batch size cannot exceed 50 results.");

        RuleForEach(x => x.Results).SetValidator(new FirstImpressionResultDtoValidator());
    }
}

public sealed class FirstImpressionResultDtoValidator : AbstractValidator<FirstImpressionResultDto>
{
    public FirstImpressionResultDtoValidator()
    {
        RuleFor(x => x.PublicationId)
            .NotEmpty();

        RuleFor(x => x.ReactionTimeMs)
            .GreaterThanOrEqualTo(0);
    }
}
