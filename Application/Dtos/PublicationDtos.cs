using System.Linq.Expressions;
using Domain;
using Microsoft.AspNetCore.Http;

namespace Application.Dtos;

public record CreatePublicationRequest(
    string Description,
    IFormFile Image,
    List<Guid>? TagIds,
    List<Guid>? ClothIds);

public record PublicationResponse(
    Guid Id,
    string Description,
    string ImageUrl,
    DateTimeOffset CreatedAt,
    Guid UserId,
    string Username,
    List<TagResponse> Tags,
    List<ClothResponse> Clothes)
{
    public static Expression<Func<Publication, PublicationResponse>> Projection => p => new PublicationResponse(
        p.Id,
        p.Description,
        p.Images.FirstOrDefault() ?? string.Empty,
        p.CreatedAt,
        p.UserId,
        p.User.UserName ?? string.Empty,
        p.Tags.Select(t => new TagResponse(t.Id, t.Name, t.Category)).ToList(),
        p.Clothes.Select(c => new ClothResponse(c.Id, c.Name, c.Brand, c.PhotoUrl)).ToList()
    );
}

public record TagResponse(Guid Id, string Name, string Category)
{
    public static Expression<Func<Tag, TagResponse>> Projection => t => new TagResponse(t.Id, t.Name, t.Category);
}

public record ClothResponse(
    Guid Id,
    string Name,
    string? Brand,
    string? PhotoUrl)
{
    public static Expression<Func<Cloth, ClothResponse>> Projection => c => new ClothResponse(c.Id, c.Name, c.Brand, c.PhotoUrl);
}
