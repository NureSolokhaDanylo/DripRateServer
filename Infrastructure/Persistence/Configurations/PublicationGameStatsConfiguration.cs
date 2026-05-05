using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class PublicationGameStatsConfiguration : IEntityTypeConfiguration<PublicationGameStats>
{
    public void Configure(EntityTypeBuilder<PublicationGameStats> builder)
    {
        builder.HasKey(s => s.PublicationId);
        
        builder.Property(s => s.GuessPriceTotalCount).HasDefaultValue(0);
        builder.Property(s => s.GuessPriceRealSum).HasPrecision(18, 2).HasDefaultValue(0);
        builder.Property(s => s.GuessPriceGuessedSum).HasPrecision(18, 2).HasDefaultValue(0);

        builder.Property(s => s.FirstImpressionTotalCount).HasDefaultValue(0);
        builder.Property(s => s.FirstImpressionPositiveCount).HasDefaultValue(0);
        builder.Property(s => s.FirstImpressionReactionTimeSum).HasDefaultValue(0);

        builder.Property(s => s.TagMatchTotalCount).HasDefaultValue(0);
        builder.Property(s => s.TagMatchCorrectCount).HasDefaultValue(0);
    }
}
