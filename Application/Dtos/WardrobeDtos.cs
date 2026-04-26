using Microsoft.AspNetCore.Http;

namespace Application.Dtos;

public record AddClothRequest(
    string Name,
    string? Brand,
    string? StoreLink,
    decimal? EstimatedPrice,
    IFormFile? Photo);

public record UpdateClothRequest(
    string Name,
    string? Brand,
    string? StoreLink,
    decimal? EstimatedPrice,
    IFormFile? Photo);

public record ClothResponseDto(
    Guid Id,
    string Name,
    string? Brand,
    string? PhotoUrl,
    string? StoreLink,
    decimal? EstimatedPrice);
