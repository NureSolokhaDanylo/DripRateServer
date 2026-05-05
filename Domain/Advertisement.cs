namespace Domain;

public sealed class Advertisement
{
    private Guid _id;
    private string _text = string.Empty;
    private int _maxImpressions;
    private int _shownCount;
    private bool _isActive;
    private DateTimeOffset _createdAt;

    private readonly List<string> _images = new();
    private readonly List<Tag> _tags = new();
    private readonly List<AdvertisementView> _views = new();

    public Guid Id => _id;
    public string Text => _text;
    public int MaxImpressions => _maxImpressions;
    public int ShownCount => _shownCount;
    public bool IsActive => _isActive;
    public DateTimeOffset CreatedAt => _createdAt;

    public IReadOnlyCollection<string> Images => _images.AsReadOnly();
    public IReadOnlyCollection<Tag> Tags => _tags.AsReadOnly();
    public IReadOnlyCollection<AdvertisementView> Views => _views.AsReadOnly();

    private Advertisement() { }

    public Advertisement(string text, int maxImpressions, IEnumerable<string> images)
    {
        _id = Guid.NewGuid();
        _text = text;
        _maxImpressions = maxImpressions;
        _images.AddRange(images);
        _createdAt = DateTimeOffset.UtcNow;
        _shownCount = 0;
        _isActive = true;
    }

    public void Update(string text, int maxImpressions, IEnumerable<string> finalImages, bool? isActive = null)
    {
        _text = text;
        _maxImpressions = maxImpressions;
        _images.Clear();
        _images.AddRange(finalImages);
        
        if (isActive.HasValue)
        {
            _isActive = isActive.Value && _shownCount < _maxImpressions;
        }
        else if (_shownCount >= _maxImpressions)
        {
            _isActive = false;
        }
    }

    public bool SetStatus(bool isActive)
    {
        if (isActive && _shownCount >= _maxImpressions)
        {
            return false;
        }

        _isActive = isActive;
        return true;
    }

    public void AddTag(Tag tag)
    {
        if (!_tags.Contains(tag)) _tags.Add(tag);
    }

    public void ClearTags() => _tags.Clear();

    public void IncrementShownCount()
    {
        _shownCount++;
        if (_shownCount >= _maxImpressions)
        {
            _isActive = false;
        }
    }
}
