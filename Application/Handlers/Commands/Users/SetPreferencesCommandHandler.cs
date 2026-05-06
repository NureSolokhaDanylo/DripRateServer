using Application.Commands.Users;
using Application.Interfaces;
using Domain.Errors;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Handlers.Commands.Users;

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

        if (user == null)
        {
            return UserErrors.NotFound;
        }

        var tags = await _context.Tags
            .Where(t => request.TagIds.Contains(t.Id))
            .ToListAsync(cancellationToken);

        user.SetPreferredTags(tags);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}
