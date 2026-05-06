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
