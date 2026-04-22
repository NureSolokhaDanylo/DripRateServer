using Application.Commands;
using Application.Interfaces;
using Domain;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Handlers.Commands;

public sealed class CreateCollectionCommandHandler : IRequestHandler<CreateCollectionCommand, ErrorOr<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateCollectionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<Guid>> Handle(CreateCollectionCommand request, CancellationToken cancellationToken)
    {
        var collection = Collection.CreateUserDefined(request.UserId, request.Name, request.Description, request.IsPublic);
        _context.Collections.Add(collection);
        await _context.SaveChangesAsync(cancellationToken);

        return collection.Id;
    }
}

public sealed class AddToCollectionCommandHandler : IRequestHandler<AddToCollectionCommand, ErrorOr<Success>>
{
    private readonly IApplicationDbContext _context;

    public AddToCollectionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<Success>> Handle(AddToCollectionCommand request, CancellationToken cancellationToken)
    {
        var collection = await _context.Collections
            .Include(c => c.Publications)
            .FirstOrDefaultAsync(c => c.Id == request.CollectionId && c.UserId == request.UserId, cancellationToken);

        if (collection == null) return Error.NotFound(description: "Collection not found.");

        var publication = await _context.Publications.FindAsync(new object[] { request.PublicationId }, cancellationToken);
        if (publication == null) return Error.NotFound(description: "Publication not found.");

        collection.AddPublication(publication);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}

public sealed class RemoveFromCollectionCommandHandler : IRequestHandler<RemoveFromCollectionCommand, ErrorOr<Success>>
{
    private readonly IApplicationDbContext _context;

    public RemoveFromCollectionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<Success>> Handle(RemoveFromCollectionCommand request, CancellationToken cancellationToken)
    {
        var collection = await _context.Collections
            .Include(c => c.Publications)
            .FirstOrDefaultAsync(c => c.Id == request.CollectionId && c.UserId == request.UserId, cancellationToken);

        if (collection == null) return Error.NotFound(description: "Collection not found.");

        var publication = await _context.Publications.FindAsync(new object[] { request.PublicationId }, cancellationToken);
        if (publication == null) return Error.NotFound(description: "Publication not found.");

        collection.RemovePublication(publication);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}

public sealed class ToggleLikeCommandHandler : IRequestHandler<ToggleLikeCommand, ErrorOr<bool>>
{
    private readonly IApplicationDbContext _context;

    public ToggleLikeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<bool>> Handle(ToggleLikeCommand request, CancellationToken cancellationToken)
    {
        var likesCollection = await _context.Collections
            .Include(c => c.Publications)
            .FirstOrDefaultAsync(c => c.UserId == request.UserId && c.Type == CollectionType.SystemLikes, cancellationToken);

        if (likesCollection == null) return Error.Failure(description: "Likes collection not initialized.");

        var publication = await _context.Publications.FindAsync(new object[] { request.PublicationId }, cancellationToken);
        if (publication == null) return Error.NotFound(description: "Publication not found.");

        bool isLiked;
        if (likesCollection.Publications.Any(p => p.Id == request.PublicationId))
        {
            likesCollection.RemovePublication(publication);
            isLiked = false;
        }
        else
        {
            likesCollection.AddPublication(publication);
            isLiked = true;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return isLiked;
    }
}
