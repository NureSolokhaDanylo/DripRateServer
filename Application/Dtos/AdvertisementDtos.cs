using Microsoft.AspNetCore.Http;

namespace Application.Dtos;

public record CreateAdvertisementRequest(
    string Text,
    int MaxImpressions,
    List<IFormFile> Images,
    List<Guid> TagIds);

public record UpdateAdvertisementRequest(
    string Text,
    int MaxImpressions,
    List<string> ExistingImages,
    List<IFormFile>? NewImages,
    List<Guid> TagIds);

public record AdvertisementResponse(
    Guid Id,
    List<string> Images,
    string Text,
    int MaxImpressions,
    int ShownCount,
    List<TagResponse> Tags,
    DateTimeOffset CreatedAt);
