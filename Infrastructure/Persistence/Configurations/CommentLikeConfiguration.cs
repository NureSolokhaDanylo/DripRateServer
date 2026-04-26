using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class CommentLikeConfiguration : IEntityTypeConfiguration<CommentLike>
{
    public void Configure(EntityTypeBuilder<CommentLike> builder)
    {
        builder.HasKey(cl => new { cl.UserId, cl.CommentId });
        builder.Property(cl => cl.UserId).HasField("_userId");
        builder.Property(cl => cl.CommentId).HasField("_commentId");
        builder.Property(cl => cl.CreatedAt).HasField("_createdAt");

        builder.HasIndex(cl => cl.CommentId);

        builder.HasOne(cl => cl.Comment)
            .WithMany(c => c.Likes)
            .HasForeignKey(cl => cl.CommentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(cl => cl.User)
            .WithMany()
            .HasForeignKey(cl => cl.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(cl => cl.Comment).Metadata.SetField("_comment");
        builder.Navigation(cl => cl.User).Metadata.SetField("_user");
    }
}
