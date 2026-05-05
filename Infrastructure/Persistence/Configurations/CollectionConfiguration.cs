using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class CollectionConfiguration : IEntityTypeConfiguration<Collection>
{
    public void Configure(EntityTypeBuilder<Collection> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasField("_id");

        builder.Property(c => c.Name).HasMaxLength(100).IsRequired().HasField("_name");
        builder.Property(c => c.Description).HasMaxLength(1000).HasField("_description");
        builder.Property(c => c.IsPublic).HasField("_isPublic");
        builder.Property(c => c.IsSystem).HasField("_isSystem");
        builder.Property(c => c.Type).HasField("_type");
        builder.Property(c => c.CreatedAt).HasField("_createdAt");
        builder.Property(c => c.UserId).HasField("_userId");

        builder.HasIndex(c => c.UserId);

        // Composites for performance
        builder.HasIndex(c => new { c.IsPublic, c.CreatedAt, c.Id }).IsDescending(false, true, false);

        builder.HasOne(c => c.User)
            .WithMany(u => u.Collections)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Many-to-Many с публикациями
        builder.HasMany(c => c.Publications)
            .WithMany(p => p.Collections)
            .UsingEntity<Dictionary<string, object>>(
                "CollectionPublication",
                j => j.HasOne<Publication>().WithMany().HasForeignKey("PublicationId").OnDelete(DeleteBehavior.Restrict),
                j => j.HasOne<Collection>().WithMany().HasForeignKey("CollectionId").OnDelete(DeleteBehavior.Restrict),
                j =>
                {
                    j.ToTable("CollectionPublications");
                    j.HasIndex("PublicationId");
                });

        builder.Navigation(c => c.User).Metadata.SetField("_user");
        builder.Navigation(c => c.Publications).Metadata.SetField("_publications");
    }
}
