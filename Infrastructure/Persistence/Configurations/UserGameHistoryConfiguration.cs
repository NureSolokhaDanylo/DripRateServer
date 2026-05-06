using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class UserGameHistoryConfiguration : IEntityTypeConfiguration<UserGameHistory>
{
    public void Configure(EntityTypeBuilder<UserGameHistory> builder)
    {
        builder.HasKey(h => new { h.UserId, h.PublicationId, h.GameType });

        builder.HasQueryFilter(h => !h.User.IsBanned && !h.Publication.User.IsBanned);

        builder.Property(h => h.CreatedAt)
            .IsRequired();

        builder.HasOne(h => h.User)
            .WithMany()
            .HasForeignKey(h => h.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(h => h.Publication)
            .WithMany()
            .HasForeignKey(h => h.PublicationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
