using Application.Commands;
using Application.Extensions;
using Application.Interfaces;
using Application.Interfaces.Internal;
using Domain.Errors;
using ErrorOr;
using MediatR;

namespace Application.Handlers.Commands;

internal sealed class DeletePublicationCommandHandler : IRequestHandler<DeletePublicationCommand, ErrorOr<Deleted>>
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
        var pubResult = await _context.Publications.GetOwnedOrErrorAsync(
            request.PublicationId, 
            request.UserId, 
            PublicationErrors.Forbidden, 
            cancellationToken);
            
        if (pubResult.IsError) return pubResult.Errors;

        await _deletionService.DeletePublicationContentAsync(request.PublicationId, cancellationToken);
        
        _context.Publications.Remove(pubResult.Value);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Deleted;
    }
}
