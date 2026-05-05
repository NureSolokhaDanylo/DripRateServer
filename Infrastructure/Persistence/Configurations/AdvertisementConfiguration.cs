using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class AdvertisementConfiguration : IEntityTypeConfiguration<Advertisement>
{
    public void Configure(EntityTypeBuilder<Advertisement> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasField("_id");

        builder.Property(a => a.Text).HasMaxLength(4000).HasField("_text");
        builder.Property(a => a.MaxImpressions).HasField("_maxImpressions");
        builder.Property(a => a.ShownCount).HasField("_shownCount");
        builder.Property(a => a.IsActive).HasField("_isActive");
        builder.Property(a => a.CreatedAt).HasField("_createdAt");

        builder.HasIndex(a => a.IsActive);

        builder.Property<List<string>>("_images")
            .HasColumnName("Images");

        builder.HasMany(a => a.Tags)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "AdvertisementTag",
                j => j.HasOne<Tag>().WithMany().HasForeignKey("TagId").OnDelete(DeleteBehavior.Restrict),
                j => j.HasOne<Advertisement>().WithMany().HasForeignKey("AdvertisementId").OnDelete(DeleteBehavior.Restrict),
                j =>
                {
                    j.ToTable("AdvertisementTags");
                    j.HasIndex("TagId");
                });

        builder.HasMany(a => a.Views)
            .WithOne(v => v.Advertisement)
            .HasForeignKey(v => v.AdvertisementId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(a => a.Tags).Metadata.SetField("_tags");
        builder.Navigation(a => a.Views).Metadata.SetField("_views");
    }
}

public sealed class AdvertisementViewConfiguration : IEntityTypeConfiguration<AdvertisementView>
{
    public void Configure(EntityTypeBuilder<AdvertisementView> builder)
    {
        builder.HasKey(v => new { v.AdvertisementId, v.UserId });

        builder.Property(v => v.AdvertisementId).HasField("_advertisementId");
        builder.Property(v => v.UserId).HasField("_userId");
        builder.Property(v => v.ViewedAt).HasField("_viewedAt");

        builder.HasOne(v => v.Advertisement)
            .WithMany(a => a.Views)
            .HasForeignKey(v => v.AdvertisementId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(v => v.User)
            .WithMany()
            .HasForeignKey(v => v.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(v => v.Advertisement).Metadata.SetField("_advertisement");
        builder.Navigation(v => v.User).Metadata.SetField("_user");
    }
}
