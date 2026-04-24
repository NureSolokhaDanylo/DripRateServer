using Application.Commands;
using Application.Extensions;
using Application.Interfaces;
using Application.Interfaces.Internal;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Handlers.Commands;

public sealed class DeletePublicationCommandHandler : IRequestHandler<DeletePublicationCommand, ErrorOr<Deleted>>
{
    private readonly IApplicationDbContext _context;
    private readonly IDeletionService _deletionService;

    public DeletePublicationCommandHandler(IApplicationDbContext context, IDeletionService deletionService)
    {
        _context = context;
        _deletionService = deletionService;
    }

    public async Task<ErrorOr<Deleted>> Handle(DeletePublicationCommand request, CancellationToken cancellationToken)
    {
        var pubResult = await _context.Publications.GetOwnedOrErrorAsync(request.PublicationId, request.UserId, cancellationToken);
        if (pubResult.IsError) return pubResult.Errors;

        var pub = pubResult.Value;

        // Rule 8 (Restrict): Delegate complex cleanup to internal service
        await _deletionService.DeletePublicationContentAsync(request.PublicationId, cancellationToken);

        _context.Publications.Remove(pub);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Deleted;
    }
}
