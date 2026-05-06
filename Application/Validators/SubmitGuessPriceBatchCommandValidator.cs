using Application.Commands.Games;
using Application.Dtos;
using FluentValidation;

namespace Application.Validators;

public sealed class SubmitGuessPriceBatchCommandValidator : AbstractValidator<SubmitGuessPriceBatchCommand>
{
    public SubmitGuessPriceBatchCommandValidator()
    {
        RuleFor(x => x.Results)
            .NotNull()
            .Must(r => r.Count <= 50)
            .WithMessage("Batch size cannot exceed 50 results.");

        RuleForEach(x => x.Results).SetValidator(new GuessPriceResultDtoValidator());
    }
}

public sealed class GuessPriceResultDtoValidator : AbstractValidator<GuessPriceResultDto>
{
    public GuessPriceResultDtoValidator()
    {
        RuleFor(x => x.PublicationId)
            .NotEmpty();

        RuleFor(x => x.GuessedPrice)
            .GreaterThanOrEqualTo(0);
    }
}
