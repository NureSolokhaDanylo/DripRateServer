using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class ReportConfiguration : IEntityTypeConfiguration<Report>
{
    public void Configure(EntityTypeBuilder<Report> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasField("_id")
            .ValueGeneratedNever();

        builder.Property(r => r.Category)
            .HasField("_category")
            .HasConversion<string>();

        builder.Property(r => r.TargetType)
            .HasField("_targetType")
            .HasConversion<string>();

        builder.Property(r => r.Status)
            .HasField("_status")
            .HasConversion<string>();

        builder.Property(r => r.Text)
            .HasField("_text")
            .HasMaxLength(1000);

        builder.Property(r => r.CreatedAt)
            .HasField("_createdAt");

        builder.Property(r => r.AssignedAt)
            .HasField("_assignedAt");

        builder.Property(r => r.ResolvedAt)
            .HasField("_resolvedAt");

        builder.Property(r => r.AuthorId)
            .HasField("_authorId");

        builder.Property(r => r.TargetId)
            .HasField("_targetId");

        builder.Property(r => r.AssignedToUserId)
            .HasField("_assignedToUserId");

        builder.HasOne(r => r.Author)
            .WithMany()
            .HasForeignKey(r => r.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.AssignedToUser)
            .WithMany()
            .HasForeignKey(r => r.AssignedToUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => r.TargetId);
        builder.HasIndex(r => r.Status);
        builder.HasIndex(r => r.CreatedAt);

        builder.HasQueryFilter(r => !r.Author.IsBanned);
    }
}
