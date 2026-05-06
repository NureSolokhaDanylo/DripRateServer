using System.Linq.Expressions;
using Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Application.Dtos;

public record CreatePublicationRequest(
    string Description,
    IFormFile? Image,
    List<IFormFile>? Images,
    List<Guid>? TagIds,
    List<Guid>? ClothIds);

public record PublicationResponse(
    Guid Id,
    string Description,
    string ImageUrl,
    List<string> ImageUrls,
    DateTimeOffset CreatedAt,
    Guid UserId,
    string UserDisplayName,
    List<TagResponse> Tags,
    List<ClothResponse> Clothes,
    int LikesCount,
    int CommentsCount,
    int AssessmentsCount,
    double AverageRating,
    double AverageColorCoordination,
    double AverageFitAndProportions,
    double AverageOriginality,
    double AverageOverallStyle,
    bool IsLikedByMe,
    bool IsSavedByMe,
    PublicationGameStatsResponse? GameStats)
{
    public static Expression<Func<Publication, PublicationResponse>> GetProjection(Guid? currentUserId) => p => new PublicationResponse(
        p.Id,
        p.Description,
        p.Images.FirstOrDefault() ?? string.Empty,
        p.Images.ToList(),
        p.CreatedAt,
        p.UserId,
        p.User.DisplayName,
        p.Tags.Select(t => new TagResponse(t.Id, t.Name, t.Category)).ToList(),
        p.Clothes.Select(c => new ClothResponse(c.Id, c.Name, c.Brand, c.PhotoUrl)).ToList(),
        p.LikesCount,
        p.CommentsCount,
        p.AssessmentsCount,
        p.AverageRating,
        p.AssessmentsCount > 0 ? (double)EF.Property<int>(p, "_ratingColorSum") / p.AssessmentsCount : 0,
        p.AssessmentsCount > 0 ? (double)EF.Property<int>(p, "_ratingFitSum") / p.AssessmentsCount : 0,
        p.AssessmentsCount > 0 ? (double)EF.Property<int>(p, "_ratingOriginalitySum") / p.AssessmentsCount : 0,
        p.AssessmentsCount > 0 ? (double)EF.Property<int>(p, "_ratingStyleSum") / p.AssessmentsCount : 0,
        currentUserId.HasValue && p.Collections.Any(c => c.Type == CollectionType.SystemLikes && c.UserId == currentUserId.Value),
        currentUserId.HasValue && p.Collections.Any(c => c.Type == CollectionType.SystemSaved && c.UserId == currentUserId.Value),
        p.GameStats != null ? new PublicationGameStatsResponse(
            new GuessPriceStatsResponse(
                p.GameStats.GuessPriceTotalCount,
                p.GameStats.GuessPriceTotalCount > 0 ? p.GameStats.GuessPriceGuessedSum / p.GameStats.GuessPriceTotalCount : 0),
            new FirstImpressionStatsResponse(
                p.GameStats.FirstImpressionTotalCount,
                p.GameStats.FirstImpressionTotalCount > 0 ? (double)p.GameStats.FirstImpressionPositiveCount / p.GameStats.FirstImpressionTotalCount * 100 : 0,
                p.GameStats.FirstImpressionTotalCount > 0 ? (double)p.GameStats.FirstImpressionReactionTimeSum / p.GameStats.FirstImpressionTotalCount : 0),
            new TagMatchStatsResponse(
                p.GameStats.TagMatchTotalCount,
                p.GameStats.TagMatchTotalCount > 0 ? (double)p.GameStats.TagMatchCorrectCount / p.GameStats.TagMatchTotalCount : 0)
        ) : null
    );
}

public record PublicationGameStatsResponse(
    GuessPriceStatsResponse GuessPrice,
    FirstImpressionStatsResponse FirstImpression,
    TagMatchStatsResponse TagMatch);

public record GuessPriceStatsResponse(int TotalAttempts, decimal AverageGuessedPrice);
public record FirstImpressionStatsResponse(int TotalAttempts, double PositivePercentage, double AverageReactionTimeMs);
public record TagMatchStatsResponse(int TotalAttempts, double AverageCorrectTags);

public record TagResponse(Guid Id, string Name, string Category)
{
    public static Expression<Func<Tag, TagResponse>> Projection => t => new TagResponse(t.Id, t.Name, t.Category);
}

public record ClothResponse(
    Guid Id,
    string Name,
    string? Brand,
    string? PhotoUrl)
{
    public static Expression<Func<Cloth, ClothResponse>> Projection => c => new ClothResponse(c.Id, c.Name, c.Brand, c.PhotoUrl);
}
