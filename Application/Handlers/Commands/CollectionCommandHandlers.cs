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
            pub.UpdateLikesCount(-1);
            isLiked = false;
        }
        else
        {
            likesCollection.AddPublication(pub);
            pub.UpdateLikesCount(1);
            isLiked = true;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return isLiked;
    }
}

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
