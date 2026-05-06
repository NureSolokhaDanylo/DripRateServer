using Application.Commands.Collections;
using Application.Interfaces;
using Domain.Errors;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Handlers.Commands.Collections;

public sealed class UpdateCollectionCommandHandler : IRequestHandler<UpdateCollectionCommand, ErrorOr<Success>>
{
    private readonly IApplicationDbContext _context;

    public UpdateCollectionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<Success>> Handle(UpdateCollectionCommand request, CancellationToken cancellationToken)
    {
        var collection = await _context.Collections
            .FirstOrDefaultAsync(c => c.Id == request.CollectionId, cancellationToken);

        if (collection == null) return CollectionErrors.NotFound;
        if (collection.UserId != request.UserId) return CollectionErrors.Forbidden;
        if (collection.IsSystem) return CollectionErrors.Forbidden; // Cannot update system collections

        var nameExists = await _context.Collections
            .AnyAsync(c => c.UserId == request.UserId && c.Name == request.Name && c.Id != request.CollectionId, cancellationToken);

        if (nameExists)
        {
            return CollectionErrors.NameAlreadyExists;
        }

        collection.Update(request.Name, request.Description, request.IsPublic);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}
