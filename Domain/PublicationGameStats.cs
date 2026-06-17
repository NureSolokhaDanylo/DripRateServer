namespace Domain;

public sealed class PublicationGameStats
{
    public Guid PublicationId { get; private set; }
    public Publication Publication { get; private set; } = null!;

    // Guess Price
    public int GuessPriceTotalCount { get; private set; }
    public decimal GuessPriceRealSum { get; private set; }
    public decimal GuessPriceGuessedSum { get; private set; }

    // First Impression
    public int FirstImpressionTotalCount { get; private set; }
    public int FirstImpressionPositiveCount { get; private set; }
    public long FirstImpressionReactionTimeSum { get; private set; }

    // Tag Match
    public int TagMatchTotalCount { get; private set; }
    public int TagMatchCorrectCount { get; private set; }

    public const int MinFirstImpressionReactionTimeMs = 150;
    public const int MaxFirstImpressionReactionTimeMs = 5000;

    public static bool IsValidReactionTime(long reactionTimeMs)
        => reactionTimeMs >= MinFirstImpressionReactionTimeMs && reactionTimeMs <= MaxFirstImpressionReactionTimeMs;

    private PublicationGameStats() { }

    public PublicationGameStats(Guid publicationId)
    {
        if (publicationId == Guid.Empty) throw new ArgumentException("Publication ID cannot be empty.", nameof(publicationId));
        PublicationId = publicationId;
    }

    public void AddGuessPriceResult(decimal realPrice, decimal guessedPrice)
    {
        if (realPrice < 0) throw new ArgumentException("Real price cannot be negative.", nameof(realPrice));
        if (guessedPrice < 0) throw new ArgumentException("Guessed price cannot be negative.", nameof(guessedPrice));

        GuessPriceTotalCount++;
        GuessPriceRealSum += realPrice;
        GuessPriceGuessedSum += guessedPrice;
    }

    public void AddFirstImpressionResult(bool isPositive, long reactionTimeMs)
    {
        if (reactionTimeMs < 0) throw new ArgumentException("Reaction time cannot be negative.", nameof(reactionTimeMs));

        FirstImpressionTotalCount++;
        if (isPositive) FirstImpressionPositiveCount++;
        FirstImpressionReactionTimeSum += reactionTimeMs;
    }

    public void AddTagMatchResult(int correctCount)
    {
        if (correctCount < 0) throw new ArgumentException("Correct count cannot be negative.", nameof(correctCount));

        TagMatchTotalCount++;
        TagMatchCorrectCount += correctCount;
    }
}
