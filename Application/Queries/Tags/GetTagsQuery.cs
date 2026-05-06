using Application.Dtos;
using ErrorOr;
using MediatR;

namespace Application.Queries.Tags;

public record GetTagsQuery : IRequest<ErrorOr<List<TagResponse>>>;
