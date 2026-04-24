using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class PublicationConfiguration : IEntityTypeConfiguration<Publication>
{
    public void Configure(EntityTypeBuilder<Publication> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasField("_id");

        builder.Property(p => p.Description).HasMaxLength(2000).HasField("_description");
        builder.Property(p => p.CreatedAt).HasField("_createdAt");
        builder.Property(p => p.UserId).HasField("_userId");

        builder.HasOne(p => p.User)
            .WithMany(u => u.Publications)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Мапим поле напрямую для коллекции строк
        builder.Property<List<string>>("_images")
            .HasColumnName("Images");

        // Many-to-Many с тегами
        builder.HasMany(p => p.Tags)
            .WithMany()
            .UsingEntity(j => j.ToTable("PublicationTags"));

        // Many-to-Many с одеждой
        builder.HasMany(p => p.Clothes)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "PublicationClothes",
                j => j.HasOne<Cloth>().WithMany().HasForeignKey("ClothesId").OnDelete(DeleteBehavior.Restrict),
                j => j.HasOne<Publication>().WithMany().HasForeignKey("PublicationId").OnDelete(DeleteBehavior.Restrict));

        builder.HasMany(p => p.Comments)
            .WithOne(c => c.Publication)
            .HasForeignKey(c => c.PublicationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Assessments)
            .WithOne(a => a.Publication)
            .HasForeignKey(a => a.PublicationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(p => p.User).Metadata.SetField("_user");
        builder.Navigation(p => p.Tags).Metadata.SetField("_tags");
        builder.Navigation(p => p.Clothes).Metadata.SetField("_clothes");
        builder.Navigation(p => p.Comments).Metadata.SetField("_comments");
        builder.Navigation(p => p.Assessments).Metadata.SetField("_assessments");
        builder.Navigation(p => p.Collections).Metadata.SetField("_collections");
    }
}
