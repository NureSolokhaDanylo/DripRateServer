using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class CollectionConfiguration : IEntityTypeConfiguration<Collection>
{
    public void Configure(EntityTypeBuilder<Collection> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name).HasMaxLength(100).IsRequired();
        builder.Property(c => c.Description).HasMaxLength(1000);

        builder.HasOne(c => c.User)
            .WithMany(u => u.Collections)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Many-to-Many с публикациями
        builder.HasMany(c => c.Publications)
            .WithMany(p => p.Collections)
            .UsingEntity<Dictionary<string, object>>(
                "CollectionPublications",
                j => j.HasOne<Publication>().WithMany().HasForeignKey("PublicationId").OnDelete(DeleteBehavior.Restrict),
                j => j.HasOne<Collection>().WithMany().HasForeignKey("CollectionId").OnDelete(DeleteBehavior.Restrict));

        builder.Navigation(c => c.Publications).Metadata.SetField("_publications");
    }
}
