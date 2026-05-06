using Application.Commands.Collections;
using Application.Interfaces;
using Domain.Errors;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Handlers.Commands.Collections;

public sealed class DeleteCollectionCommandHandler : IRequestHandler<DeleteCollectionCommand, ErrorOr<Deleted>>
{
    private readonly IApplicationDbContext _context;

    public DeleteCollectionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<Deleted>> Handle(DeleteCollectionCommand request, CancellationToken cancellationToken)
    {
        var collection = await _context.Collections
            .Include(c => c.Publications)
            .FirstOrDefaultAsync(c => c.Id == request.CollectionId, cancellationToken);

        if (collection == null) return CollectionErrors.NotFound;
        if (collection.UserId != request.UserId) return CollectionErrors.Forbidden;
        if (collection.IsSystem) return CollectionErrors.Forbidden;

        collection.ClearPublications();
        _context.Collections.Remove(collection);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Deleted;
    }
}
