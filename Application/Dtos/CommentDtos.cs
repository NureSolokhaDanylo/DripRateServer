using System.Linq.Expressions;
using Domain;

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
    int RepliesCount)
{
    public static Expression<Func<Comment, CommentResponse>> GetProjection(Guid? currentUserId)
    {
        return c => new CommentResponse(
            c.Id,
            c.Text,
            c.CreatedAt,
            c.UserId,
            c.User.UserName ?? string.Empty,
            c.User.AvatarUrl,
            c.Likes.Count,
            currentUserId.HasValue && c.Likes.Any(l => l.UserId == currentUserId.Value),
            c.ParentCommentId,
            c.Replies.Count
        );
    }
}

public record CreateCommentRequest(
    string Text,
    Guid? ParentCommentId);
