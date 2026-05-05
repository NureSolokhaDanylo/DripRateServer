using Application.Commands;
using Application.Interfaces;
using Application.Interfaces.Internal;
using Domain;
using Domain.Errors;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Handlers.Commands;

internal sealed class CreateReportCommandHandler : IRequestHandler<CreateReportCommand, ErrorOr<Success>>
{
    private readonly IApplicationDbContext _context;

    public CreateReportCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<Success>> Handle(CreateReportCommand request, CancellationToken cancellationToken)
    {
        // 1. Check for duplicate report
        var existingReport = await _context.Reports
            .AnyAsync(r => r.AuthorId == request.AuthorId && 
                           r.TargetType == request.TargetType && 
                           r.TargetId == request.TargetId && 
                           (r.Status == ReportStatus.Pending || r.Status == ReportStatus.InReview), 
                cancellationToken);

        if (existingReport) return ReportErrors.DuplicateReport;

        // 2. Check target existence and self-reporting
        Guid ownerId = Guid.Empty;
        bool targetExists = false;

        switch (request.TargetType)
        {
            case ReportTargetType.Publication:
                var pub = await _context.Publications
                    .Where(p => p.Id == request.TargetId)
                    .Select(p => new { p.UserId })
                    .FirstOrDefaultAsync(cancellationToken);
                if (pub != null)
                {
                    targetExists = true;
                    ownerId = pub.UserId;
                }
                break;
            case ReportTargetType.Comment:
                var comment = await _context.Comments
                    .Where(c => c.Id == request.TargetId)
                    .Select(c => new { c.UserId })
                    .FirstOrDefaultAsync(cancellationToken);
                if (comment != null)
                {
                    targetExists = true;
                    ownerId = comment.UserId;
                }
                break;
            case ReportTargetType.User:
                targetExists = await _context.Users.AnyAsync(u => u.Id == request.TargetId, cancellationToken);
                ownerId = request.TargetId;
                break;
        }

        if (!targetExists) return ReportErrors.InvalidTarget;
        if (ownerId == request.AuthorId) return ReportErrors.SelfReport;

        var report = new Report(request.AuthorId, request.TargetType, request.TargetId, request.Category, request.Text);
        _context.Reports.Add(report);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}

internal sealed class AssignReportedEntityCommandHandler : IRequestHandler<AssignReportedEntityCommand, ErrorOr<Success>>
{
    private readonly IApplicationDbContext _context;

    public AssignReportedEntityCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<Success>> Handle(AssignReportedEntityCommand request, CancellationToken cancellationToken)
    {
        var reports = await _context.Reports
            .Where(r => r.TargetType == request.TargetType && r.TargetId == request.TargetId && r.Status != ReportStatus.Resolved && r.Status != ReportStatus.Dismissed)
            .ToListAsync(cancellationToken);

        if (!reports.Any()) return ReportErrors.NotFound;

        if (reports.Any(r => r.AssignedToUserId.HasValue && r.AssignedToUserId != request.ModeratorId))
        {
            return ReportErrors.AlreadyAssigned;
        }

        foreach (var report in reports)
        {
            report.AssignTo(request.ModeratorId);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success;
    }
}

internal sealed class ResolveReportedEntityCommandHandler : IRequestHandler<ResolveReportedEntityCommand, ErrorOr<Success>>
{
    private readonly IApplicationDbContext _context;
    private readonly IDeletionService _deletionService;

    public ResolveReportedEntityCommandHandler(IApplicationDbContext context, IDeletionService deletionService)
    {
        _context = context;
        _deletionService = deletionService;
    }

    public async Task<ErrorOr<Success>> Handle(ResolveReportedEntityCommand request, CancellationToken cancellationToken)
    {
        var reports = await _context.Reports
            .Where(r => r.TargetType == request.TargetType && r.TargetId == request.TargetId && r.Status != ReportStatus.Resolved && r.Status != ReportStatus.Dismissed)
            .ToListAsync(cancellationToken);

        if (!reports.Any()) return ReportErrors.NotFound;

        // Check if reports are assigned to someone else
        if (reports.Any(r => r.AssignedToUserId.HasValue && r.AssignedToUserId != request.ModeratorId))
        {
            return ReportErrors.Unauthorized;
        }

        // Perform action
        if (request.Action == ModerationAction.DeleteEntity)
        {
            switch (request.TargetType)
            {
                case ReportTargetType.Publication:
                    var pub = await _context.Publications.FindAsync(new object[] { request.TargetId }, cancellationToken);
                    if (pub != null)
                    {
                        await _deletionService.DeletePublicationContentAsync(request.TargetId, cancellationToken);
                        _context.Publications.Remove(pub);
                    }
                    break;
                case ReportTargetType.Comment:
                    await DeleteCommentAsync(request.TargetId, cancellationToken);
                    break;
                case ReportTargetType.User:
                    var user = await _context.Users.FindAsync(new object[] { request.TargetId }, cancellationToken);
                    if (user != null)
                    {
                        await _deletionService.DeleteUserContentAsync(request.TargetId, cancellationToken);
                        _context.Users.Remove(user);
                    }
                    break;
            }
        }
        else if (request.Action == ModerationAction.BanUser)
        {
            var userId = request.TargetType switch
            {
                ReportTargetType.User => request.TargetId,
                ReportTargetType.Publication => (await _context.Publications.Where(p => p.Id == request.TargetId).Select(p => p.UserId).FirstOrDefaultAsync(cancellationToken)),
                ReportTargetType.Comment => (await _context.Comments.Where(c => c.Id == request.TargetId).Select(c => c.UserId).FirstOrDefaultAsync(cancellationToken)),
                _ => Guid.Empty
            };

            if (userId != Guid.Empty)
            {
                var user = await _context.Users.FindAsync(new object[] { userId }, cancellationToken);
                user?.Ban();
            }
        }

        var finalStatus = request.Action == ModerationAction.Dismiss ? ReportStatus.Dismissed : ReportStatus.Resolved;
        foreach (var report in reports)
        {
            report.Resolve(finalStatus);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success;
    }

    private async Task DeleteCommentAsync(Guid commentId, CancellationToken cancellationToken)
    {
        var comment = await _context.Comments
            .AsNoTracking()
            .Where(c => c.Id == commentId)
            .Select(c => new { c.Id, c.PublicationId, c.ParentCommentId })
            .FirstOrDefaultAsync(cancellationToken);

        if (comment == null) return;

        var deletedCommentsCount = await _context.Database
            .SqlQuery<int>($@"
                WITH CommentTree AS (
                    SELECT Id
                    FROM Comments
                    WHERE Id = {commentId}
                    UNION ALL
                    SELECT c.Id
                    FROM Comments c
                    INNER JOIN CommentTree ct ON c.ParentCommentId = ct.Id
                )
                SELECT COUNT(1) AS [Value]
                FROM CommentTree")
            .SingleAsync(cancellationToken);

        if (comment.ParentCommentId.HasValue)
        {
            await _context.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE Comments SET RepliesCount = CASE WHEN RepliesCount > 0 THEN RepliesCount - 1 ELSE 0 END
                WHERE Id = {comment.ParentCommentId.Value};", cancellationToken);
        }

        await _context.Database.ExecuteSqlInterpolatedAsync($@"
            ;WITH CommentTree AS (
                SELECT Id FROM Comments WHERE Id = {commentId}
                UNION ALL
                SELECT c.Id FROM Comments c INNER JOIN CommentTree ct ON c.ParentCommentId = ct.Id
            )
            DELETE FROM CommentLikes WHERE CommentId IN (SELECT Id FROM CommentTree);

            ;WITH CommentTree AS (
                SELECT Id FROM Comments WHERE Id = {commentId}
                UNION ALL
                SELECT c.Id FROM Comments c INNER JOIN CommentTree ct ON c.ParentCommentId = ct.Id
            )
            DELETE FROM Comments WHERE Id IN (SELECT Id FROM CommentTree);", cancellationToken);

        await _context.Database.ExecuteSqlInterpolatedAsync($@"
            UPDATE Publications SET CommentsCount = CASE WHEN CommentsCount >= {deletedCommentsCount} THEN CommentsCount - {deletedCommentsCount} ELSE 0 END
            WHERE Id = {comment.PublicationId};", cancellationToken);
    }
}
