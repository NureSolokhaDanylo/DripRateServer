using Application.Commands.Collections;
using Application.Interfaces;
using Domain;
using Domain.Errors;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Handlers.Commands.Collections;

public sealed class CreateCollectionCommandHandler : IRequestHandler<CreateCollectionCommand, ErrorOr<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateCollectionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<Guid>> Handle(CreateCollectionCommand request, CancellationToken cancellationToken)
    {
        var nameExists = await _context.Collections
            .AnyAsync(c => c.UserId == request.UserId && c.Name == request.Name, cancellationToken);

        if (nameExists)
        {
            return CollectionErrors.NameAlreadyExists;
        }

        var collection = Collection.CreateUserDefined(request.UserId, request.Name, request.Description, request.IsPublic);
        _context.Collections.Add(collection);
        await _context.SaveChangesAsync(cancellationToken);
        return collection.Id;
    }
}
