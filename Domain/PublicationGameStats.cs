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

    private PublicationGameStats() { }

    public PublicationGameStats(Guid publicationId)
    {
        PublicationId = publicationId;
    }

    public void AddGuessPriceResult(decimal realPrice, decimal guessedPrice)
    {
        GuessPriceTotalCount++;
        GuessPriceRealSum += realPrice;
        GuessPriceGuessedSum += guessedPrice;
    }

    public void AddFirstImpressionResult(bool isPositive, long reactionTimeMs)
    {
        FirstImpressionTotalCount++;
        if (isPositive) FirstImpressionPositiveCount++;
        FirstImpressionReactionTimeSum += reactionTimeMs;
    }

    public void AddTagMatchResult(int correctCount)
    {
        TagMatchTotalCount++;
        TagMatchCorrectCount += correctCount;
    }
}
