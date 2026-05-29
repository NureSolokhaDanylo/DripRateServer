namespace Application.Dtos;

public sealed record AarrrMetricsResponse(
    int Acquisition_TotalUsers,
    int Activation_ActiveUsers,
    int Retention_UsersReturnedAfter7Days,
    int Referral_TotalFollows,
    int Revenue_AdViews
);
