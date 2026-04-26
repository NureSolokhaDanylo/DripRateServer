using ErrorOr;

namespace Domain.Errors;

public enum ApiErrorCategory
{
    Business = 1,
}

public readonly record struct ApiErrorMetadata(ApiErrorCategory Category, ErrorType ErrorType)
{
    public int StatusCode => ErrorType switch
    {
        ErrorType.Conflict => 409,
        ErrorType.Validation => 400,
        ErrorType.NotFound => 404,
        ErrorType.Unauthorized => 401,
        ErrorType.Forbidden => 403,
        _ => 500,
    };
}

public static class ApiErrorRegistry
{
    private static readonly IReadOnlyDictionary<string, ApiErrorMetadata> ErrorMap =
        new Dictionary<string, ApiErrorMetadata>(StringComparer.Ordinal)
        {
            [AuthErrors.InvalidCredentialsCode] = Business(ErrorType.Unauthorized),
            [AuthErrors.EmailAlreadyTakenCode] = Business(ErrorType.Conflict),
            [AuthErrors.UnauthorizedCode] = Business(ErrorType.Unauthorized),

            [PublicationErrors.NotFoundCode] = Business(ErrorType.NotFound),
            [PublicationErrors.ForbiddenCode] = Business(ErrorType.Forbidden),

            [CollectionErrors.NotFoundCode] = Business(ErrorType.NotFound),
            [CollectionErrors.ForbiddenCode] = Business(ErrorType.Forbidden),
            [CollectionErrors.LikesNotInitializedCode] = Business(ErrorType.Unexpected),
            [CollectionErrors.NameAlreadyExistsCode] = Business(ErrorType.Conflict),

            [CommentErrors.NotFoundCode] = Business(ErrorType.NotFound),
            [CommentErrors.ParentNotFoundCode] = Business(ErrorType.NotFound),
            [CommentErrors.ForbiddenCode] = Business(ErrorType.Forbidden),

            [UserErrors.NotFoundCode] = Business(ErrorType.NotFound),
            [UserErrors.DeleteFailedCode] = Business(ErrorType.Unexpected),

            [FileErrors.ProcessingFailedCode] = Business(ErrorType.Unexpected),

            [ClothErrors.NotFoundCode] = Business(ErrorType.NotFound),
            [ClothErrors.ForbiddenCode] = Business(ErrorType.Forbidden),

            [AssessmentErrors.CannotRateOwnPublicationCode] = Business(ErrorType.Conflict),
            [SocialErrors.CannotFollowSelfCode] = Business(ErrorType.Conflict),
        };

    public static bool TryGet(string code, out ApiErrorMetadata metadata)
        => ErrorMap.TryGetValue(code, out metadata);

    public static ApiErrorMetadata GetRequired(string code)
    {
        if (!TryGet(code, out var metadata))
        {
            throw new InvalidOperationException($"API error code '{code}' is not registered in {nameof(ApiErrorRegistry)}.");
        }

        return metadata;
    }

    private static ApiErrorMetadata Business(ErrorType type) => new(ApiErrorCategory.Business, type);
}
