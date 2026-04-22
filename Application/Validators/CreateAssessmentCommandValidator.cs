using Application.Commands;
using FluentValidation;

namespace Application.Validators;

public sealed class CreateAssessmentCommandValidator : AbstractValidator<CreateAssessmentCommand>
{
    public CreateAssessmentCommandValidator()
    {
        RuleFor(x => x.ColorCoordination).InclusiveBetween(1, 10);
        RuleFor(x => x.FitAndProportions).InclusiveBetween(1, 10);
        RuleFor(x => x.Originality).InclusiveBetween(1, 10);
        RuleFor(x => x.OverallStyle).InclusiveBetween(1, 10);
    }
}
