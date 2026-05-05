using Application.Dtos;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Commands;

public record CreateAdvertisementCommand(
    string Text,
    int MaxImpressions,
    List<IFormFile> Images,
    List<Guid> TagIds) : IRequest<ErrorOr<AdvertisementResponse>>;

public record UpdateAdvertisementCommand(
    Guid Id,
    string Text,
    int MaxImpressions,
    List<string> ExistingImages,
    List<IFormFile>? NewImages,
    List<Guid> TagIds) : IRequest<ErrorOr<AdvertisementResponse>>;

public record DeleteAdvertisementCommand(Guid Id) : IRequest<ErrorOr<Deleted>>;

public record ViewAdvertisementCommand(Guid AdId, Guid UserId) : IRequest<ErrorOr<Success>>;
