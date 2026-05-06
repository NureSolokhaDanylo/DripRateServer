using Application.Commands.Collections;
using Application.Extensions;
using Application.Interfaces;
using Domain;
using Domain.Errors;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Handlers.Commands.Collections;

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
