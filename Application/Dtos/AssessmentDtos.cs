namespace Application.Dtos;

public record AssessmentResponse(
    double AverageColorCoordination,
    double AverageFitAndProportions,
    double AverageOriginality,
    double AverageOverallStyle,
    double TotalAverage,
    int TotalAssessments);

public record IndividualAssessmentResponse(
    Guid UserId,
    string Username,
    string AvatarUrl,
    int ColorCoordination,
    int FitAndProportions,
    int Originality,
    int OverallStyle,
    DateTimeOffset CreatedAt);

public record CreateAssessmentRequest(
    int ColorCoordination,
    int FitAndProportions,
    int Originality,
    int OverallStyle);
