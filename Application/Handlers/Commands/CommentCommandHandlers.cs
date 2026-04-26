using Application.Commands;
using Application.Interfaces;
using Domain;
using Domain.Errors;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Handlers.Commands;

public sealed class CreateCommentCommandHandler : IRequestHandler<CreateCommentCommand, ErrorOr<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateCommentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<Guid>> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
    {
        var publication = await _context.Publications.FirstOrDefaultAsync(p => p.Id == request.PublicationId, cancellationToken);
        if (publication == null) return PublicationErrors.NotFound;

        Comment? parentComment = null;
        if (request.ParentCommentId.HasValue)
        {
            parentComment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == request.ParentCommentId.Value, cancellationToken);
            if (parentComment == null) return CommentErrors.ParentNotFound;
        }

        var comment = new Comment(request.UserId, request.PublicationId, request.Text, request.ParentCommentId);
        
        if (parentComment != null)
        {
            parentComment.AddReply(comment);
        }
        
        publication.AddComment(comment);
        
        await _context.SaveChangesAsync(cancellationToken);

        return comment.Id;
    }
}

public sealed class DeleteCommentCommandHandler : IRequestHandler<DeleteCommentCommand, ErrorOr<Deleted>>
{
    private readonly IApplicationDbContext _context;

    public DeleteCommentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<Deleted>> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = await _context.Comments
            .AsNoTracking()
            .Where(c => c.Id == request.CommentId)
            .Select(c => new
            {
                c.Id,
                c.UserId,
                c.PublicationId,
                c.ParentCommentId
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (comment == null) return CommentErrors.NotFound;

        if (comment.UserId != request.UserId)
        {
            return CommentErrors.Forbidden;
        }

        var deletedCommentsCount = await _context.Database
            .SqlQuery<int>($@"
                WITH CommentTree AS (
                    SELECT Id
                    FROM Comments
                    WHERE Id = {request.CommentId}

                    UNION ALL

                    SELECT c.Id
                    FROM Comments c
                    INNER JOIN CommentTree ct ON c.ParentCommentId = ct.Id
                )
                SELECT COUNT(1) AS [Value]
                FROM CommentTree
                OPTION (MAXRECURSION 0)")
            .SingleAsync(cancellationToken);

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        if (comment.ParentCommentId.HasValue)
        {
            await _context.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE Comments
                SET RepliesCount = CASE
                    WHEN RepliesCount > 0 THEN RepliesCount - 1
                    ELSE 0
                END
                WHERE Id = {comment.ParentCommentId.Value};",
                cancellationToken);
        }

        await _context.Database.ExecuteSqlInterpolatedAsync($@"
            ;WITH CommentTree AS (
                SELECT Id
                FROM Comments
                WHERE Id = {request.CommentId}

                UNION ALL

                SELECT c.Id
                FROM Comments c
                INNER JOIN CommentTree ct ON c.ParentCommentId = ct.Id
            )
            DELETE FROM CommentLikes
            WHERE CommentId IN (SELECT Id FROM CommentTree)
            OPTION (MAXRECURSION 0);

            ;WITH CommentTree AS (
                SELECT Id
                FROM Comments
                WHERE Id = {request.CommentId}

                UNION ALL

                SELECT c.Id
                FROM Comments c
                INNER JOIN CommentTree ct ON c.ParentCommentId = ct.Id
            )
            DELETE FROM Comments
            WHERE Id IN (SELECT Id FROM CommentTree)
            OPTION (MAXRECURSION 0);",
            cancellationToken);

        await _context.Database.ExecuteSqlInterpolatedAsync($@"
            UPDATE Publications
            SET CommentsCount = CASE
                WHEN CommentsCount >= {deletedCommentsCount} THEN CommentsCount - {deletedCommentsCount}
                ELSE 0
            END
            WHERE Id = {comment.PublicationId};",
            cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return Result.Deleted;
    }
}

public sealed class ToggleCommentLikeCommandHandler : IRequestHandler<ToggleCommentLikeCommand, ErrorOr<bool>>
{
    private readonly IApplicationDbContext _context;

    public ToggleCommentLikeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<bool>> Handle(ToggleCommentLikeCommand request, CancellationToken cancellationToken)
    {
        var comment = await _context.Comments
            .Include(c => c.Likes)
            .FirstOrDefaultAsync(c => c.Id == request.CommentId, cancellationToken);

        if (comment == null)
        {
            return CommentErrors.NotFound;
        }

        comment.ToggleLike(request.UserId);
        await _context.SaveChangesAsync(cancellationToken);

        return comment.Likes.Any(l => l.UserId == request.UserId);
    }
}
