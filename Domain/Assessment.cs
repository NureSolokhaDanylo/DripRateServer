namespace Domain;

public sealed class Assessment
{
    private Guid _id;
    private int _colorCoordination;
    private int _fitAndProportions;
    private int _originality;
    private int _overallStyle;
    private Guid _userId;
    private User _user = null!;
    private Guid _publicationId;
    private Publication _publication = null!;

    public Guid Id => _id;
    
    // Ratings 1-10
    public int ColorCoordination => _colorCoordination;
    public int FitAndProportions => _fitAndProportions;
    public int Originality => _originality;
    public int OverallStyle => _overallStyle;

    public Guid UserId => _userId;
    public User User => _user;

    public Guid PublicationId => _publicationId;
    public Publication Publication => _publication;

    private Assessment() { }

    public Assessment(Guid userId, Guid publicationId, int color, int fit, int originality, int style)
    {
        _userId = userId;
        _publicationId = publicationId;
        
        UpdateRatings(color, fit, originality, style);
    }

    public void UpdateRatings(int color, int fit, int originality, int style)
    {
        _colorCoordination = ValidateRating(color);
        _fitAndProportions = ValidateRating(fit);
        _originality = ValidateRating(originality);
        _overallStyle = ValidateRating(style);
    }

    private static int ValidateRating(int value) => Math.Clamp(value, 1, 10);
}
