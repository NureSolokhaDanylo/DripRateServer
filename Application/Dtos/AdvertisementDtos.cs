using Microsoft.AspNetCore.Http;

namespace Application.Dtos;

public record CreateAdvertisementRequest(
    string Text,
    string Url,
    int MaxImpressions,
    List<IFormFile> Images,
    List<Guid> TagIds);

public record UpdateAdvertisementRequest(
    string Text,
    string Url,
    int MaxImpressions,
    List<string> ExistingImages,
    List<IFormFile>? NewImages,
    List<Guid> TagIds,
    bool? IsActive);

public record AdvertisementResponse(
    Guid Id,
    List<string> Images,
    string Text,
    string Url,
    int MaxImpressions,
    int ShownCount,
    bool IsActive,
    List<TagResponse> Tags,
    DateTimeOffset CreatedAt);
