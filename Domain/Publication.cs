namespace Domain;

public sealed class Publication
{
    private Guid _id;
    private string _description = string.Empty;
    private DateTimeOffset _createdAt;
    private Guid _userId;
    private User _user = null!;

    private readonly List<Tag> _tags = new();
    private readonly List<Cloth> _clothes = new();
    private readonly List<Comment> _comments = new();
    private readonly List<Assessment> _assessments = new();
    private readonly List<string> _images = new();
    private readonly List<Collection> _collections = new();

    private int _likesCount;
    private int _commentsCount;
    private int _assessmentsCount;
    private double _averageRating;
    private int _ratingColorSum;
    private int _ratingFitSum;
    private int _ratingOriginalitySum;
    private int _ratingStyleSum;

    public Guid Id => _id;
    public string Description => _description;
    public DateTimeOffset CreatedAt => _createdAt;
    public Guid UserId => _userId;
    public User User => _user;

    public IReadOnlyCollection<string> Images => _images.AsReadOnly();
    public IReadOnlyCollection<Tag> Tags => _tags.AsReadOnly();
    public IReadOnlyCollection<Cloth> Clothes => _clothes.AsReadOnly();
    public IReadOnlyCollection<Comment> Comments => _comments.AsReadOnly();
    public IReadOnlyCollection<Assessment> Assessments => _assessments.AsReadOnly();
    public IReadOnlyCollection<Collection> Collections => _collections.AsReadOnly();

    public int LikesCount => _likesCount;
    public int CommentsCount => _commentsCount;
    public int AssessmentsCount => _assessmentsCount;
    public double AverageRating => _averageRating;

    public double AverageColorCoordination => _assessmentsCount > 0 ? (double)_ratingColorSum / _assessmentsCount : 0;
    public double AverageFitAndProportions => _assessmentsCount > 0 ? (double)_ratingFitSum / _assessmentsCount : 0;
    public double AverageOriginality => _assessmentsCount > 0 ? (double)_ratingOriginalitySum / _assessmentsCount : 0;
    public double AverageOverallStyle => _assessmentsCount > 0 ? (double)_ratingStyleSum / _assessmentsCount : 0;

    public MiniGameSettings GameSettings { get; private set; } = null!;
    public PublicationGameStats GameStats { get; private set; } = null!;

    private Publication() { }

    public Publication(Guid userId, string description, IEnumerable<string> images)
    {
        _userId = userId;
        _description = description;
        _createdAt = DateTimeOffset.UtcNow;
        _images.AddRange(images);
        
        _likesCount = 0;
        _commentsCount = 0;
        _assessmentsCount = 0;
        _averageRating = 0;
        
        GameSettings = new MiniGameSettings(false, true, false);
    }

    public void ConfigureMiniGames(bool guessPriceEnabled, bool tagMatchEnabled)
    {
        GameSettings = new MiniGameSettings(guessPriceEnabled, true, tagMatchEnabled);
    }

    internal void AddTag(Tag tag)
    {
        if (!_tags.Contains(tag)) _tags.Add(tag);
    }

    internal void AttachCloth(Cloth cloth)
    {
        if (!_clothes.Contains(cloth)) _clothes.Add(cloth);
    }

    internal void AddComment(Comment comment)
    {
        _comments.Add(comment);
        _commentsCount++;
    }

    internal void RemoveComment(Comment comment)
    {
        if (_comments.Remove(comment))
        {
            _commentsCount = Math.Max(0, _commentsCount - 1);
        }
    }

    internal void UpdateLikesCount(int delta)
    {
        _likesCount = Math.Max(0, _likesCount + delta);
    }

    internal void ApplyAssessment(Assessment? old, Assessment @new)
    {
        if (old != null)
        {
            _ratingColorSum -= old.ColorCoordination;
            _ratingFitSum -= old.FitAndProportions;
            _ratingOriginalitySum -= old.Originality;
            _ratingStyleSum -= old.OverallStyle;
        }
        else
        {
            _assessmentsCount++;
        }

        _ratingColorSum += @new.ColorCoordination;
        _ratingFitSum += @new.FitAndProportions;
        _ratingOriginalitySum += @new.Originality;
        _ratingStyleSum += @new.OverallStyle;

        if (_assessmentsCount > 0)
        {
            _averageRating = (_ratingColorSum + _ratingFitSum + _ratingOriginalitySum + _ratingStyleSum) / (4.0 * _assessmentsCount);
        }
        else
        {
            _averageRating = 0;
        }
    }
}

public record MiniGameSettings(
    bool IsGuessPriceEnabled,
    bool IsFirstImpressionEnabled,
    bool IsTagMatchEnabled
);
