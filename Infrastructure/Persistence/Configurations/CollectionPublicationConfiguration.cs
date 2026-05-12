using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class CollectionPublicationConfiguration : IEntityTypeConfiguration<CollectionPublication>
{
    public void Configure(EntityTypeBuilder<CollectionPublication> builder)
    {
        builder.ToTable("CollectionPublications");

        builder.HasKey(cp => new { cp.CollectionId, cp.PublicationId });

        builder.Property(cp => cp.CollectionId).HasField("_collectionId");
        builder.Property(cp => cp.PublicationId).HasField("_publicationId");
        builder.Property(cp => cp.AddedAt).HasField("_addedAt");

        builder.HasOne(cp => cp.Collection)
            .WithMany(c => c.CollectionPublications)
            .HasForeignKey(cp => cp.CollectionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cp => cp.Publication)
            .WithMany(p => p.CollectionPublications)
            .HasForeignKey(cp => cp.PublicationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(cp => cp.Collection).Metadata.SetField("_collection");
        builder.Navigation(cp => cp.Publication).Metadata.SetField("_publication");
    }
}
