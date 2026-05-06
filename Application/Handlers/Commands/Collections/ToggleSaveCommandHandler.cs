using Application.Commands.Collections;
using Application.Extensions;
using Application.Interfaces;
using Domain;
using Domain.Errors;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Handlers.Commands.Collections;

public sealed class ToggleSaveCommandHandler : IRequestHandler<ToggleSaveCommand, ErrorOr<bool>>
{
    private readonly IApplicationDbContext _context;

    public ToggleSaveCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<bool>> Handle(ToggleSaveCommand request, CancellationToken cancellationToken)
    {
        var savedCollection = await _context.Collections
            .Include(c => c.Publications)
            .FirstOrDefaultAsync(c => c.UserId == request.UserId && c.Type == CollectionType.SystemSaved, cancellationToken);

        if (savedCollection == null) return CollectionErrors.LikesNotInitialized; // Or create a generic one

        var publicationResult = await _context.Publications
            .GetByIdOrErrorAsync(request.PublicationId, PublicationErrors.NotFound, cancellationToken);
        if (publicationResult.IsError) return publicationResult.Errors;

        var pub = publicationResult.Value;
        bool isSaved;

        if (savedCollection.Publications.Any(p => p.Id == request.PublicationId))
        {
            savedCollection.RemovePublication(pub);
            isSaved = false;
        }
        else
        {
            savedCollection.AddPublication(pub);
            isSaved = true;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return isSaved;
    }
}
