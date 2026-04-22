using Application.Dtos;
using ErrorOr;
using MediatR;

namespace Application.Queries;

public record GetTagsQuery : IRequest<ErrorOr<List<TagResponse>>>;
