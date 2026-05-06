using Application.Commands.Reports;
using Application.Interfaces;
using Application.Interfaces.Internal;
using Domain;
using Domain.Errors;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Application.Commands.Reports;

namespace Application.Handlers.Commands.Reports;

internal sealed class ResolveReportedEntityCommandHandler : IRequestHandler<ResolveReportedEntityCommand, ErrorOr<Success>>
{
    private readonly IApplicationDbContext _context;
    private readonly IDeletionService _deletionService;
    private readonly UserManager<User> _userManager;

    public ResolveReportedEntityCommandHandler(
        IApplicationDbContext context, 
        IDeletionService deletionService,
        UserManager<User> userManager)
    {
        _context = context;
        _deletionService = deletionService;
        _userManager = userManager;
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
                    await _deletionService.DeleteCommentAsync(request.TargetId, cancellationToken);
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
                if (user != null)
                {
                    if (await _userManager.IsInRoleAsync(user, "Moderator"))
                    {
                        return UserErrors.CannotBanModerator;
                    }
                    user.Ban();
                }
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
}
