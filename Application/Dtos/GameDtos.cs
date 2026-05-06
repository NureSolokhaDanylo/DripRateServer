using Domain;

namespace Application.Dtos;

public record FirstImpressionGameItemDto(
    Guid PublicationId,
    UserSimpleDto Author,
    IReadOnlyCollection<string> Images
);

public record FirstImpressionResultDto(
    Guid PublicationId,
    bool IsPositive,
    long ReactionTimeMs
);

public record GuessPriceGameItemDto(
    Guid PublicationId,
    UserSimpleDto Author,
    IReadOnlyCollection<string> Images,
    decimal RealPrice
);

public record GuessPriceResultDto(
    Guid PublicationId,
    decimal GuessedPrice
);

public record TagMatchGameItemDto(
    Guid PublicationId,
    UserSimpleDto Author,
    IReadOnlyCollection<string> Images,
    IReadOnlyCollection<TagResponse> Tags
);

public record TagMatchResultDto(
    Guid PublicationId,
    IReadOnlyCollection<Guid> TagIds
);
