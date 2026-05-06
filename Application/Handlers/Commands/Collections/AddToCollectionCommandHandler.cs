using Application.Commands.Collections;
using Application.Extensions;
using Application.Interfaces;
using Domain.Errors;
using ErrorOr;
using MediatR;

namespace Application.Handlers.Commands.Collections;

public sealed class AddToCollectionCommandHandler : IRequestHandler<AddToCollectionCommand, ErrorOr<Success>>
{
    private readonly IApplicationDbContext _context;

    public AddToCollectionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<Success>> Handle(AddToCollectionCommand request, CancellationToken cancellationToken)
    {
        var collectionResult = await _context.Collections
            .GetOwnedOrErrorAsync(request.CollectionId, request.UserId, CollectionErrors.Forbidden, cancellationToken);
        if (collectionResult.IsError) return collectionResult.Errors;

        var publicationResult = await _context.Publications
            .GetByIdOrErrorAsync(request.PublicationId, PublicationErrors.NotFound, cancellationToken);
        if (publicationResult.IsError) return publicationResult.Errors;

        collectionResult.Value.AddPublication(publicationResult.Value);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}
