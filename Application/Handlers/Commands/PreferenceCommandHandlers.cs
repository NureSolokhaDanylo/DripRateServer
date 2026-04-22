using Application.Commands;
using Application.Interfaces;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Handlers.Commands;

public sealed class SetPreferencesCommandHandler : IRequestHandler<SetPreferencesCommand, ErrorOr<Success>>
{
    private readonly IApplicationDbContext _context;

    public SetPreferencesCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<Success>> Handle(SetPreferencesCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.PreferredTags)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null) return Error.NotFound(description: "User not found.");

        var tags = await _context.Tags
            .Where(t => request.TagIds.Contains(t.Id))
            .ToListAsync(cancellationToken);

        user.SetPreferredTags(tags);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}
