namespace Application.Dtos;

public record GlobalFeedResponse(
    List<PublicationResponse> Publications,
    List<AdvertisementResponse> Advertisements);
