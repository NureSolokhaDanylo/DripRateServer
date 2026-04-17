namespace Domain;

public sealed class Assessment
{
    public Guid Id { get; private set; }
    
    // Ratings 1-10
    public int ColorCoordination { get; private set; }
    public int FitAndProportions { get; private set; }
    public int Originality { get; private set; }
    public int OverallStyle { get; private set; }

    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    public Guid PublicationId { get; private set; }
    public Publication Publication { get; private set; } = null!;

    private Assessment() { }

    public Assessment(Guid userId, Guid publicationId, int color, int fit, int originality, int style)
    {
        UserId = userId;
        PublicationId = publicationId;
        
        UpdateRatings(color, fit, originality, style);
    }

    public void UpdateRatings(int color, int fit, int originality, int style)
    {
        ColorCoordination = ValidateRating(color);
        FitAndProportions = ValidateRating(fit);
        Originality = ValidateRating(originality);
        OverallStyle = ValidateRating(style);
    }

    private static int ValidateRating(int value) => Math.Clamp(value, 1, 10);
}
