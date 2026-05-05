using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class UserGameCursorConfiguration : IEntityTypeConfiguration<UserGameCursor>
{
    public void Configure(EntityTypeBuilder<UserGameCursor> builder)
    {
        builder.HasKey(c => new { c.UserId, c.GameType });

        builder.HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
