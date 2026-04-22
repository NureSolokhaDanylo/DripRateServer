namespace Application.Dtos;

public record CommentResponse(
    Guid Id,
    string Text,
    DateTimeOffset CreatedAt,
    Guid UserId,
    string Username,
    string? AvatarUrl,
    int LikesCount,
    bool IsLikedByMe,
    Guid? ParentCommentId,
    int RepliesCount);

public record CreateCommentRequest(
    string Text,
    Guid? ParentCommentId);
