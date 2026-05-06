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
