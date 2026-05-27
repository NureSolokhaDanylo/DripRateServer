using Application.Commands.Collections;
using Application.Extensions;
using Application.Interfaces;
using Domain.Errors;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Handlers.Commands.Collections;

public sealed class RemoveFromCollectionCommandHandler : IRequestHandler<RemoveFromCollectionCommand, ErrorOr<Success>>
{
    private readonly IApplicationDbContext _context;

    public RemoveFromCollectionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<Success>> Handle(RemoveFromCollectionCommand request, CancellationToken cancellationToken)
    {
        var collectionResult = await _context.Collections
            .Include(c => c.CollectionPublications)
            .GetOwnedOrErrorAsync(request.CollectionId, request.UserId, CollectionErrors.Forbidden, cancellationToken);
        if (collectionResult.IsError) return collectionResult.Errors;

        var publicationResult = await _context.Publications
            .GetByIdOrErrorAsync(request.PublicationId, PublicationErrors.NotFound, cancellationToken);
        if (publicationResult.IsError) return publicationResult.Errors;

        collectionResult.Value.RemovePublication(publicationResult.Value);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}
