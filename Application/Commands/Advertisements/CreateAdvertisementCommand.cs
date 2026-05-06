using Application.Dtos;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Commands.Advertisements;

public record CreateAdvertisementCommand(
    string Text,
    string Url,
    int MaxImpressions,
    List<IFormFile> Images,
    List<Guid> TagIds) : IRequest<ErrorOr<AdvertisementResponse>>;
