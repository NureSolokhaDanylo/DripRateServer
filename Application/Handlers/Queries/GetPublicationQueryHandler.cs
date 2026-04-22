using Application.Dtos;
using Application.Interfaces;
using Application.Queries;
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
        var result = await _context.Publications
            .AsNoTracking()
            .Where(p => p.Id == request.Id)
            .Select(p => new PublicationResponse(
                p.Id,
                p.Description,
                p.Images.FirstOrDefault() ?? string.Empty,
                p.CreatedAt,
                p.UserId,
                p.User.UserName ?? string.Empty,
                p.Tags.Select(t => new TagResponse(t.Id, t.Name, t.Category)).ToList(),
                p.Clothes.Select(c => new ClothResponse(c.Id, c.Name, c.Brand, c.PhotoUrl)).ToList()
            ))
            .FirstOrDefaultAsync(cancellationToken);

        if (result == null)
        {
            return Error.NotFound(description: "Publication not found.");
        }

        return result;
    }
}
