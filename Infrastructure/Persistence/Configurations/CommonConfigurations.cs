using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class ClothConfiguration : IEntityTypeConfiguration<Cloth>
{
    public void Configure(EntityTypeBuilder<Cloth> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasField("_id");
        builder.Property(c => c.Name).HasMaxLength(200).IsRequired().HasField("_name");
        builder.Property(c => c.Brand).HasMaxLength(100).HasField("_brand");
        builder.Property(c => c.PhotoUrl).HasMaxLength(2048).HasField("_photoUrl");
        builder.Property(c => c.StoreLink).HasMaxLength(2048).HasField("_storeLink");
        builder.Property(c => c.EstimatedPrice).HasPrecision(18, 2).HasField("_estimatedPrice");
        builder.Property(c => c.UserId).HasField("_userId");

        builder.HasOne(c => c.User)
            .WithMany(u => u.Wardrobe)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(c => c.User).Metadata.SetField("_user");
    }
}

public sealed class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasField("_id");
        builder.Property(t => t.Name).HasMaxLength(50).IsRequired().HasField("_name");
        builder.Property(t => t.Category).HasMaxLength(50).IsRequired().HasField("_category");
        builder.HasIndex(t => t.Name).IsUnique();
        builder.HasIndex(t => t.Category);
    }
}

public sealed class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasField("_id");
        builder.Property(c => c.Text).HasMaxLength(1000).IsRequired().HasField("_text");
        builder.Property(c => c.CreatedAt).HasField("_createdAt");
        builder.Property(c => c.UserId).HasField("_userId");
        builder.Property(c => c.PublicationId).HasField("_publicationId");
        builder.Property(c => c.ParentCommentId).HasField("_parentCommentId");
        
        // Убираем каскад от пользователя, оставляем только от публикации
        builder.HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Publication)
            .WithMany(p => p.Comments)
            .HasForeignKey(c => c.PublicationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.ParentComment)
            .WithMany(c => c.Replies)
            .HasForeignKey(c => c.ParentCommentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Likes)
            .WithOne(cl => cl.Comment)
            .HasForeignKey(cl => cl.CommentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(c => c.User).Metadata.SetField("_user");
        builder.Navigation(c => c.Publication).Metadata.SetField("_publication");
        builder.Navigation(c => c.ParentComment).Metadata.SetField("_parentComment");
        builder.Navigation(c => c.Replies).Metadata.SetField("_replies");
        builder.Navigation(c => c.Likes).Metadata.SetField("_likes");
    }
}

public sealed class AssessmentConfiguration : IEntityTypeConfiguration<Assessment>
{
    public void Configure(EntityTypeBuilder<Assessment> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasField("_id");
        builder.Property(a => a.ColorCoordination).HasField("_colorCoordination");
        builder.Property(a => a.FitAndProportions).HasField("_fitAndProportions");
        builder.Property(a => a.Originality).HasField("_originality");
        builder.Property(a => a.OverallStyle).HasField("_overallStyle");
        builder.Property(a => a.UserId).HasField("_userId");
        builder.Property(a => a.PublicationId).HasField("_publicationId");
        
        builder.HasIndex(a => new { a.UserId, a.PublicationId }).IsUnique();

        // Убираем каскад от пользователя, оставляем только от публикации
        builder.HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Publication)
            .WithMany(p => p.Assessments)
            .HasForeignKey(a => a.PublicationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(a => a.User).Metadata.SetField("_user");
        builder.Navigation(a => a.Publication).Metadata.SetField("_publication");
    }
}

public sealed class FollowConfiguration : IEntityTypeConfiguration<Follow>
{
    public void Configure(EntityTypeBuilder<Follow> builder)
    {
        builder.HasKey(f => new { f.FollowerId, f.FolloweeId });
        builder.Property(f => f.FollowerId).HasField("_followerId");
        builder.Property(f => f.FolloweeId).HasField("_followeeId");

        builder.HasOne(f => f.Follower)
            .WithMany(u => u.Following)
            .HasForeignKey(f => f.FollowerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(f => f.Followee)
            .WithMany(u => u.Followers)
            .HasForeignKey(f => f.FolloweeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(f => f.Follower).Metadata.SetField("_follower");
        builder.Navigation(f => f.Followee).Metadata.SetField("_followee");
    }
}
