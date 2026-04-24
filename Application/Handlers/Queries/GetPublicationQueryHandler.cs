using Application.Dtos;
using Application.Interfaces;
using Application.Queries;
using Domain.Errors;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Handlers.Queries;

public sealed class GetPublicationQueryHandler : IRequestHandler<GetPublicationQuery, ErrorOr<PublicationResponse>>
{
    private readonly IApplicationDbContext _context;

    public GetPublicationQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<PublicationResponse>> Handle(GetPublicationQuery request, CancellationToken cancellationToken)
    {
        var publication = await _context.Publications
            .AsNoTracking()
            .Where(p => p.Id == request.Id)
            .Select(PublicationResponse.Projection)
            .FirstOrDefaultAsync(cancellationToken);

        if (publication == null)
        {
            return PublicationErrors.NotFound;
        }

        return publication;
    }
}
