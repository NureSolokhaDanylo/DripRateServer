using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasMany(u => u.Publications)
            .WithOne(p => p.User)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.Wardrobe)
            .WithOne(c => c.User)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(u => u.AvatarUrl).IsRequired().HasMaxLength(2048).HasField("_avatarUrl");
        builder.Property(u => u.Bio).HasMaxLength(500).HasField("_bio");
        builder.Property(u => u.CreatedAt).HasField("_createdAt");

        // Навигация через приватные поля
        builder.Navigation(u => u.Publications).Metadata.SetField("_publications");
        builder.Navigation(u => u.Wardrobe).Metadata.SetField("_wardrobe");
        builder.Navigation(u => u.Followers).Metadata.SetField("_followers");
        builder.Navigation(u => u.Following).Metadata.SetField("_following");
        builder.Navigation(u => u.Collections).Metadata.SetField("_collections");
        builder.Navigation(u => u.PreferredTags).Metadata.SetField("_preferredTags");

        builder.HasMany(u => u.PreferredTags)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "UserPreferredTag",
                j => j.HasOne<Tag>().WithMany().HasForeignKey("TagId").OnDelete(DeleteBehavior.Restrict),
                j => j.HasOne<User>().WithMany().HasForeignKey("UserId").OnDelete(DeleteBehavior.Restrict),
                j =>
                {
                    j.ToTable("UserPreferredTags");
                    j.HasIndex("TagId");
                });
    }
}
