using Application.Commands;
using Application.Extensions;
using Application.Interfaces;
using Domain;
using Domain.Errors;
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

        if (likesCollection == null) return CollectionErrors.LikesNotInitialized;

        var publicationResult = await _context.Publications
            .GetByIdOrErrorAsync(request.PublicationId, PublicationErrors.NotFound, cancellationToken);
        if (publicationResult.IsError) return publicationResult.Errors;

        var pub = publicationResult.Value;
        bool isLiked;

        if (likesCollection.Publications.Any(p => p.Id == request.PublicationId))
        {
            likesCollection.RemovePublication(pub);
            isLiked = false;
        }
        else
        {
            likesCollection.AddPublication(pub);
            isLiked = true;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return isLiked;
    }
}
