using Application.Dtos;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Commands.Advertisements;

public record UpdateAdvertisementCommand(
    Guid Id,
    string Text,
    string Url,
    int MaxImpressions,
    List<string> ExistingImages,
    List<IFormFile>? NewImages,
    List<Guid> TagIds,
    bool? IsActive) : IRequest<ErrorOr<AdvertisementResponse>>;
