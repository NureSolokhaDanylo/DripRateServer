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
    List<ClothResponse> Clothes);

public record TagResponse(Guid Id, string Name, string Category);

public record ClothResponse(
    Guid Id,
    string Name,
    string? Brand,
    string? PhotoUrl);
