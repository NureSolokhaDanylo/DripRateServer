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
            if (request.TargetType == ReportTargetType.User)
            {
                return ReportErrors.CannotDeleteUser;
            }

            switch (request.TargetType)
            {
                case ReportTargetType.Publication:
                    var pub = await _context.Publications
                        .IgnoreQueryFilters()
                        .FirstOrDefaultAsync(p => p.Id == request.TargetId, cancellationToken);
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
            if (request.TargetType != ReportTargetType.User)
            {
                return ReportErrors.CannotBanEntity;
            }

            var userId = request.TargetId;

            if (userId != Guid.Empty)
            {
                var user = await _context.Users
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
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
